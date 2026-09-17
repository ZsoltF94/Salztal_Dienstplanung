using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Rules.HardRules;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Rules.HardRules;

public sealed class AutomaticHardRuleModelBuilderTests
{
    [Fact]
    public void RegistryContainsEveryAutomaticHardRuleExactlyOnce()
    {
        IReadOnlyList<IAutomaticHardRuleTranslator> translators =
            AutomaticHardRuleTranslators.CreateAll();

        Assert.Equal(11, translators.Count);
        Assert.Equal(
            InitialAutomaticHardRuleDefinitions.All.Select(rule => rule.Id),
            translators.Select(translator => translator.RuleId));
    }

    [Fact]
    public void NormalWeeklyMaximumUsesActualMinutesAndAllowsExactBoundary()
    {
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            Enumerable.Range(0, 4)
                .Select(index => CreateNormalSlot(
                    PlanningInputMonday.AddDays(index),
                    index,
                    345)));
        StructuralPlanningModel model = Build(snapshot, ReliefShiftEmergencyGate.None);
        PlanningAssignmentCandidate[] assignments = NormalCandidates(model);

        Assert.Equal(4, assignments.Length);
        Assert.Equal(CpSolverStatus.Optimal, SolveSelected(model, assignments));

        PlanningInputSnapshot overMaximum = CreateSnapshotWithNormalSlots(
            Enumerable.Range(0, 4)
                .Select(index => CreateNormalSlot(
                    PlanningInputMonday.AddDays(index),
                    index,
                    index == 3 ? 346 : 345)));
        StructuralPlanningModel overMaximumModel = Build(
            overMaximum,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(overMaximumModel, NormalCandidates(overMaximumModel)));
    }

    [Theory]
    [InlineData(AvailabilityEntryKind.Vacation)]
    [InlineData(AvailabilityEntryKind.Sickness)]
    public void VacationAndSicknessReduceEffectiveWeeklyTarget(
        AvailabilityEntryKind absenceKind)
    {
        ScheduleDemandSlotSnapshot[] slots = Enumerable.Range(0, 2)
            .Select(index => CreateNormalSlot(
                PlanningInputMonday.AddDays(index),
                index,
                600))
            .ToArray();
        PlanningAvailabilityEntrySnapshot reduction = new(
            CandidateScenarioFactory.NormalEmployeeId,
            PlanningInputMonday.AddDays(4),
            absenceKind,
            1);
        PlanningInputSnapshot reduced = CreateSnapshotWithNormalSlots(
            slots,
            availabilityEntries: [reduction]);
        StructuralPlanningModel reducedModel = Build(reduced, ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(reducedModel, NormalCandidates(reducedModel)));
    }

    [Fact]
    public void FixedDayOffDoesNotReduceEffectiveWeeklyTarget()
    {
        ScheduleDemandSlotSnapshot[] slots = Enumerable.Range(0, 2)
            .Select(index => CreateNormalSlot(
                PlanningInputMonday.AddDays(index),
                index,
                600))
            .ToArray();
        PlanningAvailabilityEntrySnapshot fixedDayOff = new(
            CandidateScenarioFactory.NormalEmployeeId,
            PlanningInputMonday.AddDays(4),
            AvailabilityEntryKind.FixedDayOff,
            1);
        PlanningInputSnapshot unchanged = CreateSnapshotWithNormalSlots(
            slots,
            availabilityEntries: [fixedDayOff]);
        StructuralPlanningModel unchangedModel = Build(
            unchanged,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Optimal,
            SolveSelected(unchangedModel, NormalCandidates(unchangedModel)));
    }

    [Fact]
    public void AuxiliaryWeeklyMaximumAllowsTwelveHoursAndRejectsOneMoreMinute()
    {
        PlanningInputSnapshot boundary = CreateSnapshotWithAuxiliarySlots(
            CreateNormalSlot(PlanningInputMonday, 1, 360),
            CreateNormalSlot(PlanningInputMonday.AddDays(1), 2, 360));
        StructuralPlanningModel boundaryModel = Build(
            boundary,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Optimal,
            SolveSelected(boundaryModel, AuxiliaryCandidates(boundaryModel)));

        PlanningInputSnapshot violation = CreateSnapshotWithAuxiliarySlots(
            CreateNormalSlot(PlanningInputMonday, 1, 360),
            CreateNormalSlot(PlanningInputMonday.AddDays(1), 2, 361));
        StructuralPlanningModel violationModel = Build(
            violation,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(violationModel, AuxiliaryCandidates(violationModel)));
    }

    [Fact]
    public void CompleteHistoryRejectsEighthConsecutiveWorkday()
    {
        PlanningHistorySnapshot history = CreateHistory(
            missingIndex: null,
            workingEmployeeId: CandidateScenarioFactory.NormalEmployeeId);
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            [CreateNormalSlot(PlanningInputMonday, 1, 60)],
            history: history);
        StructuralPlanningModel model = Build(snapshot, ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(model, NormalCandidates(model)));
    }

    [Fact]
    public void MissingHistoryIsNotInventedAsWorkOrFree()
    {
        PlanningHistorySnapshot history = CreateHistory(
            missingIndex: 3,
            workingEmployeeId: CandidateScenarioFactory.NormalEmployeeId);
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            [CreateNormalSlot(PlanningInputMonday, 1, 60)],
            history: history);
        StructuralPlanningModel model = Build(snapshot, ReliefShiftEmergencyGate.None);

        Assert.Equal(CpSolverStatus.Optimal, SolveSelected(model, NormalCandidates(model)));
    }

    [Fact]
    public void EightCurrentWorkdaysAreRejectedWhileSevenAreAllowed()
    {
        ScheduleDemandSlotSnapshot[] slots = Enumerable.Range(0, 8)
            .Select(index => CreateNormalSlot(
                PlanningInputMonday.AddDays(index),
                index,
                60))
            .ToArray();
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(slots);
        StructuralPlanningModel sevenModel = Build(snapshot, ReliefShiftEmergencyGate.None);
        PlanningAssignmentCandidate[] seven = NormalCandidates(sevenModel).Take(7).ToArray();

        Assert.Equal(CpSolverStatus.Optimal, SolveSelected(sevenModel, seven));

        StructuralPlanningModel eightModel = Build(snapshot, ReliefShiftEmergencyGate.None);
        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(eightModel, NormalCandidates(eightModel)));
    }

    [Fact]
    public void WeekendAfterFridayVacationIsBlockedAndOtherVacationBoundaryIsNot()
    {
        DateOnly friday = PlanningInputMonday.AddDays(4);
        ScheduleDemandSlotSnapshot saturdaySlot = CreateNormalSlot(
            friday.AddDays(1),
            1,
            60);
        PlanningAvailabilityEntrySnapshot fridayVacation = new(
            CandidateScenarioFactory.NormalEmployeeId,
            friday,
            AvailabilityEntryKind.Vacation,
            1);
        PlanningInputSnapshot blocked = CreateSnapshotWithNormalSlots(
            [saturdaySlot],
            availabilityEntries: [fridayVacation]);
        StructuralPlanningModel blockedModel = Build(blocked, ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(blockedModel, NormalCandidates(blockedModel)));

        PlanningInputSnapshot notApplicable = CreateSnapshotWithNormalSlots(
            [saturdaySlot],
            availabilityEntries:
            [
                fridayVacation with
                {
                    Date = friday.AddDays(-1),
                },
            ]);
        StructuralPlanningModel notApplicableModel = Build(
            notApplicable,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Optimal,
            SolveSelected(notApplicableModel, NormalCandidates(notApplicableModel)));
    }

    [Fact]
    public void ReliefShiftRequiresConfirmedRestaurantLateEmergency()
    {
        PlanningInputSnapshot snapshot = CandidateScenarioFactory.Create(
            enableAuxiliaryReliefShift: true);
        PlanningCandidateSet candidateSet = PlanningCandidateBuilder.Build(snapshot);
        PlanningAssignmentCandidate relief = candidateSet.Candidates.First(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        StructuralPlanningModel blockedModel = StructuralPlanningModelBuilder.Build(candidateSet);
        AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            blockedModel,
            ReliefShiftEmergencyGate.None);

        Assert.Equal(
            CpSolverStatus.Infeasible,
            SolveSelected(blockedModel, [relief]));

        StructuralPlanningModel allowedModel = StructuralPlanningModelBuilder.Build(candidateSet);
        ReliefShiftEmergencyGate gate = new([relief.Coverages[1].Demand]);
        AutomaticHardRuleModelBuilder.Apply(snapshot, allowedModel, gate);

        Assert.Equal(CpSolverStatus.Optimal, SolveSelected(allowedModel, [relief]));
    }

    [Fact]
    public void HardRulesLeaveDemandOpenInsteadOfSelectingForbiddenCandidate()
    {
        PlanningHistorySnapshot history = CreateHistory(
            missingIndex: null,
            workingEmployeeId: CandidateScenarioFactory.NormalEmployeeId);
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            [CreateNormalSlot(PlanningInputMonday, 1, 60)],
            history: history);
        StructuralPlanningModel model = Build(snapshot, ReliefShiftEmergencyGate.None);
        CpSolver solver = CreateSolver();

        CpSolverStatus status = solver.Solve(model.Model);

        Assert.Equal(CpSolverStatus.Optimal, status);
        Assert.All(model.CandidateVariables.Values, variable =>
            Assert.Equal(0, solver.Value(variable)));
    }

    [Fact]
    public void IndependentEvaluationDetectsRemovedWeeklyMaximumConstraint()
    {
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            Enumerable.Range(0, 4)
                .Select(index => CreateNormalSlot(
                    PlanningInputMonday.AddDays(index),
                    index,
                    346)));
        StructuralPlanningModel structurallyOnly = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        HardRulePlanningContext context = new(
            snapshot,
            structurallyOnly,
            ReliefShiftEmergencyGate.None);
        string[] selected = NormalCandidates(structurallyOnly)
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(context, selected);

        Assert.False(evaluation.IsValid);
        Assert.Equal(
            RuleEvaluationStatus.Violated,
            evaluation.Results.Single(result => result.RuleId
                == InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum.Id).Status);
    }

    [Fact]
    public void IndependentEvaluationMarksOnlyWorkdayRuleAsNotFullyEvaluable()
    {
        PlanningHistorySnapshot history = CreateHistory(
            missingIndex: 3,
            workingEmployeeId: CandidateScenarioFactory.NormalEmployeeId);
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            [CreateNormalSlot(PlanningInputMonday, 1, 60)],
            history: history);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        HardRulePlanningContext context = new(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(
                context,
                NormalCandidates(model).Select(candidate => candidate.TechnicalKey));

        Assert.True(evaluation.IsValid);
        Assert.Single(evaluation.Results, result =>
            result.Status == RuleEvaluationStatus.NotFullyEvaluable
            && result.RuleId
                == InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays.Id);
    }

    [Fact]
    public void IndependentEvaluationReturnsAllElevenRulesInCatalogOrder()
    {
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots([]);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        HardRulePlanningContext context = new(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(context, []);

        Assert.Equal(
            InitialAutomaticHardRuleDefinitions.All.Select(rule => rule.Id),
            evaluation.Results.Select(result => result.RuleId));
        Assert.True(evaluation.IsValid);
    }

    [Fact]
    public void SplitPatternPermissionDoesNotRequireStandaloneShiftPermission()
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot sourceType = CandidateScenarioFactory
            .CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        PlanningEmployeeTypeSnapshot type = CloneType(
            sourceType,
            sourceType.Eligibilities.Select(eligibility =>
                eligibility.TargetId == CandidateScenarioFactory.EarlyShiftId
                    ? eligibility with { Mode = ShiftEligibilityMode.ManualSuggestion }
                    : eligibility));
        PlanningInputSnapshot snapshot = CandidateScenarioFactory.Create(
            employees: [employee],
            employeeTypes: [type]);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        PlanningAssignmentCandidate split = model.CandidateSet.Candidates.Single(candidate =>
            candidate.EmployeeId == employee.Id
            && candidate.Kind == PlanningCandidateKind.SplitShiftPattern
            && candidate.Coverages[0].Demand.Ordinal == 1
            && candidate.Coverages[1].Demand.Ordinal == 1);
        HardRulePlanningContext context = new(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(
                context,
                [split.TechnicalKey]);

        Assert.Equal(
            RuleEvaluationStatus.Satisfied,
            evaluation.Results.Single(result => result.RuleId
                == InitialAutomaticHardRuleDefinitions.ShiftEligibilityRequired.Id).Status);
        Assert.True(evaluation.IsValid);
    }

    [Fact]
    public void IndependentEvaluationRejectsUnconfirmedReliefMutation()
    {
        PlanningInputSnapshot snapshot = CandidateScenarioFactory.Create(
            employees: CandidateScenarioFactory.CreateEmployees().Where(employee =>
                employee.Id == CandidateScenarioFactory.NormalEmployeeId),
            employeeTypes: CandidateScenarioFactory.CreateEmployeeTypes().Where(type =>
                type.Id == CandidateScenarioFactory.NormalTypeId),
            enableAuxiliaryReliefShift: true);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        PlanningAssignmentCandidate relief = model.CandidateSet.Candidates.First(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        HardRulePlanningContext context = new(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(
                context,
                [relief.TechnicalKey]);

        Assert.False(evaluation.IsValid);
        Assert.Equal(
            RuleEvaluationStatus.Violated,
            evaluation.Results.Single(result => result.RuleId
                == InitialAutomaticHardRuleDefinitions.ReliefShiftEmergencyOnly.Id).Status);
    }

    [Fact]
    public void CpSatMaximumMatchesIndependentHardRuleReferenceSolver()
    {
        PlanningInputSnapshot snapshot = CreateSnapshotWithNormalSlots(
            Enumerable.Range(0, 5)
                .Select(index => CreateNormalSlot(
                    PlanningInputMonday.AddDays(index),
                    index,
                    345)));
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        HardRulePlanningContext context = AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);
        int referenceMaximum =
            ExhaustiveHardRuleReferenceSolver.FindMaximumCoveredMinutes(context);
        model.Model.Maximize(LinearExpr.WeightedSum(
            model.CandidateSet.Candidates.Select(candidate =>
                model.CandidateVariables[candidate.TechnicalKey]),
            model.CandidateSet.Candidates.Select(candidate =>
                (long)candidate.Coverages.Sum(coverage => coverage.CoveredMinutes))));
        CpSolver solver = CreateSolver();

        CpSolverStatus status = solver.Solve(model.Model);

        Assert.Equal(CpSolverStatus.Optimal, status);
        Assert.Equal(referenceMaximum, (int)solver.ObjectiveValue);
        Assert.Equal(1_380, referenceMaximum);
    }

    private static DateOnly PlanningInputMonday => new(2026, 9, 21);

    private static StructuralPlanningModel Build(
        PlanningInputSnapshot snapshot,
        ReliefShiftEmergencyGate gate)
    {
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(
            PlanningCandidateBuilder.Build(snapshot));
        AutomaticHardRuleModelBuilder.Apply(snapshot, model, gate);
        return model;
    }

    private static CpSolverStatus SolveSelected(
        StructuralPlanningModel model,
        IEnumerable<PlanningAssignmentCandidate> selected)
    {
        foreach (PlanningAssignmentCandidate candidate in selected)
        {
            model.Model.Add(model.CandidateVariables[candidate.TechnicalKey] == 1);
        }

        return CreateSolver().Solve(model.Model);
    }

    private static CpSolver CreateSolver() => new()
    {
        StringParameters = "num_search_workers:1 random_seed:0",
    };

    private static PlanningAssignmentCandidate[] NormalCandidates(
        StructuralPlanningModel model) => model.CandidateSet.Candidates
        .Where(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId
            && candidate.Kind == PlanningCandidateKind.NormalDemand)
        .OrderBy(candidate => candidate.Date)
        .ToArray();

    private static PlanningAssignmentCandidate[] AuxiliaryCandidates(
        StructuralPlanningModel model) => model.CandidateSet.Candidates
        .Where(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
            && candidate.Kind == PlanningCandidateKind.NormalDemand)
        .OrderBy(candidate => candidate.Date)
        .ToArray();

    private static PlanningInputSnapshot CreateSnapshotWithNormalSlots(
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availabilityEntries = null,
        PlanningHistorySnapshot? history = null)
    {
        PlanningEmployeeSnapshot normal = CandidateScenarioFactory.CreateEmployees().Single(
            employee => employee.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == normal.EmployeeTypeId);
        return Clone(
            CandidateScenarioFactory.Create(),
            [normal],
            [type],
            availabilityEntries ?? [],
            slots,
            history);
    }

    private static PlanningInputSnapshot CreateSnapshotWithAuxiliarySlots(
        params ScheduleDemandSlotSnapshot[] slots)
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.AuxiliaryEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        return Clone(
            CandidateScenarioFactory.Create(),
            [employee],
            [type],
            [],
            slots,
            null);
    }

    private static PlanningInputSnapshot Clone(
        PlanningInputSnapshot source,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<PlanningEmployeeTypeSnapshot> types,
        IEnumerable<PlanningAvailabilityEntrySnapshot> availabilityEntries,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        PlanningHistorySnapshot? history) => new(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            employees,
            types,
            source.ServiceCatalog,
            availabilityEntries,
            slots,
            [],
            source.RuleCatalog,
            source.RunOptions,
            history ?? source.History);

    private static PlanningEmployeeTypeSnapshot CloneType(
        PlanningEmployeeTypeSnapshot source,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot> eligibilities) => new(
            source.Id,
            source.Code,
            source.Name,
            source.WeeklyWorkTargetMinutes,
            source.AllowsVacationAndSickness,
            source.AbsenceDayValueMinutes,
            source.PlanningRole,
            source.AllowsAutomaticAssignment,
            source.RequiresWeeklyManualAssignment,
            source.PreservesManualAssignmentsOnGeneration,
            source.ManualSuggestionPriority,
            eligibilities);

    private static ScheduleDemandSlotSnapshot CreateNormalSlot(
        DateOnly date,
        int ordinal,
        int minutes)
    {
        TimeOnly start = new(6, 0);
        TimeOnly end = start.AddMinutes(minutes);
        return new ScheduleDemandSlotSnapshot(
            Guid.Parse($"92000000-0000-0000-0000-{ordinal + 1:D12}"),
            ScheduleDemandSourceKindSnapshot.Standard,
            date,
            CandidateScenarioFactory.RestaurantId,
            "Restaurant Test",
            CandidateScenarioFactory.LateShiftId,
            "SpÃ¤t Test",
            1,
            start,
            end,
            minutes);
    }

    private static PlanningHistorySnapshot CreateHistory(
        int? missingIndex,
        Guid workingEmployeeId)
    {
        PlanningHistoryDaySnapshot[] days = Enumerable.Range(0, 7)
            .Select(index => new PlanningHistoryDaySnapshot(
                PlanningInputMonday.AddDays(index - 7),
                index == missingIndex
                    ? PlanningHistoryDayStatus.Missing
                    : PlanningHistoryDayStatus.Available,
                index == missingIndex
                    ? []
                    : [new PlanningHistoryAssignmentSnapshot(workingEmployeeId, 60)]))
            .ToArray();
        return new PlanningHistorySnapshot(
            missingIndex is null
                ? PlanningHistoryCompleteness.Complete
                : PlanningHistoryCompleteness.Partial,
            days);
    }
}
