using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;
using Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

public sealed class SqliteScheduleStore :
    IScheduleWorkspaceReader,
    IOpenScheduleDraftStore,
    IChangeScheduleDayStore,
    IPlanningInputReader,
    IPreparePlanningSnapshotStore,
    IAcceptAutomaticScheduleProposalStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _databasePath;
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteScheduleStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _databasePath = Path.GetFullPath(databasePath);
        _contextFactory = new ServiceCatalogDbContextFactory(_databasePath);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await context.Database.MigrateAsync(cancellationToken);
    }

    async Task<ScheduleWorkspaceReadData> IScheduleWorkspaceReader.LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        PlanningInputReadData data = await LoadCoreAsync(period, cancellationToken);
        return data.Workspace;
    }

    Task<PlanningInputReadData> IPlanningInputReader.LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        return LoadCoreAsync(period, cancellationToken);
    }

    public async Task<OpenScheduleDraftStoreResult> CreateAsync(
        ScheduleDraft draft,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(draft);
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        context.ScheduleDrafts.Add(CreateDraftEntity(draft));
        context.SchedulePeriodDays.AddRange(Enumerable.Range(0, 21).Select(index =>
            new SchedulePeriodDayEntity
            {
                DraftId = draft.Id.Value,
                Date = draft.Period.StartMonday.AddDays(index),
            }));
        AddDraftChildren(context, draft);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return OpenScheduleDraftStoreResult.Success(draft);
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            SchedulePeriod? overlap = await FindOverlapAsync(
                draft.Period,
                cancellationToken);
            return overlap is null
                ? OpenScheduleDraftStoreResult.Conflict()
                : OpenScheduleDraftStoreResult.Overlap(overlap);
        }
    }

    public async Task<ScheduleDayChangeStoreResult> ChangeAsync(
        ScheduleDayChange change,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(change);
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        ScheduleDraftEntity? current = await context.ScheduleDrafts.SingleOrDefaultAsync(
            entity => entity.Id == change.UpdatedDraft.Id.Value,
            cancellationToken);
        if (current is null
            || current.Version != change.ExpectedDraftVersion
            || current.StartMonday != change.UpdatedDraft.Period.StartMonday
            || current.EndSunday != change.UpdatedDraft.Period.EndSunday)
        {
            return ScheduleDayChangeStoreResult.Conflict();
        }

        bool availabilityMatches = await ApplyAvailabilityMutationAsync(
            context,
            change.AvailabilityMutation,
            cancellationToken);
        if (!availabilityMatches)
        {
            return ScheduleDayChangeStoreResult.Conflict();
        }

        await DeleteDraftChildrenAsync(context, current.Id, cancellationToken);
        current.Version = change.UpdatedDraft.Version.Value;
        AddDraftChildren(context, change.UpdatedDraft);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ScheduleDayChangeStoreResult.Success(change.UpdatedDraft);
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return ScheduleDayChangeStoreResult.Conflict();
        }
    }

    public async Task<PreparePlanningSnapshotStoreResult> SaveAsync(
        PreparePlanningSnapshotChange change,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(change);
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        ScheduleDraftEntity? draft = await context.ScheduleDrafts.SingleOrDefaultAsync(
            entity => entity.Id == change.Snapshot.DraftId,
            cancellationToken);
        if (draft is null
            || draft.Version != change.ExpectedDraftVersion
            || draft.PreparedSnapshotId != change.ExpectedPreviousSnapshotId)
        {
            return PreparePlanningSnapshotStoreResult.Conflict();
        }

        Guid? previousId = draft.PreparedSnapshotId;
        draft.PreparedSnapshotId = null;
        if (previousId is not null)
        {
            await context.PlanningSnapshotComponents
                .Where(entity => entity.SnapshotId == previousId.Value)
                .ExecuteDeleteAsync(cancellationToken);
            await context.PlanningSnapshots
                .Where(entity => entity.Id == previousId.Value)
                .ExecuteDeleteAsync(cancellationToken);
        }

        PlanningSnapshotEntity snapshotEntity = CreateSnapshotEntity(change.Snapshot);
        context.PlanningSnapshots.Add(snapshotEntity);
        context.PlanningSnapshotComponents.AddRange(
            CreateSnapshotComponents(change.Snapshot));
        draft.PreparedSnapshotId = change.Snapshot.Id;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return PreparePlanningSnapshotStoreResult.Success(change.Snapshot);
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return PreparePlanningSnapshotStoreResult.Conflict();
        }
    }

    public async Task<AcceptAutomaticScheduleProposalStoreResult> AcceptAsync(
        AcceptAutomaticScheduleProposalChange change,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(change);
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        ScheduleDraftEntity? current = await context.ScheduleDrafts.SingleOrDefaultAsync(
            entity => entity.Id == change.UpdatedDraft.Id.Value,
            cancellationToken);
        if (current is null
            || current.Version != change.ExpectedDraftVersion
            || current.PreparedSnapshotId != change.ExpectedSnapshotId
            || current.StartMonday != change.UpdatedDraft.Period.StartMonday
            || current.EndSunday != change.UpdatedDraft.Period.EndSunday
            || change.UpdatedDraft.Version.Value != change.ExpectedDraftVersion + 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return AcceptAutomaticScheduleProposalStoreResult.Conflict();
        }

        Guid[] lockedIds = await context.ScheduleAssignmentLocks
            .Where(entity => entity.DraftId == current.Id)
            .Select(entity => entity.AssignmentId)
            .ToArrayAsync(cancellationToken);
        Guid[] replaceableIds = await context.ScheduleAssignments
            .Where(entity => entity.DraftId == current.Id)
            .Where(entity => entity.Origin == (int)AssignmentOrigin.AutomaticGeneration)
            .Where(entity => !lockedIds.Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToArrayAsync(cancellationToken);
        await context.ScheduleDemandCoverages
            .Where(entity => replaceableIds.Contains(entity.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignmentSegments
            .Where(entity => replaceableIds.Contains(entity.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignments
            .Where(entity => replaceableIds.Contains(entity.Id))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleGeneratedDaysOff
            .Where(entity => entity.DraftId == current.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await context.AutomaticScheduleRuns
            .Where(entity => entity.DraftId == current.Id)
            .ExecuteDeleteAsync(cancellationToken);

        HashSet<ScheduleAssignmentId> updatedLockedIds = change.UpdatedDraft.AssignmentLocks
            .Select(assignmentLock => assignmentLock.AssignmentId)
            .ToHashSet();
        AddAssignments(
            context,
            change.UpdatedDraft.Id.Value,
            change.UpdatedDraft.Assignments.Where(assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration
                && !updatedLockedIds.Contains(assignment.Id)));
        AddGeneratedDaysOff(context, change.UpdatedDraft);
        context.AutomaticScheduleRuns.Add(CreateAutomaticRunEntity(
            change.UpdatedDraft.Id.Value,
            change.Run));
        current.Version = change.UpdatedDraft.Version.Value;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return AcceptAutomaticScheduleProposalStoreResult.Success(
                change.UpdatedDraft);
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return AcceptAutomaticScheduleProposalStoreResult.Conflict();
        }
    }

    private async Task<PlanningInputReadData> LoadCoreAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<WorkLocationEntity> workLocations = await context.WorkLocations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftTypeEntity> shiftTypes = await context.ShiftTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftPatternEntity> shiftPatterns = await context.ShiftPatterns
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        ServiceCatalogData serviceCatalog = SqliteServiceCatalogStore.CreateData(
            workLocations,
            shiftTypes,
            shiftPatterns);
        List<EmployeeTypeEntity> employeeTypeEntities = await context.EmployeeTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<EmployeeTypeShiftEligibilityEntity> eligibilityEntities = await context
            .EmployeeTypeShiftEligibilities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        EmployeeType[] employeeTypes = SqliteEmployeeStore.MapEmployeeTypes(
            employeeTypeEntities,
            eligibilityEntities,
            serviceCatalog);
        Employee[] employees = (await context.Employees
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .Select(SqliteEmployeeStore.MapEmployee)
            .ToArray();
        AvailabilityEntryReadItem[] availabilityEntries = (await context.AvailabilityEntries
                .AsNoTracking()
                .Where(entity => entity.Date >= period.StartMonday)
                .Where(entity => entity.Date <= period.EndSunday)
                .ToListAsync(cancellationToken))
            .Select(SqliteAvailabilityStore.MapEntry)
            .ToArray();
        StandardStaffingDemandRevision[] revisions = (await context
                .StandardStaffingDemandRevisions
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .Select(SqliteStaffingDemandStore.MapStandardRevision)
            .ToArray();
        StaffingDemandDateException[] exceptions = (await context
                .StaffingDemandDateExceptions
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .Select(SqliteStaffingDemandStore.MapDateException)
            .ToArray();
        ScheduleDraftEntity[] draftEntities = await context.ScheduleDrafts
            .AsNoTracking()
            .OrderBy(entity => entity.StartMonday)
            .ToArrayAsync(cancellationToken);
        ScheduleDraftHeader[] headers = draftEntities
            .Select(MapHeader)
            .ToArray();
        ScheduleDraftEntity? exactEntity = draftEntities.SingleOrDefault(entity =>
            entity.StartMonday == period.StartMonday
            && entity.EndSunday == period.EndSunday);
        ScheduleDraft? exactDraft = exactEntity is null
            ? null
            : await LoadDraftAsync(
                context,
                exactEntity,
                serviceCatalog,
                cancellationToken);
        PlanningHistoryDayReadItem[] history = await LoadHistoryAsync(
            context,
            period,
            cancellationToken);
        PlanningInputSnapshot? prepared = exactEntity?.PreparedSnapshotId is null
            ? null
            : await LoadSnapshotAsync(
                context,
                exactEntity.PreparedSnapshotId.Value,
                cancellationToken);
        AutomaticScheduleRunRecord? automaticRun = exactEntity is null
            ? null
            : await LoadAutomaticRunAsync(
                context,
                exactEntity.Id,
                cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        ScheduleWorkspaceReadData workspace = new(
            new AvailabilityReadData(employees, employeeTypes, availabilityEntries),
            new StaffingDemandReadData(revisions, exceptions, serviceCatalog),
            headers,
            exactDraft,
            history,
            prepared,
            automaticRun);
        return new PlanningInputReadData(workspace, history, prepared);
    }

    private static async Task<ScheduleDraft> LoadDraftAsync(
        ServiceCatalogDbContext context,
        ScheduleDraftEntity entity,
        ServiceCatalogData serviceCatalog,
        CancellationToken cancellationToken)
    {
        SchedulePeriod period = SchedulePeriod.Create(entity.StartMonday).Value
            ?? throw InvalidStored("schedule period", entity.Id);
        if (period.EndSunday != entity.EndSunday)
        {
            throw InvalidStored("schedule period", entity.Id);
        }

        List<ScheduleDemandSlotEntity> slotEntities = await context.ScheduleDemandSlots
            .AsNoTracking()
            .Where(item => item.DraftId == entity.Id)
            .OrderBy(item => item.Date)
            .ThenBy(item => item.SlotKey)
            .ToListAsync(cancellationToken);
        DemandSlotSetRestoreResult slotResult = DemandSlotSet.Restore(
            period,
            slotEntities.Select(item => new DemandSlotRestoreValue(
                item.SourceId,
                (StaffingDemandSourceKind)item.SourceKind,
                item.Date,
                item.WorkLocationId,
                item.ShiftTypeId,
                item.Ordinal,
                item.ActualStart,
                item.ActualEnd)),
            serviceCatalog.WorkLocations.Select(location => location.Id),
            serviceCatalog.ShiftTypes.Select(shiftType => shiftType.Id));
        DemandSlotSet slots = slotResult.Value
            ?? throw InvalidStored("demand slots", entity.Id);
        Dictionary<string, DemandSlot> slotsByKey = slots.Slots
            .ToDictionary(slot => CreateSlotKey(slot.Id));

        AvailabilityEntrySet availability = AvailabilityEntrySet.Create(
            (await context.ScheduleAvailabilityEntries
                .AsNoTracking()
                .Where(item => item.DraftId == entity.Id)
                .ToListAsync(cancellationToken))
            .Select(item => AvailabilityEntry.Create(
                    item.EmployeeId,
                    item.Date,
                    (AvailabilityEntryKind)item.Kind).Value
                ?? throw InvalidStored("availability entry", entity.Id))).Value
            ?? throw InvalidStored("availability entries", entity.Id);
        List<ScheduleAssignmentEntity> assignmentEntities = await context
            .ScheduleAssignments
            .AsNoTracking()
            .Where(item => item.DraftId == entity.Id)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        List<ScheduleAssignmentSegmentEntity> segmentEntities = await context
            .ScheduleAssignmentSegments
            .AsNoTracking()
            .Where(item => item.DraftId == entity.Id)
            .OrderBy(item => item.AssignmentId)
            .ThenBy(item => item.Sequence)
            .ToListAsync(cancellationToken);
        List<ScheduleDemandCoverageEntity> coverageEntities = await context
            .ScheduleDemandCoverages
            .AsNoTracking()
            .Where(item => item.DraftId == entity.Id)
            .OrderBy(item => item.AssignmentId)
            .ThenBy(item => item.Sequence)
            .ToListAsync(cancellationToken);
        ScheduleAssignment[] assignments = assignmentEntities
            .Select(item => MapAssignment(
                item,
                segmentEntities.Where(segment => segment.AssignmentId == item.Id).ToArray(),
                coverageEntities.Where(coverage => coverage.AssignmentId == item.Id).ToArray(),
                slotsByKey,
                serviceCatalog))
            .ToArray();
        GeneratedDayOffMarker[] markers = (await context.ScheduleGeneratedDaysOff
                .AsNoTracking()
                .Where(item => item.DraftId == entity.Id)
                .ToListAsync(cancellationToken))
            .Select(item =>
            {
                EmployeeId.TryCreate(item.EmployeeId, out EmployeeId? employeeId);
                return employeeId is null
                    ? throw InvalidStored("generated day off", entity.Id)
                    : new GeneratedDayOffMarker(employeeId, item.Date);
            })
            .ToArray();
        AssignmentLock[] locks = (await context.ScheduleAssignmentLocks
                .AsNoTracking()
                .Where(item => item.DraftId == entity.Id)
                .ToListAsync(cancellationToken))
            .Select(item =>
            {
                ScheduleAssignmentId.TryCreate(
                    item.AssignmentId,
                    out ScheduleAssignmentId? assignmentId);
                return assignmentId is null
                    ? throw InvalidStored("assignment lock", entity.Id)
                    : new AssignmentLock(assignmentId);
            })
            .ToArray();

        return ScheduleDraft.Create(
                entity.Id,
                entity.Version,
                period,
                slots,
                availability,
                assignments,
                markers,
                locks).Value
            ?? throw InvalidStored("schedule draft", entity.Id);
    }

    private static ScheduleAssignment MapAssignment(
        ScheduleAssignmentEntity entity,
        ScheduleAssignmentSegmentEntity[] segments,
        ScheduleDemandCoverageEntity[] coverages,
        Dictionary<string, DemandSlot> slots,
        ServiceCatalogData catalog)
    {
        if (!EmployeeId.TryCreate(entity.EmployeeId, out EmployeeId? employeeId)
            || segments.Length == 0
            || segments.Any(segment => !slots.ContainsKey(segment.AnchorSlotKey)))
        {
            throw InvalidStored("schedule assignment", entity.Id);
        }

        DemandSlot first = slots[segments[0].AnchorSlotKey];
        DemandSlot? second = segments.Length > 1
            ? slots[segments[1].AnchorSlotKey]
            : null;
        ScheduleAssignmentValidationResult result = (ScheduleAssignmentKind)entity.Kind switch
        {
            ScheduleAssignmentKind.NormalDemand => ScheduleAssignment.CreateNormal(
                entity.Id,
                employeeId,
                first,
                (AssignmentOrigin)entity.Origin),
            ScheduleAssignmentKind.OfficeTime => ScheduleAssignment.CreateOfficeTime(
                entity.Id,
                employeeId,
                first),
            ScheduleAssignmentKind.ManualAdditional =>
                ScheduleAssignment.CreateManualAdditional(entity.Id, employeeId, first),
            ScheduleAssignmentKind.SplitShiftPattern =>
                ScheduleAssignment.CreateSplitShift(
                    entity.Id,
                    employeeId,
                    catalog.SplitShiftPattern,
                    first,
                    second,
                    (AssignmentOrigin)entity.Origin),
            ScheduleAssignmentKind.ReliefShiftPattern =>
                ScheduleAssignment.CreateReliefShift(
                    entity.Id,
                    employeeId,
                    catalog.ReliefShiftPattern,
                    first,
                    second,
                    (AssignmentOrigin)entity.Origin),
            _ => throw InvalidStored("schedule assignment kind", entity.Id),
        };
        ScheduleAssignment assignment = result.Value
            ?? throw InvalidStored("schedule assignment", entity.Id);
        if (!AssignmentMatches(entity, segments, coverages, assignment))
        {
            throw InvalidStored("schedule assignment details", entity.Id);
        }

        return assignment;
    }

    private static bool AssignmentMatches(
        ScheduleAssignmentEntity entity,
        ScheduleAssignmentSegmentEntity[] segments,
        ScheduleDemandCoverageEntity[] coverages,
        ScheduleAssignment assignment)
    {
        return entity.Date == assignment.Date
            && entity.PatternId == assignment.PatternId?.Value
            && entity.WorkMinutes == assignment.WorkMinutes
            && entity.IsProtectedFromAutomaticGeneration
                == assignment.IsProtectedFromAutomaticGeneration
            && segments.Length == assignment.Segments.Count
            && segments.Zip(assignment.Segments).All(pair =>
                pair.First.Date == pair.Second.Date
                && pair.First.WorkLocationId == pair.Second.WorkLocationId.Value
                && pair.First.ShiftTypeId == pair.Second.ShiftTypeId.Value
                && pair.First.ActualStart == pair.Second.ActualTime.Start
                && pair.First.ActualEnd == pair.Second.ActualTime.End
                && pair.First.WorkMinutes == pair.Second.WorkMinutes)
            && coverages.Length == assignment.Coverages.Count
            && coverages.Zip(assignment.Coverages).All(pair =>
                pair.First.CoveredStart == pair.Second.CoveredTime.Start
                && pair.First.CoveredEnd == pair.Second.CoveredTime.End
                && pair.First.CoveredMinutes == pair.Second.CoveredMinutes
                && pair.First.Kind == (int)pair.Second.Kind);
    }

    private static ScheduleDraftHeader MapHeader(ScheduleDraftEntity entity)
    {
        ScheduleDraftId.TryCreate(entity.Id, out ScheduleDraftId? id);
        ScheduleDraftVersion.TryCreate(entity.Version, out ScheduleDraftVersion? version);
        SchedulePeriod? period = SchedulePeriod.Create(entity.StartMonday).Value;
        return id is null || version is null || period is null || period.EndSunday != entity.EndSunday
            ? throw InvalidStored("schedule header", entity.Id)
            : new ScheduleDraftHeader(id, version, period);
    }

    private static async Task<PlanningHistoryDayReadItem[]> LoadHistoryAsync(
        ServiceCatalogDbContext context,
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        DateOnly firstDate;
        try
        {
            firstDate = period.StartMonday.AddDays(-7);
        }
        catch (ArgumentOutOfRangeException)
        {
            return [];
        }

        DateOnly lastDate = period.StartMonday.AddDays(-1);
        DateOnly[] availableDates = await context.SchedulePeriodDays
            .AsNoTracking()
            .Where(item => item.Date >= firstDate && item.Date <= lastDate)
            .OrderBy(item => item.Date)
            .Select(item => item.Date)
            .ToArrayAsync(cancellationToken);
        List<ScheduleAssignmentEntity> assignments = await context.ScheduleAssignments
            .AsNoTracking()
            .Where(item => item.Date >= firstDate && item.Date <= lastDate)
            .ToListAsync(cancellationToken);

        return availableDates.Select(date => new PlanningHistoryDayReadItem(
            date,
            assignments
                .Where(assignment => assignment.Date == date)
                .OrderBy(assignment => assignment.EmployeeId)
                .Select(assignment => new PlanningHistoryAssignmentReadItem(
                    assignment.EmployeeId,
                    assignment.WorkMinutes))))
            .ToArray();
    }

    private static async Task<PlanningInputSnapshot> LoadSnapshotAsync(
        ServiceCatalogDbContext context,
        Guid snapshotId,
        CancellationToken cancellationToken)
    {
        PlanningSnapshotEntity entity = await context.PlanningSnapshots
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == snapshotId, cancellationToken)
            ?? throw InvalidStored("planning snapshot", snapshotId);
        List<PlanningSnapshotComponentEntity> components = await context
            .PlanningSnapshotComponents
            .AsNoTracking()
            .Where(item => item.SnapshotId == snapshotId)
            .OrderBy(item => item.Kind)
            .ThenBy(item => item.Sequence)
            .ToListAsync(cancellationToken);
        T[] Read<T>(PlanningSnapshotComponentKind kind) => components
            .Where(item => item.Kind == (int)kind)
            .Select(item => JsonSerializer.Deserialize<T>(item.Payload, JsonOptions)
                ?? throw InvalidStored("planning snapshot component", snapshotId))
            .ToArray();

        PlanningWorkLocationSnapshot[] locations = Read<PlanningWorkLocationSnapshot>(
            PlanningSnapshotComponentKind.WorkLocation);
        PlanningShiftTypeSnapshot[] shifts = Read<PlanningShiftTypeSnapshot>(
            PlanningSnapshotComponentKind.ShiftType);
        PlanningSplitShiftPatternSnapshot split = AssertSingle(
            Read<PlanningSplitShiftPatternSnapshot>(
                PlanningSnapshotComponentKind.SplitShiftPattern),
            snapshotId);
        PlanningReliefShiftPatternSnapshot relief = AssertSingle(
            Read<PlanningReliefShiftPatternSnapshot>(
                PlanningSnapshotComponentKind.ReliefShiftPattern),
            snapshotId);
        RuleCatalogSnapshot rules = await LoadRulesAsync(
            entity.RuleCatalogVersion,
            Read<StoredRuleDefinition>(PlanningSnapshotComponentKind.RuleDefinition),
            snapshotId,
            cancellationToken);

        return new PlanningInputSnapshot(
            entity.Id,
            entity.DraftId,
            entity.DraftVersion,
            entity.PeriodMonday,
            entity.PeriodSunday,
            Read<PlanningEmployeeSnapshot>(PlanningSnapshotComponentKind.Employee),
            Read<StoredPlanningEmployeeType>(PlanningSnapshotComponentKind.EmployeeType)
                .Select(CreateEmployeeTypeSnapshot),
            new PlanningServiceCatalogSnapshot(locations, shifts, split, relief),
            Read<PlanningAvailabilityEntrySnapshot>(
                PlanningSnapshotComponentKind.AvailabilityEntry),
            Read<ScheduleDemandSlotSnapshot>(PlanningSnapshotComponentKind.DemandSlot),
            Read<StoredScheduleAssignment>(
                    PlanningSnapshotComponentKind.ServiceManagementAssignment)
                .Select(CreateAssignmentSnapshot),
            rules,
            new PlanningRunOptions(entity.EnableAuxiliaryReliefShift),
            new PlanningHistorySnapshot(
                (PlanningHistoryCompleteness)entity.HistoryCompleteness,
                Read<StoredPlanningHistoryDay>(
                        PlanningSnapshotComponentKind.HistoryDay)
                    .Select(CreateHistoryDaySnapshot)));
    }

    private static async Task<AutomaticScheduleRunRecord?> LoadAutomaticRunAsync(
        ServiceCatalogDbContext context,
        Guid draftId,
        CancellationToken cancellationToken)
    {
        AutomaticScheduleRunEntity? entity = await context.AutomaticScheduleRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.DraftId == draftId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        try
        {
            StoredAutomaticScheduleSetting[] settings = JsonSerializer.Deserialize<
                    StoredAutomaticScheduleSetting[]>(
                    entity.SettingsPayload,
                    JsonOptions)
                ?? throw InvalidStored("automatic schedule settings", draftId);
            StoredAutomaticScheduleObjective storedObjective =
                JsonSerializer.Deserialize<StoredAutomaticScheduleObjective>(
                    entity.ObjectivePayload,
                    JsonOptions)
                ?? throw InvalidStored("automatic schedule objective", draftId);
            AutomaticScheduleRunMetadata metadata = new(
                entity.SolverName,
                entity.SolverVersion,
                (AutomaticSchedulePlanningStatus)entity.ResultStatus,
                TimeSpan.FromTicks(entity.TimeLimitTicks),
                TimeSpan.FromTicks(entity.ModelBuildDurationTicks),
                TimeSpan.FromTicks(checked(
                    entity.OptimizationDurationTicks
                    + entity.LegacyPhaseDurationTicks)),
                TimeSpan.FromTicks(entity.ResultMappingDurationTicks),
                TimeSpan.FromTicks(entity.TotalDurationTicks),
                settings.Select(setting =>
                    new AutomaticScheduleSetting(setting.Key, setting.Value)));
            AutomaticScheduleObjectiveSnapshot objective = new(
                storedObjective.UncoveredEmployeeMinutes,
                storedObjective.FullyUncoveredDemandSlotCount,
                storedObjective.HighPriorityViolations.Select(CreateViolation),
                storedObjective.ReliefShiftAssignmentCount,
                storedObjective.SplitShiftAssignmentCount,
                (storedObjective.AuxiliaryMinimumCases ?? []).Select(value =>
                    new AutomaticScheduleAuxiliaryMinimumCase(
                        value.EmployeeId,
                        value.WeekMonday,
                        value.AssignedMinutes,
                        value.HasEligibleDemand)),
                (storedObjective.RelativeWeeklyTargetCases ?? []).Select(value =>
                    new AutomaticScheduleRelativeWeeklyTargetCase(
                        value.EmployeeId,
                        value.WeekMonday,
                        value.AssignedMinutes,
                        value.TargetMinutes)),
                storedObjective.MediumPriorityViolations.Select(CreateViolation),
                storedObjective.LowPriorityViolations.Select(CreateViolation),
                storedObjective.StabilityViolations.Select(CreateViolation),
                storedObjective.TechnicalTieBreakerKeys);
            return new AutomaticScheduleRunRecord(
                entity.SnapshotId,
                metadata,
                objective);
        }
        catch (Exception exception) when (
            exception is JsonException
                or ArgumentException
                or InvalidOperationException
                or OverflowException)
        {
            throw InvalidStored("automatic schedule run", draftId);
        }
    }

    private static AutomaticScheduleRuleViolation CreateViolation(
        StoredAutomaticScheduleRuleViolation value) => new(
            value.RuleId,
            value.RuleFamily,
            value.Priority,
            value.CaseKey,
            value.Magnitude);

    private static async Task<RuleCatalogSnapshot> LoadRulesAsync(
        int version,
        IReadOnlyList<StoredRuleDefinition> stored,
        Guid snapshotId,
        CancellationToken cancellationToken)
    {
        RuleCatalogSnapshot catalog = await GetRuleCatalogQuery.ExecuteAsync(
            version,
            cancellationToken);
        StoredRuleDefinition[] expected = catalog.Definitions
            .Select(CreateStoredRule)
            .ToArray();
        if (!stored.SequenceEqual(expected))
        {
            throw InvalidStored("planning rule catalog", snapshotId);
        }

        return catalog;
    }

    private static T AssertSingle<T>(IReadOnlyList<T> values, Guid snapshotId)
    {
        return values.Count == 1
            ? values[0]
            : throw InvalidStored("planning snapshot component", snapshotId);
    }

    private static ScheduleDraftEntity CreateDraftEntity(ScheduleDraft draft)
    {
        return new ScheduleDraftEntity
        {
            Id = draft.Id.Value,
            Version = draft.Version.Value,
            StartMonday = draft.Period.StartMonday,
            EndSunday = draft.Period.EndSunday,
        };
    }

    private static void AddDraftChildren(
        ServiceCatalogDbContext context,
        ScheduleDraft draft)
    {
        context.ScheduleDemandSlots.AddRange(draft.DemandSlots.Slots.Select(slot =>
            new ScheduleDemandSlotEntity
            {
                DraftId = draft.Id.Value,
                SlotKey = CreateSlotKey(slot.Id),
                SourceId = slot.Id.SourceId.Value,
                SourceKind = (int)slot.Id.SourceKind,
                Date = slot.Id.Date,
                WorkLocationId = slot.Id.WorkLocationId.Value,
                ShiftTypeId = slot.Id.ShiftTypeId.Value,
                Ordinal = slot.Id.Ordinal,
                ActualStart = slot.ActualTime.Start,
                ActualEnd = slot.ActualTime.End,
            }));
        context.ScheduleAvailabilityEntries.AddRange(
            draft.AvailabilityEntries.Entries.Select(entry =>
                new ScheduleAvailabilityEntryEntity
                {
                    DraftId = draft.Id.Value,
                    EmployeeId = entry.EmployeeId.Value,
                    Date = entry.Date,
                    Kind = (int)entry.Kind,
                }));

        AddAssignments(context, draft.Id.Value, draft.Assignments);
        AddGeneratedDaysOff(context, draft);
        context.ScheduleAssignmentLocks.AddRange(draft.AssignmentLocks.Select(item =>
            new ScheduleAssignmentLockEntity
            {
                DraftId = draft.Id.Value,
                AssignmentId = item.AssignmentId.Value,
            }));
    }

    private static void AddAssignments(
        ServiceCatalogDbContext context,
        Guid draftId,
        IEnumerable<ScheduleAssignment> assignments)
    {
        foreach (ScheduleAssignment assignment in assignments)
        {
            context.ScheduleAssignments.Add(new ScheduleAssignmentEntity
            {
                Id = assignment.Id.Value,
                DraftId = draftId,
                EmployeeId = assignment.EmployeeId.Value,
                Date = assignment.Date,
                Kind = (int)assignment.Kind,
                Origin = (int)assignment.Origin,
                PatternId = assignment.PatternId?.Value,
                WorkMinutes = assignment.WorkMinutes,
                IsProtectedFromAutomaticGeneration =
                    assignment.IsProtectedFromAutomaticGeneration,
            });
            context.ScheduleAssignmentSegments.AddRange(
                assignment.Segments.Select((segment, index) =>
                    new ScheduleAssignmentSegmentEntity
                    {
                        AssignmentId = assignment.Id.Value,
                        Sequence = index,
                        DraftId = draftId,
                        AnchorSlotKey = CreateSlotKey(segment.AnchorSlotId),
                        Date = segment.Date,
                        WorkLocationId = segment.WorkLocationId.Value,
                        ShiftTypeId = segment.ShiftTypeId.Value,
                        ActualStart = segment.ActualTime.Start,
                        ActualEnd = segment.ActualTime.End,
                        WorkMinutes = segment.WorkMinutes,
                    }));
            context.ScheduleDemandCoverages.AddRange(
                assignment.Coverages.Select((coverage, index) =>
                    new ScheduleDemandCoverageEntity
                    {
                        AssignmentId = assignment.Id.Value,
                        Sequence = index,
                        DraftId = draftId,
                        SlotKey = CreateSlotKey(coverage.SlotId),
                        CoveredStart = coverage.CoveredTime.Start,
                        CoveredEnd = coverage.CoveredTime.End,
                        CoveredMinutes = coverage.CoveredMinutes,
                        Kind = (int)coverage.Kind,
                    }));
        }
    }

    private static void AddGeneratedDaysOff(
        ServiceCatalogDbContext context,
        ScheduleDraft draft)
    {
        context.ScheduleGeneratedDaysOff.AddRange(
            draft.GeneratedDayOffMarkers.Select(marker =>
                new ScheduleGeneratedDayOffEntity
                {
                    DraftId = draft.Id.Value,
                    EmployeeId = marker.EmployeeId.Value,
                    Date = marker.Date,
                }));
    }

    private static async Task DeleteDraftChildrenAsync(
        ServiceCatalogDbContext context,
        Guid draftId,
        CancellationToken cancellationToken)
    {
        Guid[] assignmentIds = await context.ScheduleAssignments
            .Where(item => item.DraftId == draftId)
            .Select(item => item.Id)
            .ToArrayAsync(cancellationToken);
        await context.ScheduleAssignmentLocks
            .Where(item => item.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleDemandCoverages
            .Where(item => assignmentIds.Contains(item.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignmentSegments
            .Where(item => assignmentIds.Contains(item.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignments
            .Where(item => item.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAvailabilityEntries
            .Where(item => item.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleGeneratedDaysOff
            .Where(item => item.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleDemandSlots
            .Where(item => item.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task<bool> ApplyAvailabilityMutationAsync(
        ServiceCatalogDbContext context,
        ScheduleAvailabilityMutation mutation,
        CancellationToken cancellationToken)
    {
        if (mutation.Kind == ScheduleAvailabilityMutationKind.None)
        {
            return true;
        }

        AvailabilityEntry expectedEntry = mutation.ExpectedCurrent?.Entry
            ?? mutation.Replacement
            ?? throw new InvalidOperationException("Availability mutation has no key.");
        AvailabilityEntryEntity? current = await context.AvailabilityEntries
            .SingleOrDefaultAsync(
                item => item.EmployeeId == expectedEntry.EmployeeId.Value
                    && item.Date == expectedEntry.Date,
                cancellationToken);
        if (mutation.ExpectedCurrent is null)
        {
            if (current is not null
                || mutation.Kind != ScheduleAvailabilityMutationKind.Upsert
                || mutation.Replacement is null)
            {
                return false;
            }

            context.AvailabilityEntries.Add(new AvailabilityEntryEntity
            {
                EmployeeId = mutation.Replacement.EmployeeId.Value,
                Date = mutation.Replacement.Date,
                Kind = (int)mutation.Replacement.Kind,
                ChangeVersion = 1,
            });
            return true;
        }

        if (current is null
            || current.Kind != (int)mutation.ExpectedCurrent.Entry.Kind
            || current.ChangeVersion != mutation.ExpectedCurrent.ChangeVersion)
        {
            return false;
        }

        if (mutation.Kind == ScheduleAvailabilityMutationKind.Remove)
        {
            context.AvailabilityEntries.Remove(current);
            return true;
        }

        if (mutation.Replacement is null || current.ChangeVersion == long.MaxValue)
        {
            return false;
        }

        current.Kind = (int)mutation.Replacement.Kind;
        current.ChangeVersion++;
        return true;
    }

    private static PlanningSnapshotEntity CreateSnapshotEntity(
        PlanningInputSnapshot snapshot)
    {
        return new PlanningSnapshotEntity
        {
            Id = snapshot.Id,
            DraftId = snapshot.DraftId,
            DraftVersion = checked((int)snapshot.DraftVersion),
            PeriodMonday = snapshot.PeriodMonday,
            PeriodSunday = snapshot.PeriodSunday,
            RuleCatalogVersion = snapshot.RuleCatalog.Version,
            EnableAuxiliaryReliefShift =
                snapshot.RunOptions.EnableAuxiliaryReliefShift,
            HistoryCompleteness = (int)snapshot.History.Completeness,
        };
    }

    private static AutomaticScheduleRunEntity CreateAutomaticRunEntity(
        Guid draftId,
        AutomaticScheduleRunRecord run)
    {
        AutomaticScheduleRunMetadata metadata = run.Metadata;
        return new AutomaticScheduleRunEntity
        {
            DraftId = draftId,
            SnapshotId = run.SnapshotId,
            SolverName = metadata.SolverName,
            SolverVersion = metadata.SolverVersion,
            ResultStatus = (int)metadata.ResultStatus,
            TimeLimitTicks = metadata.TimeLimit.Ticks,
            ModelBuildDurationTicks = metadata.ModelBuildDuration.Ticks,
            OptimizationDurationTicks = metadata.OptimizationDuration.Ticks,
            LegacyPhaseDurationTicks = 0,
            ResultMappingDurationTicks = metadata.ResultMappingDuration.Ticks,
            TotalDurationTicks = metadata.TotalDuration.Ticks,
            SettingsPayload = JsonSerializer.Serialize(
                metadata.Settings.Select(setting =>
                    new StoredAutomaticScheduleSetting(
                        setting.Key,
                        setting.Value)),
                JsonOptions),
            ObjectivePayload = JsonSerializer.Serialize(
                CreateStoredObjective(run.Objective),
                JsonOptions),
        };
    }

    private static StoredAutomaticScheduleObjective CreateStoredObjective(
        AutomaticScheduleObjectiveSnapshot objective) => new(
            objective.UncoveredEmployeeMinutes,
            objective.FullyUncoveredDemandSlotCount,
            objective.HighPriorityViolations.Select(CreateStoredViolation).ToArray(),
            objective.MediumPriorityViolations.Select(CreateStoredViolation).ToArray(),
            objective.LowPriorityViolations.Select(CreateStoredViolation).ToArray(),
            objective.StabilityViolations.Select(CreateStoredViolation).ToArray(),
            objective.TechnicalTieBreakerKeys.ToArray(),
            objective.ReliefShiftAssignmentCount,
            objective.SplitShiftAssignmentCount,
            objective.AuxiliaryMinimumCases.Select(value =>
                new StoredAutomaticScheduleAuxiliaryMinimumCase(
                    value.EmployeeId,
                    value.WeekMonday,
                    value.AssignedMinutes,
                    value.HasEligibleDemand)).ToArray(),
            objective.RelativeWeeklyTargetCases.Select(value =>
                new StoredAutomaticScheduleRelativeWeeklyTargetCase(
                    value.EmployeeId,
                    value.WeekMonday,
                    value.AssignedMinutes,
                    value.TargetMinutes)).ToArray());

    private static StoredAutomaticScheduleRuleViolation CreateStoredViolation(
        AutomaticScheduleRuleViolation value) => new(
            value.RuleId,
            value.RuleFamily,
            value.Priority,
            value.CaseKey,
            value.Magnitude);

    private static List<PlanningSnapshotComponentEntity>
        CreateSnapshotComponents(PlanningInputSnapshot snapshot)
    {
        List<PlanningSnapshotComponentEntity> entities = [];
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.Employee,
            snapshot.Employees);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.EmployeeType,
            snapshot.EmployeeTypes.Select(CreateStoredEmployeeType));
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.WorkLocation,
            snapshot.ServiceCatalog.WorkLocations);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.ShiftType,
            snapshot.ServiceCatalog.ShiftTypes);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.SplitShiftPattern,
            [snapshot.ServiceCatalog.SplitShiftPattern]);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.ReliefShiftPattern,
            [snapshot.ServiceCatalog.ReliefShiftPattern]);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.AvailabilityEntry,
            snapshot.AvailabilityEntries);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.DemandSlot,
            snapshot.DemandSlots);
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.ServiceManagementAssignment,
            snapshot.ServiceManagementAssignments.Select(CreateStoredAssignment));
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.RuleDefinition,
            snapshot.RuleCatalog.Definitions.Select(CreateStoredRule));
        AddComponents(
            entities,
            snapshot.Id,
            PlanningSnapshotComponentKind.HistoryDay,
            snapshot.History.Days.Select(CreateStoredHistoryDay));
        return entities;
    }

    private static StoredPlanningEmployeeType CreateStoredEmployeeType(
        PlanningEmployeeTypeSnapshot value)
    {
        return new StoredPlanningEmployeeType(
            value.Id,
            value.Code,
            value.Name,
            value.WeeklyWorkTargetMinutes,
            value.AllowsVacationAndSickness,
            value.AbsenceDayValueMinutes,
            value.PlanningRole,
            value.AllowsAutomaticAssignment,
            value.RequiresWeeklyManualAssignment,
            value.PreservesManualAssignmentsOnGeneration,
            value.ManualSuggestionPriority,
            value.Eligibilities.ToArray());
    }

    private static PlanningEmployeeTypeSnapshot CreateEmployeeTypeSnapshot(
        StoredPlanningEmployeeType value)
    {
        return new PlanningEmployeeTypeSnapshot(
            value.Id,
            value.Code,
            value.Name,
            value.WeeklyWorkTargetMinutes,
            value.AllowsVacationAndSickness,
            value.AbsenceDayValueMinutes,
            value.PlanningRole,
            value.AllowsAutomaticAssignment,
            value.RequiresWeeklyManualAssignment,
            value.PreservesManualAssignmentsOnGeneration,
            value.ManualSuggestionPriority,
            value.Eligibilities);
    }

    private static StoredScheduleAssignment CreateStoredAssignment(
        ScheduleAssignmentSnapshot value)
    {
        return new StoredScheduleAssignment(
            value.AssignmentId,
            value.EmployeeId,
            value.Date,
            value.Kind,
            value.Origin,
            value.PatternId,
            value.WorkMinutes,
            value.IsProtectedFromAutomaticGeneration,
            value.Segments.ToArray(),
            value.Coverages.ToArray());
    }

    private static ScheduleAssignmentSnapshot CreateAssignmentSnapshot(
        StoredScheduleAssignment value)
    {
        return new ScheduleAssignmentSnapshot(
            value.AssignmentId,
            value.EmployeeId,
            value.Date,
            value.Kind,
            value.Origin,
            value.PatternId,
            value.WorkMinutes,
            value.IsProtectedFromAutomaticGeneration,
            value.Segments,
            value.Coverages);
    }

    private static StoredPlanningHistoryDay CreateStoredHistoryDay(
        PlanningHistoryDaySnapshot value)
    {
        return new StoredPlanningHistoryDay(
            value.Date,
            value.Status,
            value.Assignments.ToArray());
    }

    private static PlanningHistoryDaySnapshot CreateHistoryDaySnapshot(
        StoredPlanningHistoryDay value)
    {
        return new PlanningHistoryDaySnapshot(
            value.Date,
            value.Status,
            value.Assignments);
    }

    private static void AddComponents<T>(
        List<PlanningSnapshotComponentEntity> target,
        Guid snapshotId,
        PlanningSnapshotComponentKind kind,
        IEnumerable<T> values)
    {
        int sequence = 0;
        foreach (T value in values)
        {
            target.Add(new PlanningSnapshotComponentEntity
            {
                SnapshotId = snapshotId,
                Kind = (int)kind,
                Sequence = sequence++,
                Payload = JsonSerializer.Serialize(value, JsonOptions),
            });
        }
    }

    private static StoredRuleDefinition CreateStoredRule(
        RuleDefinitionSnapshot definition)
    {
        Type parameterType = definition.Parameters.GetType();
        return new StoredRuleDefinition(
            definition.Id,
            (int)definition.Family,
            (int)definition.Scope,
            (int)definition.AutomaticEffect,
            (int)definition.ManualEffect,
            definition.Priority is null ? null : (int)definition.Priority.Value,
            parameterType.FullName
                ?? throw new InvalidOperationException("Rule parameter type has no name."),
            JsonSerializer.Serialize(
                definition.Parameters,
                parameterType,
                JsonOptions),
            definition.DescriptionKey);
    }

    private async Task<SchedulePeriod?> FindOverlapAsync(
        SchedulePeriod requested,
        CancellationToken cancellationToken)
    {
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        ScheduleDraftEntity? overlap = await context.ScheduleDrafts
            .AsNoTracking()
            .Where(item => item.StartMonday <= requested.EndSunday)
            .Where(item => item.EndSunday >= requested.StartMonday)
            .OrderBy(item => item.StartMonday)
            .FirstOrDefaultAsync(cancellationToken);
        return overlap is null
            ? null
            : SchedulePeriod.Create(overlap.StartMonday).Value;
    }

    private static string CreateSlotKey(DemandSlotId slotId)
    {
        return string.Join(
            ':',
            slotId.SourceId.Value.ToString("N"),
            ((int)slotId.SourceKind).ToString(System.Globalization.CultureInfo.InvariantCulture),
            slotId.Date.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
            slotId.WorkLocationId.Value.ToString("N"),
            slotId.ShiftTypeId.Value.ToString("N"),
            slotId.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static InvalidDataException InvalidStored(string kind, Guid id)
    {
        return new InvalidDataException($"Stored {kind} '{id:D}' is invalid.");
    }

    private static bool IsConstraintViolation(Exception exception)
    {
        SqliteException? sqliteException = exception as SqliteException
            ?? exception.InnerException as SqliteException;
        return sqliteException?.SqliteErrorCode == 19;
    }
}
