using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class AutomaticSchedulePlanningReportCalculatorTests
{
    private static readonly DateOnly PeriodMonday = new(2026, 9, 21);
    private static readonly Guid SnapshotId = Guid.Parse(
        "10000000-0000-4000-8000-000000000001");
    private static readonly Guid DraftId = Guid.Parse(
        "10000000-0000-4000-8000-000000000002");
    private static readonly Guid ServiceEmployeeId = Guid.Parse(
        "20000000-0000-4000-8000-000000000001");
    private static readonly Guid NormalEmployeeId = Guid.Parse(
        "20000000-0000-4000-8000-000000000002");
    private static readonly Guid AuxiliaryEmployeeId = Guid.Parse(
        "20000000-0000-4000-8000-000000000003");
    private static readonly Guid SecondNormalEmployeeId = Guid.Parse(
        "20000000-0000-4000-8000-000000000004");
    private static readonly Guid FullyAbsentEmployeeId = Guid.Parse(
        "20000000-0000-4000-8000-000000000005");
    private static readonly Guid ServiceTypeId = Guid.Parse(
        "30000000-0000-4000-8000-000000000001");
    private static readonly Guid NormalTypeId = Guid.Parse(
        "30000000-0000-4000-8000-000000000002");
    private static readonly Guid AuxiliaryTypeId = Guid.Parse(
        "30000000-0000-4000-8000-000000000003");
    private static readonly Guid RestaurantId = Guid.Parse(
        "40000000-0000-4000-8000-000000000001");
    private static readonly Guid CafeteriaId = Guid.Parse(
        "40000000-0000-4000-8000-000000000002");
    private static readonly Guid EarlyShiftId = Guid.Parse(
        "50000000-0000-4000-8000-000000000001");
    private static readonly Guid LateShiftId = Guid.Parse(
        "50000000-0000-4000-8000-000000000002");
    private static readonly Guid CafeShiftId = Guid.Parse(
        "50000000-0000-4000-8000-000000000003");

    [Fact]
    public void CreateCalculatesDemandMinutesTargetsAndPatternCountsExactly()
    {
        PlanningInputSnapshot input = CreateInput();
        AutomaticScheduleProposal proposal = CreateProposal(input);

        AutomaticSchedulePlanningReport report =
            AutomaticSchedulePlanningReportCalculator.Create(input, proposal);

        Assert.Equal(SnapshotId, report.SnapshotId);
        Assert.Equal(8, report.Demands.Count);
        Assert.Equal(
            5,
            report.Demands.Count(value =>
                value.Status == PlanningDemandCoverageStatus.FullyCovered));
        PlanningDemandCoverageSnapshot partial = Assert.Single(report.Demands, value =>
            value.Status == PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift);
        Assert.Equal(120, partial.CoveredMinutes);
        Assert.Equal(60, partial.OpenMinutes);
        Assert.Collection(
            partial.CoveredIntervals,
            interval =>
            {
                Assert.Equal(new TimeOnly(17, 30), interval.Start);
                Assert.Equal(new TimeOnly(19, 30), interval.End);
            });
        Assert.Collection(
            partial.OpenIntervals,
            interval =>
            {
                Assert.Equal(new TimeOnly(16, 30), interval.Start);
                Assert.Equal(new TimeOnly(17, 30), interval.End);
            });

        Assert.Equal(3, report.DemandWeeks.Count);
        PlanningDemandSummarySnapshot firstWeek = report.DemandWeeks[0];
        Assert.Equal(6, firstWeek.RequiredPersonCount);
        Assert.Equal(5, firstWeek.CoveredPersonCount);
        Assert.Equal(2, firstWeek.OpenPersonCount);
        Assert.Equal(1_560, firstWeek.RequiredMinutes);
        Assert.Equal(1_320, firstWeek.CoveredMinutes);
        Assert.Equal(240, firstWeek.OpenMinutes);
        Assert.Equal(4, firstWeek.FullyCoveredPersonCount);
        Assert.Equal(1, firstWeek.PartiallyCoveredPersonCount);
        Assert.Equal(1, firstWeek.UncoveredPersonCount);
        Assert.Equal(84.6m, firstWeek.CoveragePercentage);

        PlanningDemandSummarySnapshot secondWeek = report.DemandWeeks[1];
        Assert.Equal(2, secondWeek.RequiredPersonCount);
        Assert.Equal(120, secondWeek.CoveredMinutes);
        Assert.Equal(120, secondWeek.OpenMinutes);
        Assert.Equal(50m, secondWeek.CoveragePercentage);
        Assert.Null(report.DemandWeeks[2].CoveragePercentage);
        Assert.Collection(
            report.Demands.Where(value => value.Date == PeriodMonday.AddDays(7)),
            first =>
            {
                Assert.Equal("Cafeteria", first.WorkLocationName);
                Assert.Equal(new TimeOnly(14, 0), first.DemandStart);
                Assert.Equal(1, first.Ordinal);
            },
            second => Assert.Equal(2, second.Ordinal));
        Assert.Equal(1_800, report.DemandTotal.RequiredMinutes);
        Assert.Equal(1_440, report.DemandTotal.CoveredMinutes);
        Assert.Equal(360, report.DemandTotal.OpenMinutes);
        Assert.Equal(
            report.Demands.Sum(value => value.RequiredMinutes),
            report.DemandTotal.RequiredMinutes);
        Assert.Equal(
            report.Demands.Sum(value => value.CoveredMinutes),
            report.DemandTotal.CoveredMinutes);
        Assert.Equal(
            report.Demands.Sum(value => value.OpenMinutes),
            report.DemandTotal.OpenMinutes);

        PlanningEmployeeWeekSnapshot serviceWeek = report.EmployeeWeeks[0];
        Assert.Equal(ServiceEmployeeId, serviceWeek.EmployeeId);
        Assert.Equal(60, serviceWeek.PlannedWorkMinutes);
        Assert.Equal(
            1,
            Assert.Single(serviceWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.OfficeTime).Count);

        PlanningEmployeeWeekSnapshot normalWeek = Assert.Single(
            report.EmployeeWeeks,
            value => value.EmployeeId == NormalEmployeeId
                && value.WeekMonday == PeriodMonday);
        Assert.Equal(2_400, normalWeek.WeeklyTargetMinutes);
        Assert.Equal(1_920, normalWeek.EffectiveTargetMinutes);
        Assert.Equal(1, normalWeek.VacationDayCount);
        Assert.Equal(0, normalWeek.SicknessDayCount);
        Assert.Equal(1_450, normalWeek.PlannedWorkMinutes);
        Assert.Equal(-470, normalWeek.DifferenceMinutes);
        Assert.Equal(
            1,
            Assert.Single(normalWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.NormalShift
                && value.Code == "F").Count);
        Assert.Equal(
            1,
            Assert.Single(normalWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.SplitShift).Count);
        Assert.Equal(
            1,
            Assert.Single(normalWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.ReliefShift).Count);
        Assert.Equal(
            0,
            Assert.Single(normalWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.NormalShift
                && value.Code == "S").Count);
        Assert.Equal(
            1,
            Assert.Single(normalWeek.ServiceCounts, value =>
                value.Kind == PlanningServiceCountKind.ManualAdditional
                && value.Code == "S").Count);

        PlanningEmployeeWeekSnapshot auxiliaryWeek = Assert.Single(
            report.EmployeeWeeks,
            value => value.EmployeeId == AuxiliaryEmployeeId
                && value.WeekMonday == PeriodMonday);
        Assert.Equal(0, auxiliaryWeek.PlannedWorkMinutes);
        Assert.All(auxiliaryWeek.ServiceCounts, value => Assert.Equal(0, value.Count));
        Assert.Contains(EarlyShiftId, auxiliaryWeek.ComparableShiftTypeIds);
        Assert.Contains(LateShiftId, auxiliaryWeek.ComparableShiftTypeIds);

        PlanningEmployeeWeekSnapshot absentWeek = Assert.Single(
            report.EmployeeWeeks,
            value => value.EmployeeId == FullyAbsentEmployeeId
                && value.WeekMonday == PeriodMonday);
        Assert.Equal(0, absentWeek.EffectiveTargetMinutes);
        Assert.Equal(7, absentWeek.VacationDayCount);
        Assert.Empty(absentWeek.ComparableShiftTypeIds);

        Assert.Equal(7, report.ServiceColumns.Count);
        Assert.Equal(
            ["CB", "F", "S", "D", "Spr", "B", "S"],
            report.ServiceColumns.Select(value => value.Code));
        Assert.DoesNotContain(report.ServiceColumns, value => value.Code == "X");
        Assert.Equal(
            PlanningServiceCountKind.ManualAdditional,
            report.ServiceColumns[^1].Kind);
        PlanningEmployeeWeekSummarySnapshot employeeSummary =
            report.EmployeeWeekSummaries[0];
        Assert.Equal(1, employeeSummary.SplitShiftCount);
        Assert.Equal(1, employeeSummary.ReliefShiftCount);
        PlanningShiftDistributionSnapshot earlyDistribution = Assert.Single(
            employeeSummary.ShiftDistributions,
            value => value.Code == "F");
        Assert.Equal(1, earlyDistribution.TotalCount);
        Assert.Equal(3, earlyDistribution.ComparablePersonCount);
        Assert.Equal(0, earlyDistribution.MinimumCount);
        Assert.Equal(1, earlyDistribution.MaximumCount);
        Assert.Equal(1, earlyDistribution.Spread);
        PlanningShiftDistributionSnapshot lateDistribution = Assert.Single(
            employeeSummary.ShiftDistributions,
            value => value.Code == "S");
        Assert.Equal(1, lateDistribution.TotalCount);
        Assert.Equal(3, lateDistribution.ComparablePersonCount);
        Assert.Equal(0, lateDistribution.MinimumCount);
        Assert.Equal(1, lateDistribution.MaximumCount);
        Assert.Equal(1, lateDistribution.Spread);
        Assert.Equal(15, report.EmployeeWeeks.Count);
    }

    [Fact]
    public void CreateRejectsOpenDemandThatDoesNotMatchCalculatedCoverage()
    {
        PlanningInputSnapshot input = CreateInput();
        AutomaticScheduleProposal proposal = CreateProposal(input, openMinuteOverride: 30);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            AutomaticSchedulePlanningReportCalculator.Create(input, proposal));

        Assert.Contains("open demand", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateRejectsOpenDemandWithCorrectMinutesButWrongInterval()
    {
        PlanningInputSnapshot input = CreateInput();
        AutomaticScheduleProposal proposal = CreateProposal(
            input,
            partialOpenStartOverride: new TimeOnly(16, 45));

        Assert.Throws<InvalidOperationException>(() =>
            AutomaticSchedulePlanningReportCalculator.Create(input, proposal));
    }

    [Fact]
    public void ReportCopiesSourceCollectionsAndKeepsStableOrdering()
    {
        PlanningInputSnapshot input = CreateInput();
        AutomaticScheduleProposal proposal = CreateProposal(input);

        AutomaticSchedulePlanningReport report =
            AutomaticSchedulePlanningReportCalculator.Create(input, proposal);

        Assert.Equal(
            [
                ServiceEmployeeId,
                ServiceEmployeeId,
                ServiceEmployeeId,
                NormalEmployeeId,
                NormalEmployeeId,
                NormalEmployeeId,
                SecondNormalEmployeeId,
                SecondNormalEmployeeId,
                SecondNormalEmployeeId,
                FullyAbsentEmployeeId,
                FullyAbsentEmployeeId,
                FullyAbsentEmployeeId,
                AuxiliaryEmployeeId,
                AuxiliaryEmployeeId,
                AuxiliaryEmployeeId,
            ],
            report.EmployeeWeeks.Select(value => value.EmployeeId));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<PlanningEmployeeWeekSnapshot>)report.EmployeeWeeks).Clear());
    }

    private static PlanningInputSnapshot CreateInput()
    {
        PlanningEmployeeSnapshot[] employees =
        [
            new(NormalEmployeeId, "Nora", "Normal", NormalTypeId),
            new(SecondNormalEmployeeId, "Sven", "Spät", NormalTypeId),
            new(FullyAbsentEmployeeId, "Frieda", "Vollabwesend", NormalTypeId),
            new(AuxiliaryEmployeeId, "Anton", "Aushilfe", AuxiliaryTypeId),
            new(ServiceEmployeeId, "Silke", "Leitung", ServiceTypeId),
        ];
        PlanningEmployeeTypeSnapshot[] employeeTypes =
        [
            EmployeeType(ServiceTypeId, "Typ1", 2_400, EmployeeTypePlanningRole.ServiceManagement),
            EmployeeType(
                NormalTypeId,
                "Typ40",
                2_400,
                EmployeeTypePlanningRole.Normal,
                StandardEligibilities()),
            EmployeeType(
                AuxiliaryTypeId,
                "AH",
                600,
                EmployeeTypePlanningRole.Auxiliary,
                StandardEligibilities()),
        ];
        PlanningServiceCatalogSnapshot catalog = new(
            [
                new PlanningWorkLocationSnapshot(RestaurantId, "Restaurant", "#000000"),
                new PlanningWorkLocationSnapshot(CafeteriaId, "Cafeteria", "#FFFFFF"),
            ],
            [
                new PlanningShiftTypeSnapshot(
                    EarlyShiftId,
                    "Frühdienst",
                    RestaurantId,
                    ShiftTypeDisplayKind.Abbreviation,
                    "F",
                    new TimeOnly(6, 30),
                    new TimeOnly(13, 30)),
                new PlanningShiftTypeSnapshot(
                    LateShiftId,
                    "Spätdienst",
                    RestaurantId,
                    ShiftTypeDisplayKind.Abbreviation,
                    "S",
                    new TimeOnly(16, 30),
                    new TimeOnly(19, 30)),
                new PlanningShiftTypeSnapshot(
                    CafeShiftId,
                    "Cafeteria B",
                    CafeteriaId,
                    ShiftTypeDisplayKind.Abbreviation,
                    "CB",
                    new TimeOnly(13, 30),
                    new TimeOnly(17, 30)),
            ],
            new PlanningSplitShiftPatternSnapshot(
                Guid.Parse("60000000-0000-4000-8000-000000000001"),
                RestaurantId,
                EarlyShiftId,
                LateShiftId,
                180,
                600),
            new PlanningReliefShiftPatternSnapshot(
                Guid.Parse("60000000-0000-4000-8000-000000000002"),
                DayOfWeek.Saturday,
                CafeteriaId,
                CafeShiftId,
                RestaurantId,
                LateShiftId,
                ReliefShiftSwitchRule.EndOfFirstActualDemand,
                false));
        ScheduleDemandSlotSnapshot[] demands =
        [
            Demand(1, 0, EarlyShiftId, "Frühdienst", new TimeOnly(6, 30), new TimeOnly(13, 30)),
            Demand(2, 1, LateShiftId, "Spätdienst", new TimeOnly(16, 30), new TimeOnly(19, 30)),
            Demand(3, 3, EarlyShiftId, "Frühdienst", new TimeOnly(6, 30), new TimeOnly(13, 30)),
            Demand(4, 3, LateShiftId, "Spätdienst", new TimeOnly(16, 30), new TimeOnly(19, 30)),
            Demand(5, 5, LateShiftId, "Spätdienst", new TimeOnly(16, 30), new TimeOnly(19, 30)),
            Demand(
                6,
                7,
                CafeShiftId,
                "Cafeteria B",
                new TimeOnly(14, 0),
                new TimeOnly(16, 0),
                CafeteriaId,
                "Cafeteria",
                1),
            Demand(
                6,
                7,
                CafeShiftId,
                "Cafeteria B",
                new TimeOnly(14, 0),
                new TimeOnly(16, 0),
                CafeteriaId,
                "Cafeteria",
                2),
            Demand(7, 2, LateShiftId, "Spätdienst", new TimeOnly(16, 30), new TimeOnly(19, 30)),
        ];
        return new PlanningInputSnapshot(
            SnapshotId,
            DraftId,
            7,
            PeriodMonday,
            PeriodMonday.AddDays(20),
            employees,
            employeeTypes,
            catalog,
            [
                new PlanningAvailabilityEntrySnapshot(
                    NormalEmployeeId,
                    PeriodMonday.AddDays(2),
                    AvailabilityEntryKind.Vacation,
                    1),
                new PlanningAvailabilityEntrySnapshot(
                    AuxiliaryEmployeeId,
                    PeriodMonday.AddDays(4),
                    AvailabilityEntryKind.FixedDayOff,
                    1),
                .. Enumerable.Range(0, 7).Select(index =>
                    new PlanningAvailabilityEntrySnapshot(
                        FullyAbsentEmployeeId,
                        PeriodMonday.AddDays(index),
                        AvailabilityEntryKind.Vacation,
                        index + 1)),
            ],
            demands,
            [OfficeAssignment(), ManualAdditionalAssignment(demands[1])],
            new RuleCatalogSnapshot(1, []),
            PlanningRunOptions.Default,
            new PlanningHistorySnapshot(PlanningHistoryCompleteness.Missing, []));
    }

    private static PlanningEmployeeTypeSnapshot EmployeeType(
        Guid id,
        string code,
        int targetMinutes,
        EmployeeTypePlanningRole role,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot>? eligibilities = null) => new(
            id,
            code,
            code,
            targetMinutes,
            true,
            480,
            role,
            role != EmployeeTypePlanningRole.ServiceManagement,
            role == EmployeeTypePlanningRole.ServiceManagement,
            role == EmployeeTypePlanningRole.ServiceManagement,
            ManualSuggestionPriority.Standard,
            eligibilities ?? []);

    private static PlanningEmployeeTypeEligibilitySnapshot[] StandardEligibilities() =>
    [
        ShiftEligibility(EarlyShiftId),
        ShiftEligibility(LateShiftId),
        ShiftEligibility(CafeShiftId),
    ];

    private static PlanningEmployeeTypeEligibilitySnapshot ShiftEligibility(
        Guid shiftTypeId) => new(
            ShiftEligibilityTargetKind.ShiftType,
            shiftTypeId,
            ShiftEligibilityMode.Regular,
            ShiftEligibilityActivation.Always);

    private static ScheduleDemandSlotSnapshot Demand(
        int source,
        int day,
        Guid shiftTypeId,
        string shiftName,
        TimeOnly start,
        TimeOnly end,
        Guid? workLocationId = null,
        string workLocationName = "Restaurant",
        int ordinal = 1) => new(
            Guid.Parse($"70000000-0000-4000-8000-{source:D12}"),
            ScheduleDemandSourceKindSnapshot.Standard,
            PeriodMonday.AddDays(day),
            workLocationId ?? RestaurantId,
            workLocationName,
            shiftTypeId,
            shiftName,
            ordinal,
            start,
            end,
            (int)(end - start).TotalMinutes,
            ScheduleShiftDisplayKindSnapshot.Abbreviation,
            shiftTypeId == EarlyShiftId ? "F" : "S");

    private static AutomaticScheduleProposal CreateProposal(
        PlanningInputSnapshot input,
        int? openMinuteOverride = null,
        TimeOnly? partialOpenStartOverride = null)
    {
        ScheduleDemandSlotSnapshot[] demands = input.DemandSlots.ToArray();
        ScheduleAssignmentSnapshot normal = Assignment(
            1,
            ScheduleAssignmentKindSnapshot.NormalDemand,
            420,
            [Segment(demands[0])],
            [Coverage(demands[0])]);
        ScheduleAssignmentSnapshot split = Assignment(
            2,
            ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            600,
            [Segment(demands[2]), Segment(demands[3])],
            [Coverage(demands[2]), Coverage(demands[3])]);
        ScheduleAssignmentSnapshot relief = Assignment(
            3,
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
            400,
            [
                new ScheduleAssignmentSegmentSnapshot(
                    demands[4].SourceId,
                    demands[4].Date,
                    CafeteriaId,
                    CafeShiftId,
                    new TimeOnly(13, 30),
                    new TimeOnly(17, 30),
                    240),
                new ScheduleAssignmentSegmentSnapshot(
                    demands[4].SourceId,
                    demands[4].Date,
                    RestaurantId,
                    LateShiftId,
                    new TimeOnly(17, 30),
                    new TimeOnly(20, 10),
                    160),
            ],
            [
                new ScheduleDemandCoverageSnapshot(
                    demands[4].SourceId,
                    demands[4].Date,
                    demands[4].WorkLocationId,
                    demands[4].ShiftTypeId,
                    demands[4].Ordinal,
                    new TimeOnly(17, 30),
                    new TimeOnly(19, 30),
                    120,
                    ScheduleDemandCoverageKindSnapshot.PartialReliefShift),
            ]);
        ScheduleAssignmentSnapshot secondWeek = Assignment(
            5,
            ScheduleAssignmentKindSnapshot.NormalDemand,
            120,
            [Segment(demands[5])],
            [Coverage(demands[5])]);
        ScheduleAssignmentSnapshot late = Assignment(
            6,
            ScheduleAssignmentKindSnapshot.NormalDemand,
            180,
            [Segment(demands[7])],
            [Coverage(demands[7])],
            SecondNormalEmployeeId);
        int partialOpenMinutes = openMinuteOverride ?? 60;
        TimeOnly partialOpenStart = partialOpenStartOverride
            ?? demands[4].ActualStart;
        AutomaticScheduleOpenDemand[] openDemands =
        [
            new(
                demands[1].SourceId,
                demands[1].Date,
                demands[1].WorkLocationId,
                demands[1].ShiftTypeId,
                demands[1].Ordinal,
                demands[1].ActualStart,
                demands[1].ActualEnd,
                demands[1].DurationMinutes,
                AutomaticScheduleOpenDemandKind.FullyUncovered),
            new(
                demands[4].SourceId,
                demands[4].Date,
                demands[4].WorkLocationId,
                demands[4].ShiftTypeId,
                demands[4].Ordinal,
                partialOpenStart,
                partialOpenStart.AddMinutes(partialOpenMinutes),
                partialOpenMinutes,
                AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift),
            new(
                demands[6].SourceId,
                demands[6].Date,
                demands[6].WorkLocationId,
                demands[6].ShiftTypeId,
                demands[6].Ordinal,
                demands[6].ActualStart,
                demands[6].ActualEnd,
                demands[6].DurationMinutes,
                AutomaticScheduleOpenDemandKind.FullyUncovered),
        ];
        return new AutomaticScheduleProposal(
            input.Id,
            input.DraftId,
            input.DraftVersion,
            [normal, split, relief, secondWeek, late],
            [new AutomaticScheduleDayOffProposal(
                AuxiliaryEmployeeId,
                PeriodMonday.AddDays(6))],
            openDemands,
            new ScheduleObjectiveVector(
                openDemands.Sum(value => value.UncoveredMinutes),
                2,
                RuleViolationSet.Empty,
                1,
                1,
                AuxiliaryWeeklyMinimumObjective.Empty,
                RelativeWeeklyTargetObjective.Empty,
                RuleViolationSet.Empty,
                RuleViolationSet.Empty,
                RuleViolationSet.Empty,
                ["synthetic"]),
            EmptyEvaluations(),
            new AutomaticScheduleRunMetadata(
                "Synthetic solver",
                "1.0",
                AutomaticSchedulePlanningStatus.Optimal,
                TimeSpan.FromMinutes(2),
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                []));
    }

    private static ScheduleRuleEvaluationSet EmptyEvaluations()
    {
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        return new ScheduleRuleEvaluationSet(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.NotApplicable,
                NoRuleResultParameters.Instance)));
    }

    private static ScheduleAssignmentSnapshot Assignment(
        int id,
        ScheduleAssignmentKindSnapshot kind,
        int workMinutes,
        IEnumerable<ScheduleAssignmentSegmentSnapshot> segments,
        IEnumerable<ScheduleDemandCoverageSnapshot> coverages,
        Guid? employeeId = null)
    {
        ScheduleAssignmentSegmentSnapshot[] segmentValues = segments.ToArray();
        return new ScheduleAssignmentSnapshot(
            Guid.Parse($"80000000-0000-4000-8000-{id:D12}"),
            employeeId ?? NormalEmployeeId,
            segmentValues[0].Date,
            kind,
            ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
            kind switch
            {
                ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                    Guid.Parse("60000000-0000-4000-8000-000000000001"),
                ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                    Guid.Parse("60000000-0000-4000-8000-000000000002"),
                _ => null,
            },
            workMinutes,
            false,
            segmentValues,
            coverages);
    }

    private static ScheduleAssignmentSnapshot OfficeAssignment() => new(
        Guid.Parse("80000000-0000-4000-8000-000000000004"),
        ServiceEmployeeId,
        PeriodMonday,
        ScheduleAssignmentKindSnapshot.OfficeTime,
        ScheduleAssignmentOriginSnapshot.ServiceManagement,
        null,
        60,
        true,
        [],
            []);

    private static ScheduleAssignmentSnapshot ManualAdditionalAssignment(
        ScheduleDemandSlotSnapshot demand) => new(
            Guid.Parse("80000000-0000-4000-8000-000000000007"),
            NormalEmployeeId,
            demand.Date,
            ScheduleAssignmentKindSnapshot.ManualAdditional,
            ScheduleAssignmentOriginSnapshot.ManualEdit,
            null,
            30,
            false,
            [
                new ScheduleAssignmentSegmentSnapshot(
                    demand.SourceId,
                    demand.Date,
                    demand.WorkLocationId,
                    demand.ShiftTypeId,
                    new TimeOnly(18, 0),
                    new TimeOnly(18, 30),
                    30),
            ],
            []);

    private static ScheduleAssignmentSegmentSnapshot Segment(
        ScheduleDemandSlotSnapshot demand) => new(
            demand.SourceId,
            demand.Date,
            demand.WorkLocationId,
            demand.ShiftTypeId,
            demand.ActualStart,
            demand.ActualEnd,
            demand.DurationMinutes);

    private static ScheduleDemandCoverageSnapshot Coverage(
        ScheduleDemandSlotSnapshot demand) => new(
            demand.SourceId,
            demand.Date,
            demand.WorkLocationId,
            demand.ShiftTypeId,
            demand.Ordinal,
            demand.ActualStart,
            demand.ActualEnd,
            demand.DurationMinutes,
            ScheduleDemandCoverageKindSnapshot.Full);
}
