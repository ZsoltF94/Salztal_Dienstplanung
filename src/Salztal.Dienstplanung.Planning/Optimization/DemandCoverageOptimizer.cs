using System.Diagnostics;
using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Rules.HardRules;
using Salztal.Dienstplanung.Planning.Rules.SoftRules;
using Salztal.Dienstplanung.Planning.Rules.Stability;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal static class DemandCoverageOptimizer
{
    public static DemandCoverageOptimizationResult Optimize(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet) => Optimize(
            snapshot,
            candidateSet,
            TimeSpan.FromDays(1),
            CancellationToken.None).Result;

    public static AutomaticScheduleOptimizationRun Optimize(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        TimeSpan timeLimit,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(candidateSet);
        PlanningSolveBudget budget = new(timeLimit, cancellationToken);
        Stopwatch solveStopwatch = Stopwatch.StartNew();
        try
        {
            DemandCoverageOptimizationResult result = OptimizeJoint(
                snapshot,
                candidateSet,
                budget);
            return new AutomaticScheduleOptimizationRun(
                result,
                true,
                solveStopwatch.Elapsed);
        }
        catch (PlanningTimeLimitWithFeasibleSelectionException exception)
        {
            return new AutomaticScheduleOptimizationRun(
                CreateFallbackResult(snapshot, candidateSet, exception.SelectedCandidateKeys),
                false,
                solveStopwatch.Elapsed);
        }
    }

    private static DemandCoverageOptimizationResult OptimizeJoint(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        PlanningSolveBudget budget)
    {
        PhaseOptimum regularOptimum = FindJointRegularOptimum(
            snapshot,
            candidateSet,
            budget);
        PlanningDemandKey[] reliefLateDemands = candidateSet.Candidates
            .Where(candidate => candidate.Kind == PlanningCandidateKind.ReliefShiftPattern)
            .Select(candidate => candidate.Coverages[1].Demand)
            .Distinct()
            .Order()
            .ToArray();
        StructuralPlanningModel model = CreateJointHardRuleModel(
            snapshot,
            candidateSet,
            new ReliefShiftEmergencyGate(reliefLateDemands),
            out HardRulePlanningContext context);
        AddRegularOptimumBounds(model, regularOptimum);
        _ = MaximizeAndFreeze(
            model,
            CoveredMinutesExpression(model, includeRelief: true),
            budget);
        _ = MaximizeAndFreeze(
            model,
            TouchedFullDemandExpression(model, includeRelief: true),
            budget);
        HighPriorityRuleModel highRules = HighPriorityRuleModelBuilder.Apply(context);
        long minimumHighViolationCount = MinimizeAndFreeze(
            model,
            highRules.ViolationCount,
            budget);
        ParetoStage highStage = new(
            CurrentSoftRuleDefinitions.All
                .Where(rule => rule.Priority == RulePriority.High)
                .ToArray(),
            highRules.Magnitude);
        LinearExpr reliefCount = PatternCount(
            model,
            PlanningCandidateKind.ReliefShiftPattern);
        long minimumReliefCount = MinimizeAndFreezeWithParetoValidation(
            model,
            reliefCount,
            [highStage],
            budget);
        LinearExpr splitCount = PatternCount(
            model,
            PlanningCandidateKind.SplitShiftPattern);
        long minimumSplitCount = MinimizeAndFreezeWithParetoValidation(
            model,
            splitCount,
            [highStage],
            budget);
        JointWeeklyObjectiveModel weekly = JointWeeklyObjectiveModelBuilder.Apply(context);
        long minimumAuxiliaryViolationCount = MinimizeAndFreezeWithParetoValidation(
            model,
            weekly.AuxiliaryViolationCount,
            [highStage],
            budget);
        long minimumAuxiliaryMissingMinutes = MinimizeAndFreezeWithParetoValidation(
            model,
            weekly.AuxiliaryMissingMinutes,
            [highStage],
            budget);
        MinimizeRelativeWeeklyTargetsAndFreeze(
            model,
            weekly.RelativeTargetCases,
            [highStage],
            budget);
        MediumPriorityRuleModel mediumRules = MediumPriorityRuleModelBuilder.Apply(context);
        ParetoStage mediumStage = new(
            [CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend],
            mediumRules.Magnitude);
        long minimumMediumViolationCount = MinimizeAndFreezeWithParetoValidation(
            model,
            mediumRules.ViolationCount,
            [highStage],
            budget);
        FairDistributionRuleModel stability = FairDistributionRuleModelBuilder.Apply(context);
        _ = MinimizeAndFreezeWithParetoValidation(
            model,
            stability.TotalSpread,
            [highStage, mediumStage],
            budget);
        CpSolver solver = FindStableTechnicalSolution(
            model,
            [highStage, mediumStage],
            budget);
        PlanningAssignmentCandidate[] selected = candidateSet.Candidates
            .Where(candidate => solver.Value(
                model.CandidateVariables[candidate.TechnicalKey]) == 1)
            .ToArray();
        string[] selectedKeys = selected.Select(candidate => candidate.TechnicalKey).ToArray();
        AutomaticHardRuleSelectionEvaluation hard =
            AutomaticHardRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        HighPriorityRuleSelectionEvaluation high =
            HighPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        MediumPriorityRuleSelectionEvaluation medium =
            MediumPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        FairDistributionRuleSelectionEvaluation fair =
            FairDistributionRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        JointPlanningSelectionEvaluation joint =
            JointPlanningSelectionEvaluator.Evaluate(context, selectedKeys);
        if (!hard.IsValid
            || high.Violations.Count != minimumHighViolationCount
            || selected.Count(candidate => candidate.Kind
                == PlanningCandidateKind.ReliefShiftPattern) != minimumReliefCount
            || selected.Count(candidate => candidate.Kind
                == PlanningCandidateKind.SplitShiftPattern) != minimumSplitCount
            || joint.AuxiliaryMinimum.ViolatedWeekCount
                != minimumAuxiliaryViolationCount
            || joint.AuxiliaryMinimum.MissingMinutes
                != minimumAuxiliaryMissingMinutes
            || medium.Violations.Count != minimumMediumViolationCount
            || weekly.RelativeTargetCases.Any(item =>
                solver.Value(item.WeeklyMinutes)
                    != joint.WeeklyMinutes[(item.EmployeeId, item.WeekMonday)]))
        {
            throw new InvalidOperationException(
                "The joint optimization failed independent objective validation.");
        }

        return new DemandCoverageOptimizationResult(
            selected,
            CreateOpenDemands(candidateSet, selected),
            hard,
            high,
            medium,
            fair,
            joint);
    }

    private static PhaseOptimum FindJointRegularOptimum(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        PlanningSolveBudget budget)
    {
        StructuralPlanningModel model = CreateJointHardRuleModel(
            snapshot,
            candidateSet,
            ReliefShiftEmergencyGate.None,
            out _);
        long coveredMinutes = MaximizeAndFreeze(
            model,
            CoveredMinutesExpression(model, includeRelief: false),
            budget);
        long touchedDemands = MaximizeAndFreeze(
            model,
            TouchedFullDemandExpression(model, includeRelief: false),
            budget);
        return new PhaseOptimum(coveredMinutes, touchedDemands);
    }

    private static StructuralPlanningModel CreateJointHardRuleModel(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        ReliefShiftEmergencyGate reliefGate,
        out HardRulePlanningContext context)
    {
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidateSet);
        context = AutomaticHardRuleModelBuilder.Apply(snapshot, model, reliefGate);
        return model;
    }

    private static LinearExpr PatternCount(
        StructuralPlanningModel model,
        PlanningCandidateKind kind) => LinearExpr.Sum(model.CandidateSet.Candidates
        .Where(candidate => candidate.Kind == kind)
        .Select(candidate => model.CandidateVariables[candidate.TechnicalKey]));

    private static void MinimizeRelativeWeeklyTargetsAndFreeze(
        StructuralPlanningModel model,
        IReadOnlyList<RelativeWeeklyTargetCaseModel> cases,
        IReadOnlyList<ParetoStage> higherStages,
        PlanningSolveBudget budget)
    {
        RelativeWeeklyTargetCaseModel[] comparable = cases
            .Where(item => item.TargetMinutes > 0)
            .ToArray();
        if (comparable.Length == 0)
        {
            return;
        }

        ExactFraction[] fractions = comparable
            .SelectMany(item => Enumerable.Range(0, item.MaximumDeviation + 1)
                .Select(deviation => ExactFraction.Create(
                    deviation,
                    item.TargetMinutes)))
            .Distinct()
            .Order()
            .ToArray();
        Dictionary<ExactFraction, int> ranks = fractions
            .Select((fraction, index) => (fraction, index))
            .ToDictionary(item => item.fraction, item => item.index);
        List<IntVar> rankVariables = [];
        foreach (RelativeWeeklyTargetCaseModel item in comparable)
        {
            long[] rankByDeviation = Enumerable.Range(0, item.MaximumDeviation + 1)
                .Select(deviation => (long)ranks[ExactFraction.Create(
                    deviation,
                    item.TargetMinutes)])
                .ToArray();
            IntVar rank = model.Model.NewIntVar(
                0,
                fractions.Length - 1,
                $"relative-rank-{item.EmployeeId:N}-{item.WeekMonday:yyyyMMdd}");
            model.Model.AddElement(item.AbsoluteDeviation, rankByDeviation, rank);
            rankVariables.Add(rank);
        }

        List<List<BoolVar>> excludedLevelIndicators = rankVariables
            .Select(_ => new List<BoolVar>())
            .ToList();
        int fixedCaseCount = 0;
        while (fixedCaseCount < rankVariables.Count)
        {
            IntVar[] remainingRanks = rankVariables.Select((rank, index) =>
                RemainingRank(
                    model,
                    rank,
                    excludedLevelIndicators[index],
                    fractions.Length - 1,
                    index)).ToArray();
            IntVar maximum = model.Model.NewIntVar(
                0,
                fractions.Length - 1,
                $"relative-remaining-maximum-{fixedCaseCount}");
            model.Model.AddMaxEquality(maximum, remainingRanks);
            long level = MinimizeAndFreezeWithParetoValidation(
                model,
                maximum,
                higherStages,
                budget);
            if (level == 0)
            {
                return;
            }

            BoolVar[] atLevel = rankVariables.Select((rank, index) =>
                EqualityIndicator(
                    model,
                    rank,
                    level,
                    $"relative-level-{level}-{index}")).ToArray();
            long levelCount = MinimizeAndFreezeWithParetoValidation(
                model,
                LinearExpr.Sum(atLevel),
                higherStages,
                budget);
            fixedCaseCount = checked(fixedCaseCount + (int)levelCount);
            for (int index = 0; index < atLevel.Length; index++)
            {
                excludedLevelIndicators[index].Add(atLevel[index]);
            }
        }
    }

    private static IntVar RemainingRank(
        StructuralPlanningModel model,
        IntVar rank,
        List<BoolVar> excludedLevels,
        int maximumRank,
        int index)
    {
        if (excludedLevels.Count == 0)
        {
            return rank;
        }

        BoolVar excluded = model.Model.NewBoolVar(
            $"relative-excluded-{excludedLevels.Count}-{index}");
        model.Model.AddMaxEquality(excluded, excludedLevels);
        IntVar remaining = model.Model.NewIntVar(
            0,
            maximumRank,
            $"relative-remaining-{excludedLevels.Count}-{index}");
        model.Model.Add(remaining == 0).OnlyEnforceIf(excluded);
        model.Model.Add(remaining == rank).OnlyEnforceIf(excluded.Not());
        return remaining;
    }

    private static BoolVar EqualityIndicator(
        StructuralPlanningModel model,
        IntVar value,
        long expected,
        string name)
    {
        BoolVar result = model.Model.NewBoolVar(name);
        model.Model.Add(value == expected).OnlyEnforceIf(result);
        model.Model.Add(value != expected).OnlyEnforceIf(result.Not());
        return result;
    }
    private static void AddRegularOptimumBounds(
        StructuralPlanningModel model,
        PhaseOptimum regularOptimum)
    {
        model.Model.Add(
            CoveredMinutesExpression(model, includeRelief: false)
            == regularOptimum.CoveredMinutes);
        model.Model.Add(
            TouchedFullDemandExpression(model, includeRelief: false)
            == regularOptimum.TouchedFullDemands);
    }

    private static LinearExpr CoveredMinutesExpression(
        StructuralPlanningModel model,
        bool includeRelief)
    {
        PlanningAssignmentCandidate[] candidates = model.CandidateSet.Candidates
            .ToArray();
        return LinearExpr.WeightedSum(
            candidates.Select(candidate =>
                model.CandidateVariables[candidate.TechnicalKey]),
            candidates.Select(candidate =>
                (long)(includeRelief
                    ? candidate.Coverages.Sum(coverage => coverage.CoveredMinutes)
                    : RegularCoverage(candidate).Sum(coverage =>
                        coverage.CoveredMinutes))));
    }

    private static LinearExpr TouchedFullDemandExpression(
        StructuralPlanningModel model,
        bool includeRelief)
    {
        HashSet<PlanningDemandKey> fullyUncoveredDemands = model.CandidateSet
            .RemainingDemands
            .Where(demand => demand.Kind == PlanningRemainingDemandKind.FullyUncovered)
            .Select(demand => demand.Demand)
            .ToHashSet();
        return LinearExpr.Sum(model.CandidateSet.Candidates
            .SelectMany(candidate => (includeRelief
                    ? candidate.Coverages
                    : RegularCoverage(candidate))
                .Where(coverage => fullyUncoveredDemands.Contains(coverage.Demand))
                .Select(_ => model.CandidateVariables[candidate.TechnicalKey])));
    }

    private static IEnumerable<PlanningCandidateCoverage> RegularCoverage(
        PlanningAssignmentCandidate candidate) =>
        candidate.Kind == PlanningCandidateKind.ReliefShiftPattern
            ? candidate.Coverages.Take(1)
            : candidate.Coverages;

    private static long MaximizeAndFreeze(
        StructuralPlanningModel model,
        LinearExpr expression,
        PlanningSolveBudget budget)
    {
        model.Model.Maximize(expression);
        CpSolver solver = SolveOptimal(model, budget);
        long optimum = checked((long)Math.Round(solver.ObjectiveValue));
        model.Model.Add(expression == optimum);
        return optimum;
    }

    private static long MinimizeAndFreeze(
        StructuralPlanningModel model,
        LinearExpr expression,
        PlanningSolveBudget budget)
    {
        model.Model.Minimize(expression);
        CpSolver solver = SolveOptimal(model, budget);
        long optimum = checked((long)Math.Round(solver.ObjectiveValue));
        model.Model.Add(expression == optimum);
        return optimum;
    }

    private static long MinimizeAndFreezeWithParetoValidation(
        StructuralPlanningModel model,
        LinearExpr expression,
        IReadOnlyList<ParetoStage> higherStages,
        PlanningSolveBudget budget)
    {
        while (true)
        {
            model.Model.Minimize(expression);
            CpSolver candidate = SolveOptimal(model, budget);
            if (AddCutForFirstDominatedStage(
                    model,
                    candidate,
                    higherStages,
                    budget))
            {
                continue;
            }

            long optimum = checked((long)Math.Round(candidate.ObjectiveValue));
            model.Model.Add(expression == optimum);
            return optimum;
        }
    }

    private static CpSolver FindStableTechnicalSolution(
        StructuralPlanningModel model,
        IReadOnlyList<ParetoStage> higherStages,
        PlanningSolveBudget budget)
    {
        model.Model.Model.Objective = null;
        model.Model.AddDecisionStrategy(
            model.CandidateSet.Candidates.Select(candidate =>
                model.CandidateVariables[candidate.TechnicalKey]),
            DecisionStrategyProto.Types.VariableSelectionStrategy.ChooseFirst,
            DecisionStrategyProto.Types.DomainReductionStrategy.SelectMaxValue);
        while (true)
        {
            CpSolver candidate = SolveFeasible(model, budget, fixedSearch: true);
            if (!AddCutForFirstDominatedStage(
                    model,
                    candidate,
                    higherStages,
                    budget))
            {
                return candidate;
            }
        }
    }

    private static bool AddCutForFirstDominatedStage(
        StructuralPlanningModel model,
        CpSolver candidate,
        IReadOnlyList<ParetoStage> stages,
        PlanningSolveBudget budget)
    {
        for (int stageIndex = 0; stageIndex < stages.Count; stageIndex++)
        {
            CpSolver? dominator = FindDominator(
                model,
                stages[stageIndex],
                candidate,
                budget);
            if (dominator is null)
            {
                continue;
            }

            for (int priorIndex = 0; priorIndex < stageIndex; priorIndex++)
            {
                CpSolver? priorDominator = FindDominator(
                    model,
                    stages[priorIndex],
                    dominator,
                    budget);
                if (priorDominator is not null)
                {
                    AddDominatedRegionExclusion(
                        model,
                        stages[priorIndex],
                        priorDominator);
                    return true;
                }
            }

            AddDominatedRegionExclusion(model, stages[stageIndex], dominator);
            return true;
        }

        return false;
    }

    private static CpSolver? FindDominator(
        StructuralPlanningModel model,
        ParetoStage stage,
        CpSolver incumbent,
        PlanningSolveBudget budget)
    {
        BoolVar probe = model.Model.NewBoolVar(
            $"dominance-probe-{model.Model.Model.Variables.Count}");
        long[] magnitudes = stage.Rules.Select(rule =>
            incumbent.Value(stage.Magnitude(rule))).ToArray();
        foreach ((RuleDefinition rule, long magnitude) in stage.Rules.Zip(magnitudes))
        {
            model.Model.Add(stage.Magnitude(rule) <= magnitude).OnlyEnforceIf(probe);
        }

        model.Model.Add(LinearExpr.Sum(stage.Rules.Select(stage.Magnitude))
            <= magnitudes.Sum() - 1).OnlyEnforceIf(probe);
        model.Model.AddAssumption(probe);
        CpSolver? result = TrySolveFeasible(model, budget);
        model.Model.Model.Assumptions.Clear();
        model.Model.Add(probe == 0);
        return result;
    }

    private static void AddDominatedRegionExclusion(
        StructuralPlanningModel model,
        ParetoStage stage,
        CpSolver dominator)
    {
        List<BoolVar> improvements = [];
        List<BoolVar> degradations = [];
        foreach (RuleDefinition rule in stage.Rules)
        {
            LinearExpr magnitude = stage.Magnitude(rule);
            long boundary = dominator.Value(magnitude);
            string suffix = $"{rule.Id.Value}-{model.Model.Model.Variables.Count}";
            BoolVar improvement = model.Model.NewBoolVar($"pareto-better-{suffix}");
            model.Model.Add(magnitude <= boundary - 1).OnlyEnforceIf(improvement);
            model.Model.Add(magnitude >= boundary).OnlyEnforceIf(improvement.Not());
            BoolVar degradation = model.Model.NewBoolVar($"pareto-worse-{suffix}");
            model.Model.Add(magnitude >= boundary + 1).OnlyEnforceIf(degradation);
            model.Model.Add(magnitude <= boundary).OnlyEnforceIf(degradation.Not());
            improvements.Add(improvement);
            degradations.Add(degradation);
        }

        foreach (BoolVar degradation in degradations)
        {
            model.Model.AddBoolOr(improvements).OnlyEnforceIf(degradation);
        }
    }

    private static CpSolver SolveOptimal(
        StructuralPlanningModel model,
        PlanningSolveBudget budget)
    {
        CpSolver solver = budget.CreateSolver();
        using CancellationTokenRegistration registration =
            budget.RegisterCancellation(solver);
        CpSolverStatus status = solver.Solve(model.Model);
        if (status == CpSolverStatus.Optimal)
        {
            budget.RecordSelection(model, solver);
            return solver;
        }

        if (status == CpSolverStatus.Feasible)
        {
            budget.RecordSelection(model, solver);
        }

        budget.ThrowForStoppedSearch(status);
        throw new UnreachableException();
    }

    private static CpSolver SolveFeasible(
        StructuralPlanningModel model,
        PlanningSolveBudget budget,
        bool fixedSearch = false) =>
        TrySolveFeasible(model, budget, fixedSearch)
        ?? throw new InvalidOperationException("The frozen optimization model is infeasible.");

    private static CpSolver? TrySolveFeasible(
        StructuralPlanningModel model,
        PlanningSolveBudget budget,
        bool fixedSearch = false)
    {
        CpSolver solver = budget.CreateSolver(fixedSearch);
        using CancellationTokenRegistration registration =
            budget.RegisterCancellation(solver);
        CpSolverStatus status = solver.Solve(model.Model);
        if (status is CpSolverStatus.Optimal or CpSolverStatus.Feasible)
        {
            budget.RecordSelection(model, solver);
            return solver;
        }

        if (status == CpSolverStatus.Infeasible)
        {
            return null;
        }

        budget.ThrowForStoppedSearch(status);
        throw new UnreachableException();
    }

    private static DemandCoverageOptimizationResult CreateFallbackResult(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        IEnumerable<string> selectedCandidateKeys)
    {
        HashSet<string> selectedKeys = selectedCandidateKeys.ToHashSet(StringComparer.Ordinal);
        PlanningAssignmentCandidate[] selected = candidateSet.Candidates
            .Where(candidate => selectedKeys.Contains(candidate.TechnicalKey))
            .ToArray();
        if (selected.Length != selectedKeys.Count)
        {
            throw new RetainedSelectionContainsUnknownCandidateException();
        }

        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidateSet);
        PlanningDemandKey[] reliefLateDemands = candidateSet.Candidates
            .Where(candidate => candidate.Kind == PlanningCandidateKind.ReliefShiftPattern)
            .Select(candidate => candidate.Coverages[1].Demand)
            .Distinct()
            .Order()
            .ToArray();
        HardRulePlanningContext context = AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            model,
            new ReliefShiftEmergencyGate(reliefLateDemands));
        AutomaticHardRuleSelectionEvaluation hard =
            AutomaticHardRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        if (!hard.IsValid)
        {
            throw new RetainedSelectionFailedHardRuleValidationException(
                string.Join(',', hard.Results
                    .Where(result => result.Status == RuleEvaluationStatus.Violated)
                    .Select(result => result.RuleId.Value)));
        }

        return new DemandCoverageOptimizationResult(
            selected,
            CreateOpenDemands(candidateSet, selected),
            hard,
            HighPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys),
            MediumPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys),
            FairDistributionRuleSelectionEvaluator.Evaluate(context, selectedKeys),
            JointPlanningSelectionEvaluator.Evaluate(context, selectedKeys));
    }

    private static IEnumerable<AutomaticScheduleOpenDemand> CreateOpenDemands(
        PlanningCandidateSet candidateSet,
        IEnumerable<PlanningAssignmentCandidate> selectedCandidates)
    {
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage> coverages =
            selectedCandidates
                .SelectMany(candidate => candidate.Coverages)
                .ToDictionary(coverage => coverage.Demand);
        foreach (PlanningRemainingDemand remaining in candidateSet.RemainingDemands)
        {
            if (!coverages.TryGetValue(
                    remaining.Demand,
                    out PlanningCandidateCoverage? coverage))
            {
                yield return CreateOpenDemand(
                    remaining,
                    remaining.UncoveredStart,
                    remaining.UncoveredEnd,
                    remaining.Kind == PlanningRemainingDemandKind.FullyUncovered
                        ? AutomaticScheduleOpenDemandKind.FullyUncovered
                        : AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift);
                continue;
            }

            if (coverage.CoveredStart > remaining.UncoveredStart)
            {
                yield return CreateOpenDemand(
                    remaining,
                    remaining.UncoveredStart,
                    coverage.CoveredStart,
                    AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift);
            }

            if (coverage.CoveredEnd < remaining.UncoveredEnd)
            {
                yield return CreateOpenDemand(
                    remaining,
                    coverage.CoveredEnd,
                    remaining.UncoveredEnd,
                    AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift);
            }
        }
    }

    private static AutomaticScheduleOpenDemand CreateOpenDemand(
        PlanningRemainingDemand remaining,
        TimeOnly start,
        TimeOnly end,
        AutomaticScheduleOpenDemandKind kind) => new(
            remaining.Demand.SourceId,
            remaining.Demand.Date,
            remaining.Demand.WorkLocationId,
            remaining.Demand.ShiftTypeId,
            remaining.Demand.Ordinal,
            start,
            end,
            (int)(end - start).TotalMinutes,
            kind);

    private sealed record PhaseOptimum(
        long CoveredMinutes,
        long TouchedFullDemands);

    private sealed record ParetoStage(
        IReadOnlyList<RuleDefinition> Rules,
        Func<RuleDefinition, LinearExpr> Magnitude);

    private readonly record struct ExactFraction(long Numerator, long Denominator) :
        IComparable<ExactFraction>
    {
        public static ExactFraction Create(long numerator, long denominator)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(numerator);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(denominator);
            long divisor = GreatestCommonDivisor(numerator, denominator);
            return new ExactFraction(numerator / divisor, denominator / divisor);
        }

        public int CompareTo(ExactFraction other)
        {
            Int128 left = (Int128)Numerator * other.Denominator;
            Int128 right = (Int128)other.Numerator * Denominator;
            return left.CompareTo(right);
        }

        private static long GreatestCommonDivisor(long left, long right)
        {
            while (right != 0)
            {
                (left, right) = (right, left % right);
            }

            return left;
        }
    }
}

internal sealed class RetainedSelectionContainsUnknownCandidateException : Exception;

internal sealed class RetainedSelectionFailedHardRuleValidationException(
    string technicalContext) : Exception, IAutomaticScheduleTechnicalContextProvider
{
    public string TechnicalContext { get; } = technicalContext;
}
