using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class AutomaticScheduleProposal
{
    public AutomaticScheduleProposal(
        Guid snapshotId,
        Guid draftId,
        long expectedDraftVersion,
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        IEnumerable<AutomaticScheduleDayOffProposal> generatedDayOffs,
        IEnumerable<AutomaticScheduleOpenDemand> openDemands,
        ScheduleObjectiveVector objectiveVector,
        ScheduleRuleEvaluationSet ruleEvaluations,
        AutomaticScheduleRunMetadata metadata)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new ArgumentException("Snapshot identifier is required.", nameof(snapshotId));
        }

        if (draftId == Guid.Empty)
        {
            throw new ArgumentException("Draft identifier is required.", nameof(draftId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedDraftVersion);
        ArgumentNullException.ThrowIfNull(assignments);
        ArgumentNullException.ThrowIfNull(generatedDayOffs);
        ArgumentNullException.ThrowIfNull(openDemands);
        ArgumentNullException.ThrowIfNull(objectiveVector);
        ArgumentNullException.ThrowIfNull(ruleEvaluations);
        ArgumentNullException.ThrowIfNull(metadata);

        ScheduleAssignmentSnapshot[] assignmentValues = assignments.ToArray();
        AutomaticScheduleDayOffProposal[] dayOffValues = generatedDayOffs.ToArray();
        AutomaticScheduleOpenDemand[] openDemandValues = openDemands.ToArray();
        if (assignmentValues.Any(value => value is null)
            || dayOffValues.Any(value => value is null)
            || openDemandValues.Any(value => value is null))
        {
            throw new ArgumentException(
                "Automatic schedule proposal collections cannot contain null values.");
        }

        if (assignmentValues.Select(value => value.AssignmentId).Distinct().Count()
            != assignmentValues.Length)
        {
            throw new ArgumentException(
                "Automatic assignment identifiers must be unique.",
                nameof(assignments));
        }

        if (dayOffValues
            .Select(value => (value.EmployeeId, value.Date))
            .Distinct()
            .Count() != dayOffValues.Length)
        {
            throw new ArgumentException(
                "Generated day-off markers must be unique per employee and date.",
                nameof(generatedDayOffs));
        }

        if (openDemandValues
            .Select(value => new
            {
                value.SourceId,
                value.Date,
                value.WorkLocationId,
                value.ShiftTypeId,
                value.Ordinal,
                value.UncoveredStart,
                value.UncoveredEnd,
            })
            .Distinct()
            .Count() != openDemandValues.Length)
        {
            throw new ArgumentException(
                "Open demand intervals must be unique.",
                nameof(openDemands));
        }

        SnapshotId = snapshotId;
        DraftId = draftId;
        ExpectedDraftVersion = expectedDraftVersion;
        Assignments = Array.AsReadOnly(assignmentValues);
        GeneratedDayOffs = Array.AsReadOnly(dayOffValues);
        OpenDemands = Array.AsReadOnly(openDemandValues);
        ObjectiveVector = objectiveVector;
        RuleEvaluations = ruleEvaluations;
        Metadata = metadata;
    }

    public Guid SnapshotId { get; }

    public Guid DraftId { get; }

    public long ExpectedDraftVersion { get; }

    public ReadOnlyCollection<ScheduleAssignmentSnapshot> Assignments { get; }

    public ReadOnlyCollection<AutomaticScheduleDayOffProposal> GeneratedDayOffs { get; }

    public ReadOnlyCollection<AutomaticScheduleOpenDemand> OpenDemands { get; }

    public ScheduleObjectiveVector ObjectiveVector { get; }

    public ScheduleRuleEvaluationSet RuleEvaluations { get; }

    public AutomaticScheduleRunMetadata Metadata { get; }

    internal AutomaticScheduleProposal WithMetadata(
        AutomaticScheduleRunMetadata metadata) => new(
        SnapshotId,
        DraftId,
        ExpectedDraftVersion,
        Assignments,
        GeneratedDayOffs,
        OpenDemands,
        ObjectiveVector,
        RuleEvaluations,
        metadata);
}
