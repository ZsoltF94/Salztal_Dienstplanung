using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling.Optimization;

public sealed class ScheduleObjectiveComparerTests
{
    [Fact]
    public void UncoveredMinutesWhenLowerWinBeforeEveryLaterImprovement()
    {
        ScheduleObjectiveVector left = CreateVector(
            uncoveredMinutes: 59,
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 900)),
            medium: Violations((InitialSoftRuleDefinitions.MinimizeSplitShifts, 30)));
        ScheduleObjectiveVector right = CreateVector(uncoveredMinutes: 60);

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.UncoveredEmployeeMinutes, comparison.DecisiveStage);
    }

    [Fact]
    public void UncoveredSlotsWhenMinutesTieWinBeforeSoftRules()
    {
        ScheduleObjectiveVector left = CreateVector(
            uncoveredMinutes: 420,
            uncoveredSlots: 1,
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 900)));
        ScheduleObjectiveVector right = CreateVector(
            uncoveredMinutes: 420,
            uncoveredSlots: 2);

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.FullyUncoveredDemandSlots, comparison.DecisiveStage);
    }

    [Fact]
    public void HigherPriorityRuleStageWinsBeforeManyLowerViolations()
    {
        ScheduleObjectiveVector left = CreateVector(
            medium: Violations(
                (InitialSoftRuleDefinitions.MinimizeSplitShifts, 10),
                (InitialSoftRuleDefinitions.ThreeWeekFreeWeekend, 10)));
        ScheduleObjectiveVector right = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 1)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.HighPriorityRules, comparison.DecisiveStage);
    }

    [Fact]
    public void ReliefShiftCountWinsBeforeSplitShiftCountAndLaterTargets()
    {
        ScheduleObjectiveVector left = CreateVector(
            reliefShiftCount: 0,
            splitShiftCount: 9,
            auxiliaryMinimum: AuxiliaryMinimum((179, true)));
        ScheduleObjectiveVector right = CreateVector(
            reliefShiftCount: 1,
            splitShiftCount: 0);

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.ReliefShiftAssignments, comparison.DecisiveStage);
    }

    [Fact]
    public void SplitShiftCountWinsBeforeAuxiliaryMinimum()
    {
        ScheduleObjectiveVector left = CreateVector(
            splitShiftCount: 0,
            auxiliaryMinimum: AuxiliaryMinimum((0, true)));
        ScheduleObjectiveVector right = CreateVector(splitShiftCount: 1);

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.SplitShiftAssignments, comparison.DecisiveStage);
    }

    [Fact]
    public void AuxiliaryMinimumCountsAffectedWeeksBeforeMissingMinutes()
    {
        ScheduleObjectiveVector left = CreateVector(
            auxiliaryMinimum: AuxiliaryMinimum((179, true), (179, true)));
        ScheduleObjectiveVector right = CreateVector(
            auxiliaryMinimum: AuxiliaryMinimum((1, true)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Worse, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.AuxiliaryWeeklyMinimum, comparison.DecisiveStage);
    }

    [Fact]
    public void AuxiliaryMinimumUsesOneHundredEightyMinutesOnlyWithEligibleDemand()
    {
        AuxiliaryWeeklyMinimumObjective objective = AuxiliaryMinimum(
            (179, true),
            (180, true),
            (359, true),
            (360, true),
            (600, true),
            (720, true),
            (721, true),
            (0, false));

        Assert.Equal(1, objective.ViolatedWeekCount);
        Assert.Equal(1, objective.MissingMinutes);
    }

    [Theory]
    [InlineData(600)]
    [InlineData(1200)]
    [InlineData(1500)]
    [InlineData(1800)]
    [InlineData(2100)]
    [InlineData(2400)]
    [InlineData(900)]
    public void RelativeTargetUsesExactRatioForEveryConfirmedWeeklyTarget(
        int targetMinutes)
    {
        Guid employeeId = Guid.Parse("10000000-0000-4000-8000-000000000001");
        RelativeWeeklyTargetObjective closer = RelativeTarget(
            (employeeId, targetMinutes - (targetMinutes / 10), targetMinutes));
        RelativeWeeklyTargetObjective farther = RelativeTarget(
            (employeeId, targetMinutes - (targetMinutes / 5), targetMinutes));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
            CreateVector(relativeTarget: closer),
            CreateVector(relativeTarget: farther));

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.RelativeWeeklyTarget, comparison.DecisiveStage);
    }

    [Fact]
    public void RelativeTargetComparesWorstDeviationThenRemainingDeviationsExactly()
    {
        Guid first = Guid.Parse("10000000-0000-4000-8000-000000000001");
        Guid second = Guid.Parse("20000000-0000-4000-8000-000000000001");
        RelativeWeeklyTargetObjective fairer = RelativeTarget(
            (first, 510, 600),
            (second, 1020, 1200));
        RelativeWeeklyTargetObjective lessFair = RelativeTarget(
            (first, 480, 600),
            (second, 1080, 1200));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
            CreateVector(relativeTarget: fairer),
            CreateVector(relativeTarget: lessFair));

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.RelativeWeeklyTarget, comparison.DecisiveStage);
    }

    [Fact]
    public void RelativeTargetExcludesZeroTargetsWithoutUsingTechnicalIdentity()
    {
        Guid zeroTargetEmployee = Guid.Parse("10000000-0000-4000-8000-000000000001");
        Guid comparedEmployee = Guid.Parse("20000000-0000-4000-8000-000000000001");
        RelativeWeeklyTargetObjective left = RelativeTarget(
            (zeroTargetEmployee, 720, 0),
            (comparedEmployee, 540, 600));
        RelativeWeeklyTargetObjective right = RelativeTarget(
            (zeroTargetEmployee, 0, 0),
            (comparedEmployee, 540, 600));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
            CreateVector(relativeTarget: left),
            CreateVector(relativeTarget: right));

        Assert.Equal(1, left.ExcludedZeroTargetCaseCount);
        Assert.Equal(ScheduleObjectiveComparisonOutcome.Equivalent, comparison.Outcome);
    }

    [Fact]
    public void ViolationCaseCountWinsBeforeMagnitudeWithinSameStage()
    {
        ScheduleObjectiveVector left = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 600)));
        ScheduleObjectiveVector right = CreateVector(
            high: Violations(
                (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 1),
                (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 1)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.HighPriorityRules, comparison.DecisiveStage);
    }

    [Fact]
    public void MagnitudeIsComparedWithinSameRuleOnly()
    {
        ScheduleObjectiveVector left = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 60)));
        ScheduleObjectiveVector right = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 120)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.HighPriorityRules, comparison.DecisiveStage);
    }

    [Fact]
    public void ConflictingMagnitudesAcrossDifferentRulesStayEqualUntilNextStage()
    {
        ScheduleObjectiveVector left = CreateVector(
            high: Violations(
                (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 60),
                (InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum, 2)),
            medium: RuleViolationSet.Empty);
        ScheduleObjectiveVector right = CreateVector(
            high: Violations(
                (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 120),
                (InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum, 1)),
            medium: Violations((InitialSoftRuleDefinitions.MinimizeSplitShifts, 1)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.MediumPriorityRules, comparison.DecisiveStage);
    }

    [Fact]
    public void TechnicalTieBreakerUsesStableSortedKeysLast()
    {
        string[] mutableKeys = ["assignment-b", "assignment-a"];
        ScheduleObjectiveVector left = CreateVector(technicalKeys: mutableKeys);
        mutableKeys[0] = "changed-after-construction";
        ScheduleObjectiveVector right = CreateVector(technicalKeys: ["assignment-c"]);

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(["assignment-a", "assignment-b"], left.TechnicalTieBreakerKeys);
        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.TechnicalTieBreaker, comparison.DecisiveStage);
    }

    [Fact]
    public void SameVectorIsEquivalentWithoutDecisiveStage()
    {
        ScheduleObjectiveVector left = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 60)));
        ScheduleObjectiveVector right = CreateVector(
            high: Violations((InitialSoftRuleDefinitions.NormalWeeklyMinimum, 60)));

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Equivalent, comparison.Outcome);
        Assert.Null(comparison.DecisiveStage);
    }

    [Theory]
    [InlineData(ScheduleObjectiveStage.MediumPriorityRules)]
    [InlineData(ScheduleObjectiveStage.LowPriorityRules)]
    [InlineData(ScheduleObjectiveStage.Stability)]
    public void EveryLaterRuleStageCanDecideComparison(
        ScheduleObjectiveStage expectedStage)
    {
        RuleViolationSet violation = expectedStage == ScheduleObjectiveStage.Stability
            ? Violations((InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution, 1))
            : Violations((CreateSyntheticRule(expectedStage), 1));
        ScheduleObjectiveVector left = CreateVector();
        ScheduleObjectiveVector right = expectedStage switch
        {
            ScheduleObjectiveStage.MediumPriorityRules => CreateVector(medium: violation),
            ScheduleObjectiveStage.LowPriorityRules => CreateVector(low: violation),
            ScheduleObjectiveStage.Stability => CreateVector(stability: violation),
            _ => throw new InvalidOperationException(),
        };

        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(left, right);

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(expectedStage, comparison.DecisiveStage);
    }

    [Fact]
    public void ComparisonIsSymmetricForBetterAndWorseResults()
    {
        ScheduleObjectiveVector better = CreateVector(uncoveredMinutes: 60);
        ScheduleObjectiveVector worse = CreateVector(uncoveredMinutes: 61);

        Assert.Equal(
            ScheduleObjectiveComparisonOutcome.Better,
            ScheduleObjectiveComparer.Compare(better, worse).Outcome);
        Assert.Equal(
            ScheduleObjectiveComparisonOutcome.Worse,
            ScheduleObjectiveComparer.Compare(worse, better).Outcome);
    }

    [Fact]
    public void RuleViolationInWrongPriorityStageIsRejected()
    {
        RuleViolationSet mediumViolation = Violations(
            (InitialSoftRuleDefinitions.MinimizeSplitShifts, 1));

        Assert.Throws<ArgumentException>(() => new ScheduleObjectiveVector(
            0,
            0,
            mediumViolation,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            ["assignment-a"]));
    }

    private static ScheduleObjectiveVector CreateVector(
        int uncoveredMinutes = 0,
        int uncoveredSlots = 0,
        RuleViolationSet? high = null,
        RuleViolationSet? medium = null,
        RuleViolationSet? low = null,
        RuleViolationSet? stability = null,
        int reliefShiftCount = 0,
        int splitShiftCount = 0,
        AuxiliaryWeeklyMinimumObjective? auxiliaryMinimum = null,
        RelativeWeeklyTargetObjective? relativeTarget = null,
        IEnumerable<string>? technicalKeys = null)
    {
        return new ScheduleObjectiveVector(
            uncoveredMinutes,
            uncoveredSlots,
            high ?? RuleViolationSet.Empty,
            reliefShiftCount,
            splitShiftCount,
            auxiliaryMinimum ?? AuxiliaryWeeklyMinimumObjective.Empty,
            relativeTarget ?? RelativeWeeklyTargetObjective.Empty,
            medium ?? RuleViolationSet.Empty,
            low ?? RuleViolationSet.Empty,
            stability ?? RuleViolationSet.Empty,
            technicalKeys ?? ["assignment-a"]);
    }

    private static AuxiliaryWeeklyMinimumObjective AuxiliaryMinimum(
        params (int AssignedMinutes, bool HasEligibleDemand)[] values)
    {
        DateOnly weekMonday = new(2026, 9, 14);
        return new AuxiliaryWeeklyMinimumObjective(values.Select((value, index) =>
            new AuxiliaryWeeklyMinimumCase(
                Guid.Parse($"{index + 1:00000000}-0000-4000-8000-000000000001"),
                weekMonday,
                value.AssignedMinutes,
                value.HasEligibleDemand)));
    }

    private static RelativeWeeklyTargetObjective RelativeTarget(
        params (Guid EmployeeId, int AssignedMinutes, int TargetMinutes)[] values)
    {
        DateOnly weekMonday = new(2026, 9, 14);
        return new RelativeWeeklyTargetObjective(values.Select(value =>
            new RelativeWeeklyTargetCase(
                value.EmployeeId,
                weekMonday,
                value.AssignedMinutes,
                value.TargetMinutes)));
    }

    private static RuleViolationSet Violations(
        params (RuleDefinition Rule, long Magnitude)[] values)
    {
        return new RuleViolationSet(
            values.Select((value, index) => new RuleViolationCase(
                value.Rule,
                $"case-{index + 1}",
                new RuleViolationMagnitude(value.Magnitude))));
    }

    private static RuleDefinition CreateSyntheticRule(ScheduleObjectiveStage stage)
    {
        RulePriority priority = stage switch
        {
            ScheduleObjectiveStage.MediumPriorityRules => RulePriority.Medium,
            ScheduleObjectiveStage.LowPriorityRules => RulePriority.Low,
            _ => throw new ArgumentOutOfRangeException(nameof(stage)),
        };
        RuleDefinitionValidationResult result = RuleDefinition.Create(
            $"SYNTHETIC_{priority.ToString().ToUpperInvariant()}",
            RuleFamily.Soft,
            RuleScope.PlanningPeriod,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            priority,
            NoRuleParameters.Instance,
            $"rules.synthetic.{priority.ToString().ToLowerInvariant()}");
        return Assert.IsType<RuleDefinition>(result.Value);
    }
}
