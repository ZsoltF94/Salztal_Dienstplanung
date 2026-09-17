using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;

namespace Salztal.Dienstplanung.Planning.Mapping;

internal enum AutomaticScheduleProposalValidationCode
{
    Valid,
    IdentityMismatch,
    AssignmentMismatch,
    DayOffMismatch,
    OpenDemandMismatch,
    ObjectiveMismatch,
    RuleEvaluationMismatch,
    HardRuleViolation,
}

internal sealed record AutomaticScheduleProposalValidationResult(
    AutomaticScheduleProposalValidationCode Code)
{
    public bool IsValid => Code == AutomaticScheduleProposalValidationCode.Valid;
}

internal static class AutomaticScheduleProposalValidator
{
    public static AutomaticScheduleProposalValidationResult Validate(
        PlanningInputSnapshot snapshot,
        DemandCoverageOptimizationResult optimization,
        AutomaticScheduleProposal proposal,
        IAutomaticScheduleAssignmentIdFactory assignmentIdFactory)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(optimization);
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(assignmentIdFactory);

        if (proposal.SnapshotId != snapshot.Id
            || proposal.DraftId != snapshot.DraftId
            || proposal.ExpectedDraftVersion != snapshot.DraftVersion)
        {
            return new(AutomaticScheduleProposalValidationCode.IdentityMismatch);
        }

        if (!AssignmentsMatch(
                snapshot.Id,
                optimization.SelectedCandidates,
                proposal.Assignments,
                assignmentIdFactory))
        {
            return new(AutomaticScheduleProposalValidationCode.AssignmentMismatch);
        }

        if (!DayOffsMatch(snapshot, optimization.SelectedCandidates, proposal.GeneratedDayOffs))
        {
            return new(AutomaticScheduleProposalValidationCode.DayOffMismatch);
        }

        if (!optimization.OpenDemands.SequenceEqual(proposal.OpenDemands))
        {
            return new(AutomaticScheduleProposalValidationCode.OpenDemandMismatch);
        }

        if (proposal.ObjectiveVector.UncoveredEmployeeMinutes
                != optimization.UncoveredEmployeeMinutes
            || proposal.ObjectiveVector.FullyUncoveredDemandSlotCount
                != optimization.FullyUncoveredDemandSlotCount
            || !proposal.ObjectiveVector.TechnicalTieBreakerKeys.SequenceEqual(
                optimization.SelectedCandidates.Select(candidate => candidate.TechnicalKey))
            || !ViolationSetsMatch(
                proposal.ObjectiveVector.HighPriorityViolations,
                optimization.HighPriorityViolations)
            || !ViolationSetsMatch(
                proposal.ObjectiveVector.MediumPriorityViolations,
                optimization.MediumPriorityViolations)
            || proposal.ObjectiveVector.LowPriorityViolations.Count != 0
            || !ViolationSetsMatch(
                proposal.ObjectiveVector.StabilityViolations,
                optimization.StabilityViolations))
        {
            return new(AutomaticScheduleProposalValidationCode.ObjectiveMismatch);
        }

        Dictionary<RuleId, RuleEvaluationResult> expectedRules =
            optimization.HardRuleEvaluation.Results
                .Concat(optimization.HighPriorityRuleEvaluation.Results)
                .Concat(optimization.MediumPriorityRuleResults)
                .Concat(optimization.NoticeRuleResults)
                .Append(optimization.StabilityRuleResult)
                .ToDictionary(
                result => result.RuleId,
                result => result);
        if (proposal.RuleEvaluations.AutomaticHardResults.Count
                + proposal.RuleEvaluations.SoftResults.Count(result =>
                    expectedRules.ContainsKey(result.RuleId))
                + proposal.RuleEvaluations.NoticeResults.Count(result =>
                    expectedRules.ContainsKey(result.RuleId))
                + proposal.RuleEvaluations.StabilityResults.Count(result =>
                    expectedRules.ContainsKey(result.RuleId))
                != expectedRules.Count
            || proposal.RuleEvaluations.AutomaticHardResults
                .Concat(proposal.RuleEvaluations.SoftResults)
                .Concat(proposal.RuleEvaluations.NoticeResults)
                .Concat(proposal.RuleEvaluations.StabilityResults)
                .Where(result => expectedRules.ContainsKey(result.RuleId))
                .Any(result =>
                !expectedRules.TryGetValue(result.RuleId, out var expected)
                || result.Status != expected.Status
                || result.Parameters != expected.Parameters))
        {
            return new(AutomaticScheduleProposalValidationCode.RuleEvaluationMismatch);
        }

        return optimization.HardRuleEvaluation.IsValid
            ? new(AutomaticScheduleProposalValidationCode.Valid)
            : new(AutomaticScheduleProposalValidationCode.HardRuleViolation);
    }

    private static bool ViolationSetsMatch(
        RuleViolationSet left,
        RuleViolationSet right) =>
        left.Violations.SequenceEqual(right.Violations);

    private static bool AssignmentsMatch(
        Guid snapshotId,
        ReadOnlyCollection<PlanningAssignmentCandidate> candidates,
        ReadOnlyCollection<ScheduleAssignmentSnapshot> assignments,
        IAutomaticScheduleAssignmentIdFactory assignmentIdFactory)
    {
        if (candidates.Count != assignments.Count)
        {
            return false;
        }

        Dictionary<Guid, ScheduleAssignmentSnapshot> assignmentsById = assignments
            .ToDictionary(assignment => assignment.AssignmentId);
        foreach (PlanningAssignmentCandidate candidate in candidates)
        {
            ScheduleAssignmentKindSnapshot expectedKind = candidate.Kind switch
            {
                PlanningCandidateKind.NormalDemand =>
                    ScheduleAssignmentKindSnapshot.NormalDemand,
                PlanningCandidateKind.SplitShiftPattern =>
                    ScheduleAssignmentKindSnapshot.SplitShiftPattern,
                PlanningCandidateKind.ReliefShiftPattern =>
                    ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
                _ => throw new InvalidOperationException("Unknown planning candidate kind."),
            };
            Guid expectedId = assignmentIdFactory.Create(
                new AutomaticScheduleAssignmentIdentity(
                    snapshotId,
                    candidate.EmployeeId,
                    candidate.Date,
                    expectedKind,
                    candidate.PatternId,
                    candidate.Coverages.Select(coverage =>
                        new AutomaticScheduleDemandSlotIdentity(
                            coverage.Demand.SourceId,
                            coverage.Demand.Date,
                            coverage.Demand.WorkLocationId,
                            coverage.Demand.ShiftTypeId,
                            coverage.Demand.Ordinal))));
            if (!assignmentsById.TryGetValue(
                    expectedId,
                    out ScheduleAssignmentSnapshot? assignment)
                || assignment.EmployeeId != candidate.EmployeeId
                || assignment.Date != candidate.Date
                || assignment.Kind != expectedKind
                || assignment.Origin != ScheduleAssignmentOriginSnapshot.AutomaticGeneration
                || assignment.PatternId != candidate.PatternId
                || assignment.WorkMinutes != candidate.WorkMinutes
                || assignment.IsProtectedFromAutomaticGeneration
                || !SegmentsMatch(candidate, assignment)
                || !CoveragesMatch(candidate, assignment))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SegmentsMatch(
        PlanningAssignmentCandidate candidate,
        ScheduleAssignmentSnapshot assignment) =>
        candidate.Segments.Count == assignment.Segments.Count
        && candidate.Segments.Zip(assignment.Segments).All(pair =>
            pair.Second.DemandSourceId == pair.First.AnchorDemand.SourceId
            && pair.Second.Date == pair.First.AnchorDemand.Date
            && pair.Second.WorkLocationId == pair.First.AnchorDemand.WorkLocationId
            && pair.Second.ShiftTypeId == pair.First.AnchorDemand.ShiftTypeId
            && pair.Second.ActualStart == pair.First.ActualStart
            && pair.Second.ActualEnd == pair.First.ActualEnd
            && pair.Second.WorkMinutes == pair.First.WorkMinutes);

    private static bool CoveragesMatch(
        PlanningAssignmentCandidate candidate,
        ScheduleAssignmentSnapshot assignment) =>
        candidate.Coverages.Count == assignment.Coverages.Count
        && candidate.Coverages.Zip(assignment.Coverages).Select((pair, index) =>
        {
            ScheduleDemandCoverageKindSnapshot expectedKind =
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern && index == 1
                    ? ScheduleDemandCoverageKindSnapshot.PartialReliefShift
                    : ScheduleDemandCoverageKindSnapshot.Full;
            return pair.Second.DemandSourceId == pair.First.Demand.SourceId
                && pair.Second.Date == pair.First.Demand.Date
                && pair.Second.WorkLocationId == pair.First.Demand.WorkLocationId
                && pair.Second.ShiftTypeId == pair.First.Demand.ShiftTypeId
                && pair.Second.Ordinal == pair.First.Demand.Ordinal
                && pair.Second.CoveredStart == pair.First.CoveredStart
                && pair.Second.CoveredEnd == pair.First.CoveredEnd
                && pair.Second.CoveredMinutes == pair.First.CoveredMinutes
                && pair.Second.Kind == expectedKind;
        }).All(matches => matches);

    private static bool DayOffsMatch(
        PlanningInputSnapshot snapshot,
        ReadOnlyCollection<PlanningAssignmentCandidate> candidates,
        ReadOnlyCollection<AutomaticScheduleDayOffProposal> dayOffs)
    {
        HashSet<(Guid EmployeeId, DateOnly Date)> occupied = candidates
            .Select(candidate => (candidate.EmployeeId, candidate.Date))
            .Concat(snapshot.ServiceManagementAssignments.Select(assignment =>
                (assignment.EmployeeId, assignment.Date)))
            .Concat(snapshot.AvailabilityEntries.Select(entry =>
                (entry.EmployeeId, entry.Date)))
            .ToHashSet();
        HashSet<(Guid EmployeeId, DateOnly Date)> expected = snapshot.Employees
            .SelectMany(employee => Enumerable.Range(0, 21).Select(index =>
                (employee.Id, snapshot.PeriodMonday.AddDays(index))))
            .Where(day => !occupied.Contains(day))
            .ToHashSet();
        HashSet<(Guid EmployeeId, DateOnly Date)> actual = dayOffs
            .Select(dayOff => (dayOff.EmployeeId, dayOff.Date))
            .ToHashSet();
        return expected.SetEquals(actual) && actual.Count == dayOffs.Count;
    }
}
