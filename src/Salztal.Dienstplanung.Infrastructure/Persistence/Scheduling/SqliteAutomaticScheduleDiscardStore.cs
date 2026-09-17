using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

public sealed class SqliteAutomaticScheduleDiscardStore
    : IDiscardAutomaticScheduleStore
{
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteAutomaticScheduleDiscardStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _contextFactory = new ServiceCatalogDbContextFactory(
            Path.GetFullPath(databasePath));
    }

    public async Task<DiscardAutomaticScheduleStoreResult> DiscardAsync(
        DiscardAutomaticScheduleChange change,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(change);
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        ScheduleDraftEntity? current = await context.ScheduleDrafts.SingleOrDefaultAsync(
            entity => entity.Id == change.UpdatedDraft.Id.Value,
            cancellationToken);
        bool automaticRunExists = current is not null
            && await context.AutomaticScheduleRuns.AnyAsync(
                entity => entity.DraftId == current.Id,
                cancellationToken);
        if (!MatchesExpectedState(current, automaticRunExists, change))
        {
            await transaction.RollbackAsync(cancellationToken);
            return DiscardAutomaticScheduleStoreResult.Conflict();
        }

        try
        {
            await DeleteAutomaticResultAsync(
                context,
                current!.Id,
                cancellationToken);
            await DeletePreparationAsync(context, current, cancellationToken);
            current.Version = change.UpdatedDraft.Version.Value;
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return DiscardAutomaticScheduleStoreResult.Success(change.UpdatedDraft);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return DiscardAutomaticScheduleStoreResult.Conflict();
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static bool MatchesExpectedState(
        ScheduleDraftEntity? current,
        bool automaticRunExists,
        DiscardAutomaticScheduleChange change) =>
        current is not null
        && current.Version == change.ExpectedDraftVersion
        && current.PreparedSnapshotId == change.ExpectedPreparedSnapshotId
        && current.StartMonday == change.UpdatedDraft.Period.StartMonday
        && current.EndSunday == change.UpdatedDraft.Period.EndSunday
        && change.UpdatedDraft.Version.Value == change.ExpectedDraftVersion + 1
        && change.UpdatedDraft.Assignments.All(assignment =>
            assignment.Origin != AssignmentOrigin.AutomaticGeneration)
        && change.UpdatedDraft.GeneratedDayOffMarkers.Count == 0
        && automaticRunExists;

    private static async Task DeleteAutomaticResultAsync(
        ServiceCatalogDbContext context,
        Guid draftId,
        CancellationToken cancellationToken)
    {
        Guid[] automaticAssignmentIds = await context.ScheduleAssignments
            .Where(entity => entity.DraftId == draftId)
            .Where(entity =>
                entity.Origin == (int)AssignmentOrigin.AutomaticGeneration)
            .Select(entity => entity.Id)
            .ToArrayAsync(cancellationToken);
        await context.ScheduleAssignmentLocks
            .Where(entity => entity.DraftId == draftId)
            .Where(entity => automaticAssignmentIds.Contains(entity.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleDemandCoverages
            .Where(entity => automaticAssignmentIds.Contains(entity.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignmentSegments
            .Where(entity => automaticAssignmentIds.Contains(entity.AssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleAssignments
            .Where(entity => automaticAssignmentIds.Contains(entity.Id))
            .ExecuteDeleteAsync(cancellationToken);
        await context.ScheduleGeneratedDaysOff
            .Where(entity => entity.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.AutomaticScheduleRuns
            .Where(entity => entity.DraftId == draftId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task DeletePreparationAsync(
        ServiceCatalogDbContext context,
        ScheduleDraftEntity draft,
        CancellationToken cancellationToken)
    {
        Guid? preparedSnapshotId = draft.PreparedSnapshotId;
        draft.PreparedSnapshotId = null;
        if (preparedSnapshotId is null)
        {
            return;
        }

        await context.PlanningSnapshotComponents
            .Where(entity => entity.SnapshotId == preparedSnapshotId.Value)
            .ExecuteDeleteAsync(cancellationToken);
        await context.PlanningSnapshots
            .Where(entity => entity.Id == preparedSnapshotId.Value)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
