using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class PlanningEmployeeReportViewModelTests
{
    [Fact]
    public void WeekShowsStableRowsTimesAbsencesAndZeroServiceColumns()
    {
        PlanningEmployeeReportViewModel viewModel = new(
            PlanningEmployeeReportTestData.Create());

        Assert.Equal(3, viewModel.Weeks.Count);
        Assert.Equal(4, viewModel.Employees.Count);
        Assert.Equal(
            ["Typ1", "Regulär", "Regulär", "AH"],
            viewModel.Employees.Select(value => value.RoleDisplay));
        Assert.Equal(6, viewModel.ServiceColumns.Count);
        Assert.Equal(
            "S (manuell zusätzlich)",
            viewModel.ServiceColumns[^1].Display);

        PlanningEmployeeReportRowViewModel nora = Assert.Single(
            viewModel.Employees,
            value => value.NameDisplay == "Normal, Nora");
        Assert.Equal("40 Std.", nora.WeeklyTargetDisplay);
        Assert.Equal("32 Std.", nora.EffectiveTargetDisplay);
        Assert.Equal("20 Std. 30 min", nora.PlannedWorkDisplay);
        Assert.Equal("−11 Std. 30 min", nora.DifferenceDisplay);
        Assert.Equal("U: 1 · K: 0", nora.AbsenceDisplay);
        Assert.Equal(
            [2, 0, 1, 0, 0, 1],
            nora.ServiceCounts.Select(value => value.Count));

        PlanningEmployeeReportRowViewModel absent = Assert.Single(
            viewModel.Employees,
            value => value.NameDisplay == "Vollabwesend, Frieda");
        Assert.Equal("±0 Std.", absent.DifferenceDisplay);
        Assert.Equal("U: 5 · K: 0", absent.AbsenceDisplay);
        Assert.All(absent.ServiceCounts, value => Assert.Equal(0, value.Count));
        Assert.DoesNotContain(
            absent.ServiceCounts,
            value => value.IsIncludedInComparison);
    }

    [Fact]
    public void SummaryUsesOnlyEligiblePlanablePeopleAndKeepsPatternTotalsSeparate()
    {
        PlanningEmployeeReportViewModel viewModel = new(
            PlanningEmployeeReportTestData.Create());

        PlanningShiftDistributionViewModel early = Assert.Single(
            viewModel.SelectedWeekSummary.ShiftDistributions,
            value => value.Code == "F");
        Assert.Equal(2, early.TotalCount);
        Assert.Equal(2, early.ComparablePersonCount);
        Assert.Equal(0, early.MinimumCount);
        Assert.Equal(2, early.MaximumCount);
        Assert.Equal(2, early.Spread);
        Assert.Equal(1, viewModel.SelectedWeekSummary.SplitShiftCount);
        Assert.Equal(1, viewModel.SelectedWeekSummary.ReliefShiftCount);
        Assert.Contains("Spannweite 2", early.Display);

        viewModel.SelectedWeek = viewModel.Weeks[1];

        Assert.All(
            viewModel.Employees.SelectMany(value => value.ServiceCounts),
            value => Assert.Equal(0, value.Count));
        Assert.Equal(0, viewModel.SelectedWeekSummary.SplitShiftCount);
        Assert.Equal(0, viewModel.SelectedWeekSummary.ReliefShiftCount);
    }
}

internal static class PlanningEmployeeReportTestData
{
    private static readonly DateOnly PeriodMonday = new(2026, 9, 21);
    private static readonly Guid EarlyShiftId = Guid.Parse(
        "51000000-0000-4000-8000-000000000001");
    private static readonly Guid LateShiftId = Guid.Parse(
        "51000000-0000-4000-8000-000000000002");

    public static AutomaticSchedulePlanningReport Create()
    {
        PlanningServiceColumnSnapshot[] columns =
        [
            new(PlanningServiceCountKind.NormalShift, EarlyShiftId, "F", "Frühdienst"),
            new(PlanningServiceCountKind.NormalShift, LateShiftId, "S", "Spätdienst"),
            new(PlanningServiceCountKind.SplitShift, null, "D", "Doppeldienst"),
            new(PlanningServiceCountKind.ReliefShift, null, "Spr", "Springer-Einsatz"),
            new(PlanningServiceCountKind.OfficeTime, null, "B", "Bürozeit"),
            new(
                PlanningServiceCountKind.ManualAdditional,
                LateShiftId,
                "S",
                "Spätdienst – manuell zusätzlich"),
        ];
        PlanningEmployeeWeekSnapshot[] firstWeek =
        [
            Week(
                1,
                "Silke",
                "Leitung",
                "Typ1",
                EmployeeTypePlanningRoleKind.ServiceManagement,
                2_400,
                2_400,
                60,
                0,
                [0, 0, 0, 0, 1, 0],
                []),
            Week(
                2,
                "Nora",
                "Normal",
                "Typ40",
                EmployeeTypePlanningRoleKind.Normal,
                2_400,
                1_920,
                1_230,
                1,
                [2, 0, 1, 0, 0, 1],
                [EarlyShiftId, LateShiftId]),
            Week(
                3,
                "Frieda",
                "Vollabwesend",
                "Typ40",
                EmployeeTypePlanningRoleKind.Normal,
                2_400,
                0,
                0,
                5,
                [0, 0, 0, 0, 0, 0],
                []),
            Week(
                4,
                "Anton",
                "Aushilfe",
                "AH",
                EmployeeTypePlanningRoleKind.Auxiliary,
                600,
                600,
                400,
                0,
                [0, 1, 0, 1, 0, 0],
                [EarlyShiftId, LateShiftId]),
        ];
        PlanningEmployeeWeekSnapshot[] laterWeeks = Enumerable.Range(1, 2)
            .SelectMany(weekIndex => firstWeek.Select(value => new PlanningEmployeeWeekSnapshot(
                value.EmployeeId,
                value.FirstName,
                value.LastName,
                value.EmployeeTypeCode,
                value.EmployeeTypeName,
                value.PlanningRole,
                PeriodMonday.AddDays(weekIndex * 7),
                value.WeeklyTargetMinutes,
                value.WeeklyTargetMinutes,
                0,
                0,
                0,
                Counts([0, 0, 0, 0, 0, 0]),
                value.PlanningRole == EmployeeTypePlanningRoleKind.ServiceManagement
                    ? []
                    : [EarlyShiftId, LateShiftId])))
            .ToArray();
        return new AutomaticSchedulePlanningReport(
            Guid.Parse("11000000-0000-4000-8000-000000000001"),
            Guid.Parse("11000000-0000-4000-8000-000000000002"),
            3,
            PeriodMonday,
            PeriodMonday.AddDays(20),
            [],
            columns,
            [.. firstWeek, .. laterWeeks]);
    }

    private static PlanningEmployeeWeekSnapshot Week(
        int id,
        string firstName,
        string lastName,
        string typeCode,
        EmployeeTypePlanningRoleKind role,
        int weeklyTarget,
        int effectiveTarget,
        int plannedWork,
        int vacationDays,
        int[] counts,
        Guid[] comparableShiftTypeIds) => new(
            Guid.Parse($"21000000-0000-4000-8000-{id:D12}"),
            firstName,
            lastName,
            typeCode,
            typeCode,
            role,
            PeriodMonday,
            weeklyTarget,
            effectiveTarget,
            plannedWork,
            vacationDays,
            0,
            Counts(counts),
            comparableShiftTypeIds);

    private static PlanningServiceCountSnapshot[] Counts(int[] counts) =>
    [
        new(PlanningServiceCountKind.NormalShift, EarlyShiftId, "F", counts[0]),
        new(PlanningServiceCountKind.NormalShift, LateShiftId, "S", counts[1]),
        new(PlanningServiceCountKind.SplitShift, null, "D", counts[2]),
        new(PlanningServiceCountKind.ReliefShift, null, "Spr", counts[3]),
        new(PlanningServiceCountKind.OfficeTime, null, "B", counts[4]),
        new(PlanningServiceCountKind.ManualAdditional, LateShiftId, "S", counts[5]),
    ];
}
