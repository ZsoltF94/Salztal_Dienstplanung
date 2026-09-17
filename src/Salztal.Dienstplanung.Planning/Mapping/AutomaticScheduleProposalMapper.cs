using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;

namespace Salztal.Dienstplanung.Planning.Mapping;

internal interface IAutomaticScheduleProposalMapper
{
    public PreparedAutomaticScheduleProposal Prepare(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization);

    public AutomaticScheduleProposal Complete(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization,
        PreparedAutomaticScheduleProposal prepared,
        AutomaticScheduleRunMetadata metadata);
}

internal sealed class AutomaticScheduleProposalMapper : IAutomaticScheduleProposalMapper
{
    private readonly IAutomaticScheduleAssignmentIdFactory assignmentIdFactory;

    public AutomaticScheduleProposalMapper(
        IAutomaticScheduleAssignmentIdFactory assignmentIdFactory)
    {
        ArgumentNullException.ThrowIfNull(assignmentIdFactory);
        this.assignmentIdFactory = assignmentIdFactory;
    }

    public AutomaticScheduleProposal Map(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization,
        AutomaticScheduleRunMetadata metadata)
    {
        PreparedAutomaticScheduleProposal prepared = Prepare(snapshot, optimization);
        return Complete(snapshot, optimization, prepared, metadata);
    }

    public PreparedAutomaticScheduleProposal Prepare(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(optimization);

        ScheduleAssignmentSnapshot[] assignments = optimization.SelectedCandidates
            .Select(candidate => MapAssignment(snapshot.Id, candidate))
            .ToArray();
        AutomaticScheduleDayOffProposal[] dayOffs = CreateDayOffs(
            snapshot,
            optimization.SelectedCandidates);
        ScheduleObjectiveVector objective = new(
            optimization.UncoveredEmployeeMinutes,
            optimization.FullyUncoveredDemandSlotCount,
            optimization.HighPriorityViolations,
            optimization.ReliefShiftAssignmentCount,
            optimization.SplitShiftAssignmentCount,
            optimization.AuxiliaryWeeklyMinimum,
            optimization.RelativeWeeklyTarget,
            optimization.MediumPriorityViolations,
            RuleViolationSet.Empty,
            optimization.StabilityViolations,
            optimization.SelectedCandidates.Select(candidate => candidate.TechnicalKey));
        ScheduleRuleEvaluationSet evaluations = CreateRuleEvaluations(optimization);
        return new PreparedAutomaticScheduleProposal(
            assignments,
            dayOffs,
            objective,
            evaluations);
    }

    public AutomaticScheduleProposal Complete(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization,
        PreparedAutomaticScheduleProposal prepared,
        AutomaticScheduleRunMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(optimization);
        ArgumentNullException.ThrowIfNull(prepared);
        ArgumentNullException.ThrowIfNull(metadata);
        AutomaticScheduleProposal proposal = new(
            snapshot.Id,
            snapshot.DraftId,
            snapshot.DraftVersion,
            prepared.Assignments,
            prepared.DayOffs,
            optimization.OpenDemands,
            prepared.Objective,
            prepared.Evaluations,
            metadata);
        AutomaticScheduleProposalValidationResult validation =
            AutomaticScheduleProposalValidator.Validate(
                snapshot,
                optimization,
                proposal,
                assignmentIdFactory);
        return validation.IsValid
            ? proposal
            : throw new InvalidOperationException(
                $"Mapped proposal failed independent validation: {validation.Code}.");
    }

    private ScheduleAssignmentSnapshot MapAssignment(
        Guid snapshotId,
        PlanningAssignmentCandidate candidate)
    {
        ScheduleAssignmentKindSnapshot kind = candidate.Kind switch
        {
            PlanningCandidateKind.NormalDemand =>
                ScheduleAssignmentKindSnapshot.NormalDemand,
            PlanningCandidateKind.SplitShiftPattern =>
                ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            PlanningCandidateKind.ReliefShiftPattern =>
                ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
            _ => throw new InvalidOperationException("Unknown planning candidate kind."),
        };
        AutomaticScheduleAssignmentIdentity identity = new(
            snapshotId,
            candidate.EmployeeId,
            candidate.Date,
            kind,
            candidate.PatternId,
            candidate.Coverages.Select(coverage =>
                new AutomaticScheduleDemandSlotIdentity(
                    coverage.Demand.SourceId,
                    coverage.Demand.Date,
                    coverage.Demand.WorkLocationId,
                    coverage.Demand.ShiftTypeId,
                    coverage.Demand.Ordinal)));
        return new ScheduleAssignmentSnapshot(
            assignmentIdFactory.Create(identity),
            candidate.EmployeeId,
            candidate.Date,
            kind,
            ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
            candidate.PatternId,
            candidate.WorkMinutes,
            false,
            candidate.Segments.Select(segment =>
                new ScheduleAssignmentSegmentSnapshot(
                    segment.AnchorDemand.SourceId,
                    candidate.Date,
                    segment.AnchorDemand.WorkLocationId,
                    segment.AnchorDemand.ShiftTypeId,
                    segment.ActualStart,
                    segment.ActualEnd,
                    segment.WorkMinutes)),
            candidate.Coverages.Select((coverage, index) =>
                new ScheduleDemandCoverageSnapshot(
                    coverage.Demand.SourceId,
                    coverage.Demand.Date,
                    coverage.Demand.WorkLocationId,
                    coverage.Demand.ShiftTypeId,
                    coverage.Demand.Ordinal,
                    coverage.CoveredStart,
                    coverage.CoveredEnd,
                    coverage.CoveredMinutes,
                    candidate.Kind == PlanningCandidateKind.ReliefShiftPattern
                        && index == 1
                        ? ScheduleDemandCoverageKindSnapshot.PartialReliefShift
                        : ScheduleDemandCoverageKindSnapshot.Full)));
    }

    private static AutomaticScheduleDayOffProposal[] CreateDayOffs(
        PlanningInputSnapshot snapshot,
        IEnumerable<PlanningAssignmentCandidate> selectedCandidates)
    {
        HashSet<(Guid EmployeeId, DateOnly Date)> workingDays = selectedCandidates
            .Select(candidate => (candidate.EmployeeId, candidate.Date))
            .Concat(snapshot.ServiceManagementAssignments.Select(assignment =>
                (assignment.EmployeeId, assignment.Date)))
            .ToHashSet();
        HashSet<(Guid EmployeeId, DateOnly Date)> unavailableDays = snapshot
            .AvailabilityEntries
            .Select(entry => (entry.EmployeeId, entry.Date))
            .ToHashSet();
        return snapshot.Employees
            .SelectMany(employee => Enumerable.Range(0, 21).Select(index =>
                (employee.Id, Date: snapshot.PeriodMonday.AddDays(index))))
            .Where(day => !workingDays.Contains(day) && !unavailableDays.Contains(day))
            .OrderBy(day => day.Id)
            .ThenBy(day => day.Date)
            .Select(day => new AutomaticScheduleDayOffProposal(day.Id, day.Date))
            .ToArray();
    }

    private static ScheduleRuleEvaluationSet CreateRuleEvaluations(
        DemandCoverageOptimizationResult optimization)
    {
        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value!;
        Dictionary<RuleId, RuleEvaluationResult> hardResults = optimization
            .HardRuleEvaluation.Results
            .ToDictionary(result => result.RuleId);
        Dictionary<RuleId, RuleEvaluationResult> highResults = optimization
            .HighPriorityRuleEvaluation.Results
            .ToDictionary(result => result.RuleId);
        Dictionary<RuleId, RuleEvaluationResult> mediumResults = optimization
            .MediumPriorityRuleResults
            .ToDictionary(result => result.RuleId);
        Dictionary<RuleId, RuleEvaluationResult> noticeResults = optimization
            .NoticeRuleResults
            .ToDictionary(result => result.RuleId);
        RuleEvaluationResult stabilityResult = optimization.StabilityRuleResult;
        return new ScheduleRuleEvaluationSet(
            catalog,
            catalog.Definitions.Select(definition =>
                hardResults.GetValueOrDefault(definition.Id)
                ?? highResults.GetValueOrDefault(definition.Id)
                ?? mediumResults.GetValueOrDefault(definition.Id)
                ?? noticeResults.GetValueOrDefault(definition.Id)
                ?? (definition.Id == stabilityResult.RuleId ? stabilityResult : null)
                ?? RuleEvaluationResult.Create(
                    definition.Id,
                    RuleEvaluationStatus.NotApplicable,
                    NoRuleResultParameters.Instance)));
    }
}

internal sealed record PreparedAutomaticScheduleProposal(
    IReadOnlyCollection<ScheduleAssignmentSnapshot> Assignments,
    IReadOnlyCollection<AutomaticScheduleDayOffProposal> DayOffs,
    ScheduleObjectiveVector Objective,
    ScheduleRuleEvaluationSet Evaluations);
