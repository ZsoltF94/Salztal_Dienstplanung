using System.Collections.ObjectModel;
using System.Globalization;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class PlanningEmployeeReportWeekViewModel
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    public PlanningEmployeeReportWeekViewModel(
        int number,
        PlanningEmployeeWeekSummarySnapshot summary,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentNullException.ThrowIfNull(summary);
        Number = number;
        Display = string.Create(
            GermanCulture,
            $"Woche {number}: {summary.WeekMonday:dd.MM.}–{summary.WeekSunday:dd.MM.yyyy}");
        Summary = new PlanningEmployeeWeekSummaryViewModel(summary, serviceColumns);
    }

    public int Number { get; }

    public string Display { get; }

    public PlanningEmployeeWeekSummaryViewModel Summary { get; }
}

internal sealed class PlanningEmployeeServiceColumnViewModel
{
    public PlanningEmployeeServiceColumnViewModel(PlanningServiceColumnSnapshot column)
    {
        ArgumentNullException.ThrowIfNull(column);
        Kind = column.Kind;
        ShiftTypeId = column.ShiftTypeId;
        Code = column.Code;
        Name = column.Name;
        Display = column.Kind == PlanningServiceCountKind.ManualAdditional
            ? $"{column.Code} (manuell zusätzlich)"
            : column.Code;
    }

    public PlanningServiceCountKind Kind { get; }

    public Guid? ShiftTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public string Display { get; }
}

internal sealed class PlanningEmployeeServiceCountViewModel
{
    public PlanningEmployeeServiceCountViewModel(
        PlanningServiceCountSnapshot count,
        bool isIncludedInComparison)
    {
        ArgumentNullException.ThrowIfNull(count);
        Kind = count.Kind;
        ShiftTypeId = count.ShiftTypeId;
        Code = count.Code;
        Count = count.Count;
        IsIncludedInComparison = isIncludedInComparison;
    }

    public PlanningServiceCountKind Kind { get; }

    public Guid? ShiftTypeId { get; }

    public string Code { get; }

    public int Count { get; }

    public bool IsIncludedInComparison { get; }
}

internal sealed class PlanningEmployeeReportRowViewModel
{
    public PlanningEmployeeReportRowViewModel(
        PlanningEmployeeWeekSnapshot week,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns)
    {
        ArgumentNullException.ThrowIfNull(week);
        ArgumentNullException.ThrowIfNull(serviceColumns);
        EmployeeId = week.EmployeeId;
        PlanningRole = week.PlanningRole;
        NameDisplay = $"{week.LastName}, {week.FirstName}";
        EmployeeTypeDisplay = $"{week.EmployeeTypeCode} – {week.EmployeeTypeName}";
        RoleDisplay = week.PlanningRole switch
        {
            EmployeeTypePlanningRoleKind.ServiceManagement => "Typ1",
            EmployeeTypePlanningRoleKind.Normal => "Regulär",
            EmployeeTypePlanningRoleKind.Auxiliary => "AH",
            _ => throw new ArgumentOutOfRangeException(nameof(week)),
        };
        WeeklyTargetDisplay = PlanningEmployeeReportPresentation.FormatMinutes(
            week.WeeklyTargetMinutes);
        EffectiveTargetDisplay = PlanningEmployeeReportPresentation.FormatMinutes(
            week.EffectiveTargetMinutes);
        PlannedWorkDisplay = PlanningEmployeeReportPresentation.FormatMinutes(
            week.PlannedWorkMinutes);
        DifferenceDisplay = PlanningEmployeeReportPresentation.FormatSignedMinutes(
            week.DifferenceMinutes);
        VacationDayCount = week.VacationDayCount;
        SicknessDayCount = week.SicknessDayCount;
        AbsenceDisplay = $"U: {VacationDayCount} · K: {SicknessDayCount}";
        ServiceCounts = Array.AsReadOnly(week.ServiceCounts
            .Select(count => new PlanningEmployeeServiceCountViewModel(
                count,
                count.Kind == PlanningServiceCountKind.NormalShift
                    && count.ShiftTypeId.HasValue
                    && week.ComparableShiftTypeIds.Contains(count.ShiftTypeId.Value)))
            .ToArray());
    }

    public Guid EmployeeId { get; }

    public EmployeeTypePlanningRoleKind PlanningRole { get; }

    public string NameDisplay { get; }

    public string EmployeeTypeDisplay { get; }

    public string RoleDisplay { get; }

    public string WeeklyTargetDisplay { get; }

    public string EffectiveTargetDisplay { get; }

    public string PlannedWorkDisplay { get; }

    public string DifferenceDisplay { get; }

    public int VacationDayCount { get; }

    public int SicknessDayCount { get; }

    public string AbsenceDisplay { get; }

    public ReadOnlyCollection<PlanningEmployeeServiceCountViewModel> ServiceCounts
    {
        get;
    }
}

internal sealed class PlanningEmployeeWeekSummaryViewModel
{
    public PlanningEmployeeWeekSummaryViewModel(
        PlanningEmployeeWeekSummarySnapshot summary,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(serviceColumns);
        WeekMonday = summary.WeekMonday;
        WeekSunday = summary.WeekSunday;
        SplitShiftCount = summary.SplitShiftCount;
        ReliefShiftCount = summary.ReliefShiftCount;
        ServiceTotals = Array.AsReadOnly(summary.ServiceTotals
            .Zip(serviceColumns, (count, column) =>
                new PlanningEmployeeServiceTotalViewModel(count, column))
            .ToArray());
        ShiftDistributions = Array.AsReadOnly(summary.ShiftDistributions
            .Select(value => new PlanningShiftDistributionViewModel(value))
            .ToArray());
    }

    public DateOnly WeekMonday { get; }

    public DateOnly WeekSunday { get; }

    public int SplitShiftCount { get; }

    public int ReliefShiftCount { get; }

    public ReadOnlyCollection<PlanningEmployeeServiceTotalViewModel> ServiceTotals
    {
        get;
    }

    public ReadOnlyCollection<PlanningShiftDistributionViewModel> ShiftDistributions
    {
        get;
    }
}

internal sealed class PlanningEmployeeServiceTotalViewModel
{
    public PlanningEmployeeServiceTotalViewModel(
        PlanningServiceCountSnapshot count,
        PlanningServiceColumnSnapshot column)
    {
        ArgumentNullException.ThrowIfNull(count);
        ArgumentNullException.ThrowIfNull(column);
        Display = column.Kind == PlanningServiceCountKind.ManualAdditional
            ? $"{column.Code} (manuell zusätzlich): {count.Count}"
            : $"{column.Code}: {count.Count}";
        Count = count.Count;
    }

    public string Display { get; }

    public int Count { get; }
}

internal sealed class PlanningShiftDistributionViewModel
{
    public PlanningShiftDistributionViewModel(PlanningShiftDistributionSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Code = value.Code;
        TotalCount = value.TotalCount;
        ComparablePersonCount = value.ComparablePersonCount;
        MinimumCount = value.MinimumCount;
        MaximumCount = value.MaximumCount;
        Spread = value.Spread;
        Display = value.ComparablePersonCount == 0
            ? $"{value.Code}: gesamt {value.TotalCount}; keine vergleichbare Person"
            : $"{value.Code}: gesamt {value.TotalCount}; Vergleich {value.ComparablePersonCount}; Minimum {value.MinimumCount}; Maximum {value.MaximumCount}; Spannweite {value.Spread}";
    }

    public string Code { get; }

    public int TotalCount { get; }

    public int ComparablePersonCount { get; }

    public int? MinimumCount { get; }

    public int? MaximumCount { get; }

    public int? Spread { get; }

    public string Display { get; }
}

internal static class PlanningEmployeeReportPresentation
{
    public static string FormatMinutes(int minutes)
    {
        int hours = Math.DivRem(minutes, 60, out int remainingMinutes);
        return remainingMinutes == 0
            ? $"{hours} Std."
            : $"{hours} Std. {remainingMinutes:00} min";
    }

    public static string FormatSignedMinutes(int minutes)
    {
        string sign = minutes switch
        {
            > 0 => "+",
            < 0 => "−",
            _ => "±",
        };
        return $"{sign}{FormatMinutes(Math.Abs(minutes))}";
    }
}
