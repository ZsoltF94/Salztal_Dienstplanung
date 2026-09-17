using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class GetScheduleWorkspaceQuery
{
    private readonly IScheduleWorkspaceReader _reader;

    public GetScheduleWorkspaceQuery(IScheduleWorkspaceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<ScheduleWorkspaceQueryResult> ExecuteAsync(
        DateOnly selectedDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SchedulePeriodValidationResult periodResult =
            SchedulePeriodSelection.Create(selectedDate);
        if (!periodResult.IsSuccess)
        {
            return ScheduleWorkspaceQueryResult.Failure(
                ScheduleWorkspaceQueryStatus.ValidationFailed,
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.PeriodDoesNotFit,
                    "Für das ausgewählte Datum kann kein vollständiger Drei-Wochen-Zeitraum gebildet werden."));
        }

        SchedulePeriod period = periodResult.Value!;
        ScheduleWorkspaceReadData data = await _reader.LoadAsync(
            period,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        ScheduleDraftHeader[] exactHeaders = data.DraftHeaders
            .Where(header => header.Period == period)
            .ToArray();
        if (exactHeaders.Length == 0 || data.ExactDraft is null)
        {
            return ScheduleWorkspaceQueryResult.Failure(
                ScheduleWorkspaceQueryStatus.NotFound,
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.DraftNotFound,
                    "Für den ausgewählten Zeitraum ist noch kein Entwurf vorhanden."));
        }

        if (exactHeaders.Length > 1
            || data.ExactDraft.Id != exactHeaders[0].DraftId
            || data.ExactDraft.Version != exactHeaders[0].Version
            || data.ExactDraft.Period != period)
        {
            return ScheduleWorkspaceQueryResult.Failure(
                ScheduleWorkspaceQueryStatus.StoredDataInvalid,
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.StoredDataInvalid,
                    "Der gespeicherte Entwurf passt nicht eindeutig zum ausgewählten Zeitraum."));
        }

        ScheduleWorkspaceContextResult contextResult =
            ScheduleWorkspaceContextBuilder.Build(period, data);
        if (!contextResult.IsSuccess)
        {
            return ScheduleWorkspaceQueryResult.Failure(
                contextResult.Status == OpenOrCreateScheduleDraftStatus.CatalogInvalid
                    ? ScheduleWorkspaceQueryStatus.CatalogInvalid
                    : ScheduleWorkspaceQueryStatus.StoredDataInvalid,
                contextResult.Errors.ToArray());
        }

        ScheduleWorkspaceContext context = contextResult.Value!;
        Dictionary<(EmployeeId EmployeeId, DateOnly Date), AvailabilityEntryReadItem>
            entryItems = data.Availability.Entries.ToDictionary(item =>
                (item.Entry.EmployeeId, item.Entry.Date));
        AvailabilityPeriodSnapshot availability =
            new AvailabilityPeriodSnapshotProjector(
                data.Availability,
                entryItems,
                context.EmployeeTypes,
                context.WeeklyAvailabilities)
            .Create(period.StartMonday, period.EndSunday);
        ServiceManagementReadinessSnapshot[] readiness =
            CreateServiceManagementReadiness(
                data.ExactDraft,
                data.Availability.Employees,
                context);
        PreparationProjectionResult preparation = await CreatePreparationAsync(
            data,
            context,
            cancellationToken);
        if (preparation.Error is not null)
        {
            return ScheduleWorkspaceQueryResult.Failure(
                ScheduleWorkspaceQueryStatus.StoredDataInvalid,
                preparation.Error);
        }

        ScheduleWorkspaceSnapshot snapshot = CreateSnapshot(
            data.ExactDraft,
            data,
            context,
            availability,
            readiness,
            preparation);

        return ScheduleWorkspaceQueryResult.Success(snapshot);
    }

    private static ScheduleWorkspaceSnapshot CreateSnapshot(
        ScheduleDraft draft,
        ScheduleWorkspaceReadData data,
        ScheduleWorkspaceContext context,
        AvailabilityPeriodSnapshot availability,
        IEnumerable<ServiceManagementReadinessSnapshot> readiness,
        PreparationProjectionResult preparation)
    {
        ScheduleDemandSlotSnapshot[] slots = ScheduleSnapshotMapper.CreateDemandSlots(
            draft.DemandSlots,
            data.StaffingDemands.ServiceCatalog);
        ScheduleAssignmentSnapshot[] assignments =
            ScheduleSnapshotMapper.CreateAssignments(draft.Assignments);
        ServiceManagementAssignmentOptionSnapshot[] assignmentOptions =
            CreateAssignmentOptions(draft, data, context);

        return new ScheduleWorkspaceSnapshot(
            draft.Id.Value,
            draft.Version.Value,
            availability,
            slots,
            assignments,
            draft.GeneratedDayOffMarkers.Select(marker =>
                new ScheduleGeneratedDayOffSnapshot(
                    marker.EmployeeId.Value,
                    marker.Date)),
            assignmentOptions,
            readiness,
            preparation.Status,
            data.PreparedSnapshot?.Id,
            data.PreparedSnapshot?.RunOptions,
            data.PreparedSnapshot?.History.Completeness,
            preparation.ChangedCategories,
            CreateAcceptedAutomaticSchedule(draft, data.AutomaticScheduleRun));
    }

    private static AcceptedAutomaticScheduleSnapshot? CreateAcceptedAutomaticSchedule(
        ScheduleDraft draft,
        AutomaticScheduleRunRecord? run)
    {
        if (run is null)
        {
            return null;
        }

        return new AcceptedAutomaticScheduleSnapshot(
            draft.Assignments.Count(assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration),
            draft.GeneratedDayOffMarkers.Count);
    }

    private static ServiceManagementAssignmentOptionSnapshot[] CreateAssignmentOptions(
        ScheduleDraft draft,
        ScheduleWorkspaceReadData data,
        ScheduleWorkspaceContext context)
    {
        Employee? employee = data.Availability.Employees.SingleOrDefault(candidate =>
            candidate.IsActive
            && context.EmployeeTypes[candidate.EmployeeTypeId].PlanningPolicy.Role
                == EmployeeTypePlanningRole.ServiceManagement);
        if (employee is null)
        {
            return [];
        }

        EmployeeType employeeType = context.EmployeeTypes[employee.EmployeeTypeId];
        HashSet<DemandSlotId> occupiedByOthers = draft.Assignments
            .Where(assignment => assignment.EmployeeId != employee.Id)
            .SelectMany(assignment => assignment.Coverages)
            .Select(coverage => coverage.SlotId)
            .ToHashSet();
        DemandSlot[] availableSlots = context.DemandSlots.Slots
            .Where(slot => !occupiedByOthers.Contains(slot.Id))
            .ToArray();
        List<ServiceManagementAssignmentOptionSnapshot> options = [];

        foreach (DemandSlot slot in availableSlots.Where(slot =>
                     IsShiftEligible(employeeType, slot)))
        {
            AddOption(
                options,
                employee,
                ServiceManagementAssignmentSelectionKind.NormalDemand,
                slot,
                null,
                data);
            if (ScheduleAssignment.CreateOfficeTime(
                    employee.Id.Value,
                    employee.Id,
                    slot).IsSuccess)
            {
                AddOption(
                    options,
                    employee,
                    ServiceManagementAssignmentSelectionKind.OfficeTime,
                    slot,
                    null,
                    data);
            }
        }

        if (IsPatternEligible(
                employeeType,
                data.StaffingDemands.ServiceCatalog.SplitShiftPattern.Id))
        {
            AddCompositeOptions(
                options,
                employee,
                ServiceManagementAssignmentSelectionKind.SplitShiftPattern,
                availableSlots,
                data);
        }

        if (IsPatternEligible(
                employeeType,
                data.StaffingDemands.ServiceCatalog.ReliefShiftPattern.Id))
        {
            AddCompositeOptions(
                options,
                employee,
                ServiceManagementAssignmentSelectionKind.ReliefShiftPattern,
                availableSlots,
                data);
        }

        return options
            .OrderBy(option => option.FirstSlot.Date)
            .ThenBy(option => option.Kind)
            .ThenBy(option => option.FirstSlot.ActualStart)
            .ThenBy(option => option.FirstSlot.Ordinal)
            .ThenBy(option => option.SecondSlot?.Ordinal)
            .ToArray();
    }

    private static void AddCompositeOptions(
        List<ServiceManagementAssignmentOptionSnapshot> options,
        Employee employee,
        ServiceManagementAssignmentSelectionKind kind,
        IEnumerable<DemandSlot> slots,
        ScheduleWorkspaceReadData data)
    {
        DemandSlot[] values = slots.ToArray();
        foreach (DemandSlot first in values)
        {
            foreach (DemandSlot second in values.Where(candidate =>
                         candidate.Id.Date == first.Id.Date
                         && candidate.Id != first.Id))
            {
                ScheduleAssignmentValidationResult result = kind switch
                {
                    ServiceManagementAssignmentSelectionKind.SplitShiftPattern =>
                        ScheduleAssignment.CreateSplitShift(
                            employee.Id.Value,
                            employee.Id,
                            data.StaffingDemands.ServiceCatalog.SplitShiftPattern,
                            first,
                            second,
                            AssignmentOrigin.ServiceManagement),
                    ServiceManagementAssignmentSelectionKind.ReliefShiftPattern =>
                        ScheduleAssignment.CreateReliefShift(
                            employee.Id.Value,
                            employee.Id,
                            data.StaffingDemands.ServiceCatalog.ReliefShiftPattern,
                            first,
                            second,
                            AssignmentOrigin.ServiceManagement),
                    _ => throw new InvalidOperationException(
                        $"Unsupported composite assignment kind: {kind}"),
                };
                if (result.IsSuccess)
                {
                    AddOption(options, employee, kind, first, second, data);
                }
            }
        }
    }

    private static void AddOption(
        List<ServiceManagementAssignmentOptionSnapshot> options,
        Employee employee,
        ServiceManagementAssignmentSelectionKind kind,
        DemandSlot first,
        DemandSlot? second,
        ScheduleWorkspaceReadData data)
    {
        options.Add(new ServiceManagementAssignmentOptionSnapshot(
            employee.Id.Value,
            kind,
            ScheduleSnapshotMapper.CreateDemandSlot(
                first,
                data.StaffingDemands.ServiceCatalog),
            second is null
                ? null
                : ScheduleSnapshotMapper.CreateDemandSlot(
                    second,
                    data.StaffingDemands.ServiceCatalog)));
    }

    private static bool IsShiftEligible(EmployeeType employeeType, DemandSlot slot)
    {
        return employeeType.ShiftEligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
            && eligibility.ShiftTypeId == slot.Id.ShiftTypeId
            && eligibility.Activation == ShiftEligibilityActivation.Always);
    }

    private static bool IsPatternEligible(
        EmployeeType employeeType,
        ShiftPatternId patternId)
    {
        return employeeType.ShiftEligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
            && eligibility.ShiftPatternId == patternId
            && eligibility.Activation == ShiftEligibilityActivation.Always);
    }

    private static async Task<PreparationProjectionResult> CreatePreparationAsync(
        ScheduleWorkspaceReadData data,
        ScheduleWorkspaceContext context,
        CancellationToken cancellationToken)
    {
        if (data.PreparedSnapshot is null)
        {
            return PreparationProjectionResult.NotPrepared();
        }

        if (data.PreparedSnapshot.DraftId != data.ExactDraft!.Id.Value
            || data.PreparedSnapshot.PeriodMonday != data.ExactDraft.Period.StartMonday
            || data.PreparedSnapshot.PeriodSunday != data.ExactDraft.Period.EndSunday)
        {
            return PreparationProjectionResult.Failure(
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.StoredDataInvalid,
                    "Die gespeicherte Planungsmomentaufnahme gehört nicht zum ausgewählten Entwurf."));
        }

        RuleCatalogSnapshot ruleCatalog = await GetRuleCatalogQuery.ExecuteAsync(
            cancellationToken);
        ScheduleDayCommandContext commandContext = new(
            data,
            context,
            data.ExactDraft!);
        PlanningInputSnapshotBuildResult candidateResult =
            PlanningInputSnapshotBuilder.Build(
                commandContext,
                data.HistoryDays,
                ruleCatalog,
                data.PreparedSnapshot.RunOptions);
        if (candidateResult.Error is not null)
        {
            if (candidateResult.Error.Code is
                PreparePlanningInputErrorCode.InvalidServiceManagementAssignment
                or PreparePlanningInputErrorCode.ServiceManagementNotReady)
            {
                return PreparationProjectionResult.Outdated(
                    PlanningInputChangeCategory.ServiceManagementAssignments);
            }

            return PreparationProjectionResult.Failure(
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.StoredDataInvalid,
                    candidateResult.Error.Message));
        }

        PlanningInputComparison comparison = PlanningInputComparison.Compare(
            data.PreparedSnapshot,
            candidateResult.Value!);
        return comparison.IsCurrent
            ? PreparationProjectionResult.Prepared()
            : PreparationProjectionResult.Outdated(
                comparison.ChangedCategories.ToArray());
    }

    private static ServiceManagementReadinessSnapshot[]
        CreateServiceManagementReadiness(
            ScheduleDraft draft,
            IEnumerable<Employee> employees,
            ScheduleWorkspaceContext context)
    {
        return employees
            .Where(employee => employee.IsActive)
            .Where(employee =>
                context.EmployeeTypes[employee.EmployeeTypeId].PlanningPolicy.Role
                    == EmployeeTypePlanningRole.ServiceManagement)
            .Select(employee =>
            {
                WeeklyAvailability[] weeks = context.WeeklyAvailabilities
                    .Where(item => item.Key.EmployeeId == employee.Id)
                    .OrderBy(item => item.Key.WeekMonday)
                    .Select(item => item.Value)
                    .ToArray();
                ServiceManagementReadiness result =
                    ServiceManagementReadiness.Evaluate(
                        draft,
                        employee.Id,
                        weeks).Value
                    ?? throw new InvalidOperationException(
                        "Validated weekly availability could not be evaluated.");

                return new ServiceManagementReadinessSnapshot(
                    employee.Id.Value,
                    result.CanPrepare,
                    result.Weeks.Select(week =>
                        new ServiceManagementWeekReadinessSnapshot(
                            week.WeekMonday,
                            MapReadinessStatus(week.Status),
                            week.WorkMinutes)));
            })
            .ToArray();
    }

    private static ServiceManagementWeekReadinessStatusSnapshot MapReadinessStatus(
        ServiceManagementWeekReadinessStatus status)
    {
        return status switch
        {
            ServiceManagementWeekReadinessStatus.Ready =>
                ServiceManagementWeekReadinessStatusSnapshot.Ready,
            ServiceManagementWeekReadinessStatus.ExemptFullyUnavailable =>
                ServiceManagementWeekReadinessStatusSnapshot.ExemptFullyUnavailable,
            ServiceManagementWeekReadinessStatus.MissingAssignment =>
                ServiceManagementWeekReadinessStatusSnapshot.MissingAssignment,
            _ => throw new InvalidOperationException(
                $"Unsupported service-management readiness status: {status}"),
        };
    }
}

internal sealed record PreparationProjectionResult(
    SchedulePreparationStatus Status,
    IReadOnlyList<PlanningInputChangeCategory> ChangedCategories,
    ScheduleWorkspaceError? Error)
{
    public static PreparationProjectionResult NotPrepared() => new(
        SchedulePreparationStatus.NotPrepared,
        [],
        null);

    public static PreparationProjectionResult Prepared() => new(
        SchedulePreparationStatus.Prepared,
        [],
        null);

    public static PreparationProjectionResult Outdated(
        params PlanningInputChangeCategory[] categories) => new(
            SchedulePreparationStatus.Outdated,
            categories,
            null);

    public static PreparationProjectionResult Failure(
        ScheduleWorkspaceError error) => new(
            SchedulePreparationStatus.Outdated,
            [],
            error);
}
