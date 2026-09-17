using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;
using Salztal.Dienstplanung.Planning.Rules.SoftRules;
using Salztal.Dienstplanung.Planning.Rules.Stability;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal sealed class DemandCoverageOptimizationResult
{
    public DemandCoverageOptimizationResult(
        IEnumerable<PlanningAssignmentCandidate> selectedCandidates,
        IEnumerable<AutomaticScheduleOpenDemand> openDemands,
        AutomaticHardRuleSelectionEvaluation hardRuleEvaluation,
        HighPriorityRuleSelectionEvaluation highPriorityRuleEvaluation,
        MediumPriorityRuleSelectionEvaluation mediumPriorityRuleEvaluation,
        FairDistributionRuleSelectionEvaluation stabilityRuleEvaluation,
        JointPlanningSelectionEvaluation jointPlanningEvaluation)
    {
        ArgumentNullException.ThrowIfNull(selectedCandidates);
        ArgumentNullException.ThrowIfNull(openDemands);
        ArgumentNullException.ThrowIfNull(hardRuleEvaluation);
        ArgumentNullException.ThrowIfNull(highPriorityRuleEvaluation);
        ArgumentNullException.ThrowIfNull(mediumPriorityRuleEvaluation);
        ArgumentNullException.ThrowIfNull(stabilityRuleEvaluation);
        ArgumentNullException.ThrowIfNull(jointPlanningEvaluation);
        SelectedCandidates = Array.AsReadOnly(selectedCandidates
            .OrderBy(candidate => candidate.TechnicalKey, StringComparer.Ordinal)
            .ToArray());
        OpenDemands = Array.AsReadOnly(openDemands
            .OrderBy(demand => demand.Date)
            .ThenBy(demand => demand.WorkLocationId)
            .ThenBy(demand => demand.ShiftTypeId)
            .ThenBy(demand => demand.SourceId)
            .ThenBy(demand => demand.Ordinal)
            .ThenBy(demand => demand.UncoveredStart)
            .ToArray());
        HardRuleEvaluation = hardRuleEvaluation;
        HighPriorityRuleEvaluation = highPriorityRuleEvaluation;
        MediumPriorityRuleEvaluation = mediumPriorityRuleEvaluation;
        StabilityRuleEvaluation = stabilityRuleEvaluation;
        MediumPriorityViolations = mediumPriorityRuleEvaluation.Violations;
        StabilityViolations = stabilityRuleEvaluation.Violations;
        MediumPriorityRuleResults = Array.AsReadOnly(
            mediumPriorityRuleEvaluation.Results
                .Concat(jointPlanningEvaluation.ObjectiveRuleResults)
                .ToArray());
        NoticeRuleResults = Array.AsReadOnly(
            jointPlanningEvaluation.NoticeResults.ToArray());
        StabilityRuleResult = RuleEvaluationResult.Create(
            InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution.Id,
            StabilityViolations.Count == 0
                ? RuleEvaluationStatus.Satisfied
                : RuleEvaluationStatus.Violated,
            NoRuleResultParameters.Instance);
        UncoveredEmployeeMinutes = OpenDemands.Sum(demand => demand.UncoveredMinutes);
        FullyUncoveredDemandSlotCount = OpenDemands.Count(demand =>
            demand.Kind == AutomaticScheduleOpenDemandKind.FullyUncovered);
        ReliefShiftAssignmentCount = SelectedCandidates.Count(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        SplitShiftAssignmentCount = SelectedCandidates.Count(candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        AuxiliaryWeeklyMinimum = jointPlanningEvaluation.AuxiliaryMinimum;
        RelativeWeeklyTarget = jointPlanningEvaluation.RelativeWeeklyTarget;
    }

    public ReadOnlyCollection<PlanningAssignmentCandidate> SelectedCandidates { get; }

    public ReadOnlyCollection<AutomaticScheduleOpenDemand> OpenDemands { get; }

    public AutomaticHardRuleSelectionEvaluation HardRuleEvaluation { get; }

    public HighPriorityRuleSelectionEvaluation HighPriorityRuleEvaluation { get; }

    public RuleViolationSet HighPriorityViolations =>
        HighPriorityRuleEvaluation.Violations;

    public MediumPriorityRuleSelectionEvaluation MediumPriorityRuleEvaluation { get; }

    public RuleViolationSet MediumPriorityViolations { get; }

    public FairDistributionRuleSelectionEvaluation StabilityRuleEvaluation { get; }

    public RuleViolationSet StabilityViolations { get; }

    public ReadOnlyCollection<RuleEvaluationResult> MediumPriorityRuleResults { get; }

    public ReadOnlyCollection<RuleEvaluationResult> NoticeRuleResults { get; }

    public RuleEvaluationResult StabilityRuleResult { get; }

    public int UncoveredEmployeeMinutes { get; }

    public int FullyUncoveredDemandSlotCount { get; }

    public int ReliefShiftAssignmentCount { get; }

    public int SplitShiftAssignmentCount { get; }

    public AuxiliaryWeeklyMinimumObjective AuxiliaryWeeklyMinimum { get; }

    public RelativeWeeklyTargetObjective RelativeWeeklyTarget { get; }
}
