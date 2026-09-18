using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class PlanningDemandReportViewModelTests
{
    [Fact]
    public void DefaultViewShowsEveryDemandWithPreparedSummaryAndIntervals()
    {
        PlanningDemandReportViewModel viewModel = new(
            PlanningDemandReportTestData.Create());

        Assert.Equal(3, viewModel.Weeks.Count);
        Assert.Equal(1, viewModel.SelectedWeek.Number);
        Assert.Equal(3, viewModel.Demands.Count);
        Assert.Contains(
            viewModel.Demands,
            value => value.StatusDisplay == "Vollständig gedeckt");
        PlanningDemandRowViewModel partial = Assert.Single(
            viewModel.Demands,
            value => value.StatusDisplay == "Teilweise durch Spr gedeckt");
        Assert.Equal("16:30–17:30", partial.OpenIntervalsDisplay);
        Assert.Equal("17:30–19:30", partial.CoveredIntervalsDisplay);
        Assert.Equal(3, viewModel.SelectedWeekSummary.RequiredPersonCount);
        Assert.Equal(450, viewModel.SelectedWeekSummary.RequiredMinutes);
        Assert.Equal(210, viewModel.SelectedWeekSummary.CoveredMinutes);
        Assert.Equal(240, viewModel.SelectedWeekSummary.OpenMinutes);
        Assert.Equal(46.7m, viewModel.SelectedWeekSummary.CoveragePercentage);
        Assert.Contains("vollständig: 1", viewModel.SelectedWeekSummary.PersonCountDisplay);
        Assert.Contains("teilweise: 1", viewModel.SelectedWeekSummary.PersonCountDisplay);
        Assert.Contains("ungedeckt: 1", viewModel.SelectedWeekSummary.PersonCountDisplay);
    }

    [Fact]
    public void OpenFilterIsOptionalAndWeekSelectionKeepsWeeksSeparate()
    {
        PlanningDemandReportViewModel viewModel = new(
            PlanningDemandReportTestData.Create());

        viewModel.ShowOnlyOpenDemands = true;

        Assert.Equal(2, viewModel.Demands.Count);
        Assert.DoesNotContain(
            viewModel.Demands,
            value => value.StatusDisplay == "Vollständig gedeckt");

        viewModel.ShowOnlyOpenDemands = false;
        viewModel.SelectedWeek = viewModel.Weeks[1];

        PlanningDemandRowViewModel secondWeek = Assert.Single(viewModel.Demands);
        Assert.Equal(new DateOnly(2026, 9, 28), secondWeek.Date);
        Assert.Equal("Cafeteria", secondWeek.WorkLocationName);
        Assert.Equal(120, viewModel.SelectedWeekSummary.RequiredMinutes);
        Assert.Equal(100m, viewModel.SelectedWeekSummary.CoveragePercentage);
        Assert.Equal(570, viewModel.TotalSummary.RequiredMinutes);

        viewModel.SelectedWeek = viewModel.Weeks[2];

        Assert.Empty(viewModel.Demands);
        Assert.Equal(
            "Deckungsquote: kein Bedarf",
            viewModel.SelectedWeekSummary.CoverageDisplay);
    }
}

internal static class PlanningDemandReportTestData
{
    private static readonly DateOnly PeriodMonday = new(2026, 9, 21);
    private static readonly Guid SnapshotId = Guid.Parse(
        "10000000-0000-4000-8000-000000000001");
    private static readonly Guid DraftId = Guid.Parse(
        "10000000-0000-4000-8000-000000000002");
    private static readonly Guid RestaurantId = Guid.Parse(
        "20000000-0000-4000-8000-000000000001");
    private static readonly Guid CafeteriaId = Guid.Parse(
        "20000000-0000-4000-8000-000000000002");
    private static readonly Guid EarlyShiftId = Guid.Parse(
        "30000000-0000-4000-8000-000000000001");
    private static readonly Guid LateShiftId = Guid.Parse(
        "30000000-0000-4000-8000-000000000002");
    private static readonly Guid CafeShiftId = Guid.Parse(
        "30000000-0000-4000-8000-000000000003");

    public static AutomaticSchedulePlanningReport Create()
    {
        PlanningDemandCoverageSnapshot[] demands =
        [
            Demand(
                1,
                PeriodMonday,
                RestaurantId,
                "Restaurant",
                EarlyShiftId,
                "Frühdienst",
                new TimeOnly(6, 30),
                new TimeOnly(8, 0),
                [(new TimeOnly(6, 30), new TimeOnly(8, 0))],
                [],
                PlanningDemandCoverageStatus.FullyCovered),
            Demand(
                2,
                PeriodMonday.AddDays(1),
                RestaurantId,
                "Restaurant",
                LateShiftId,
                "Spätdienst",
                new TimeOnly(16, 30),
                new TimeOnly(19, 30),
                [(new TimeOnly(17, 30), new TimeOnly(19, 30))],
                [(new TimeOnly(16, 30), new TimeOnly(17, 30))],
                PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift),
            Demand(
                3,
                PeriodMonday.AddDays(1),
                CafeteriaId,
                "Cafeteria",
                CafeShiftId,
                "Cafeteria B",
                new TimeOnly(14, 0),
                new TimeOnly(17, 0),
                [],
                [(new TimeOnly(14, 0), new TimeOnly(17, 0))],
                PlanningDemandCoverageStatus.Uncovered),
            Demand(
                4,
                PeriodMonday.AddDays(7),
                CafeteriaId,
                "Cafeteria",
                CafeShiftId,
                "Cafeteria B",
                new TimeOnly(14, 0),
                new TimeOnly(16, 0),
                [(new TimeOnly(14, 0), new TimeOnly(16, 0))],
                [],
                PlanningDemandCoverageStatus.FullyCovered),
        ];
        return new AutomaticSchedulePlanningReport(
            SnapshotId,
            DraftId,
            3,
            PeriodMonday,
            PeriodMonday.AddDays(20),
            demands,
            [],
            []);
    }

    private static PlanningDemandCoverageSnapshot Demand(
        int source,
        DateOnly date,
        Guid workLocationId,
        string workLocationName,
        Guid shiftTypeId,
        string shiftTypeName,
        TimeOnly start,
        TimeOnly end,
        IEnumerable<(TimeOnly Start, TimeOnly End)> covered,
        IEnumerable<(TimeOnly Start, TimeOnly End)> open,
        PlanningDemandCoverageStatus status) => new(
            Guid.Parse($"40000000-0000-4000-8000-{source:D12}"),
            date,
            workLocationId,
            workLocationName,
            shiftTypeId,
            shiftTypeName,
            1,
            start,
            end,
            covered.Select(value => new PlanningDemandIntervalSnapshot(
                value.Start,
                value.End)),
            open.Select(value => new PlanningDemandIntervalSnapshot(
                value.Start,
                value.End)),
            status);
}
