using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Employees;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum PlanningServiceCountKind
{
    NormalShift,
    SplitShift,
    ReliefShift,
    OfficeTime,
    ManualAdditional,
}

public sealed class PlanningServiceColumnSnapshot
{
    internal PlanningServiceColumnSnapshot(
        PlanningServiceCountKind kind,
        Guid? shiftTypeId,
        string code,
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if ((kind is PlanningServiceCountKind.NormalShift
                or PlanningServiceCountKind.ManualAdditional) != shiftTypeId.HasValue)
        {
            throw new ArgumentException(
                "Only shift-based service columns require a shift type identifier.",
                nameof(shiftTypeId));
        }

        Kind = kind;
        ShiftTypeId = shiftTypeId;
        Code = code.Trim();
        Name = name.Trim();
    }

    public PlanningServiceCountKind Kind { get; }

    public Guid? ShiftTypeId { get; }

    public string Code { get; }

    public string Name { get; }
}

public sealed record PlanningServiceCountSnapshot(
    PlanningServiceCountKind Kind,
    Guid? ShiftTypeId,
    string Code,
    int Count);

public sealed class PlanningEmployeeWeekSnapshot
{
    internal PlanningEmployeeWeekSnapshot(
        Guid employeeId,
        string firstName,
        string lastName,
        string employeeTypeCode,
        string employeeTypeName,
        EmployeeTypePlanningRoleKind planningRole,
        DateOnly weekMonday,
        int weeklyTargetMinutes,
        int effectiveTargetMinutes,
        int plannedWorkMinutes,
        int vacationDayCount,
        int sicknessDayCount,
        IEnumerable<PlanningServiceCountSnapshot> serviceCounts,
        IEnumerable<Guid> comparableShiftTypeIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeTypeCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeTypeName);
        ArgumentOutOfRangeException.ThrowIfNegative(weeklyTargetMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(effectiveTargetMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(plannedWorkMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(vacationDayCount);
        ArgumentOutOfRangeException.ThrowIfNegative(sicknessDayCount);
        ArgumentNullException.ThrowIfNull(serviceCounts);
        ArgumentNullException.ThrowIfNull(comparableShiftTypeIds);

        PlanningServiceCountSnapshot[] counts = serviceCounts.ToArray();
        Guid[] comparableIds = comparableShiftTypeIds.Distinct().Order().ToArray();
        if (counts.Any(value => value is null || value.Count < 0)
            || counts.GroupBy(value => (value.Kind, value.ShiftTypeId, value.Code))
                .Any(group => group.Count() > 1)
            || comparableIds.Any(value => value == Guid.Empty))
        {
            throw new ArgumentException(
                "Service counts and comparable shifts must be valid and unique.");
        }

        EmployeeId = employeeId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        EmployeeTypeCode = employeeTypeCode.Trim();
        EmployeeTypeName = employeeTypeName.Trim();
        PlanningRole = planningRole;
        WeekMonday = weekMonday;
        WeeklyTargetMinutes = weeklyTargetMinutes;
        EffectiveTargetMinutes = effectiveTargetMinutes;
        PlannedWorkMinutes = plannedWorkMinutes;
        VacationDayCount = vacationDayCount;
        SicknessDayCount = sicknessDayCount;
        ServiceCounts = Array.AsReadOnly(counts);
        ComparableShiftTypeIds = Array.AsReadOnly(comparableIds);
    }

    public Guid EmployeeId { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public string EmployeeTypeCode { get; }

    public string EmployeeTypeName { get; }

    public EmployeeTypePlanningRoleKind PlanningRole { get; }

    public DateOnly WeekMonday { get; }

    public int WeeklyTargetMinutes { get; }

    public int EffectiveTargetMinutes { get; }

    public int PlannedWorkMinutes { get; }

    public int DifferenceMinutes => PlannedWorkMinutes - EffectiveTargetMinutes;

    public int VacationDayCount { get; }

    public int SicknessDayCount { get; }

    public ReadOnlyCollection<PlanningServiceCountSnapshot> ServiceCounts { get; }

    public ReadOnlyCollection<Guid> ComparableShiftTypeIds { get; }
}

public sealed class PlanningShiftDistributionSnapshot
{
    internal PlanningShiftDistributionSnapshot(
        Guid shiftTypeId,
        string code,
        int totalCount,
        IEnumerable<int> comparableCounts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        ArgumentNullException.ThrowIfNull(comparableCounts);
        int[] counts = comparableCounts.ToArray();
        if (counts.Any(value => value < 0))
        {
            throw new ArgumentException(
                "Comparable service counts cannot be negative.",
                nameof(comparableCounts));
        }

        ShiftTypeId = shiftTypeId;
        Code = code.Trim();
        TotalCount = totalCount;
        ComparablePersonCount = counts.Length;
        MinimumCount = counts.Length == 0 ? null : counts.Min();
        MaximumCount = counts.Length == 0 ? null : counts.Max();
    }

    public Guid ShiftTypeId { get; }

    public string Code { get; }

    public int TotalCount { get; }

    public int ComparablePersonCount { get; }

    public int? MinimumCount { get; }

    public int? MaximumCount { get; }

    public int? Spread => MinimumCount.HasValue && MaximumCount.HasValue
        ? MaximumCount.Value - MinimumCount.Value
        : null;
}

public sealed class PlanningEmployeeWeekSummarySnapshot
{
    internal PlanningEmployeeWeekSummarySnapshot(
        DateOnly weekMonday,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns,
        IEnumerable<PlanningEmployeeWeekSnapshot> employeeWeeks)
    {
        ArgumentNullException.ThrowIfNull(serviceColumns);
        ArgumentNullException.ThrowIfNull(employeeWeeks);
        PlanningServiceColumnSnapshot[] columns = serviceColumns.ToArray();
        PlanningEmployeeWeekSnapshot[] weeks = employeeWeeks.ToArray();
        if (weeks.Any(value => value.WeekMonday != weekMonday))
        {
            throw new ArgumentException(
                "Employee week summaries can only contain one week.",
                nameof(employeeWeeks));
        }

        WeekMonday = weekMonday;
        WeekSunday = weekMonday.AddDays(6);
        ServiceTotals = Array.AsReadOnly(columns
            .Select(column => new PlanningServiceCountSnapshot(
                column.Kind,
                column.ShiftTypeId,
                column.Code,
                weeks.Sum(week => ServiceCount(week, column))))
            .ToArray());
        ShiftDistributions = Array.AsReadOnly(columns
            .Where(column => column.Kind == PlanningServiceCountKind.NormalShift)
            .Select(column => new PlanningShiftDistributionSnapshot(
                column.ShiftTypeId!.Value,
                column.Code,
                ServiceTotals.Single(total => Matches(total, column)).Count,
                weeks
                    .Where(week => week.ComparableShiftTypeIds.Contains(
                        column.ShiftTypeId.Value))
                    .Select(week => ServiceCount(week, column))))
            .ToArray());
    }

    public DateOnly WeekMonday { get; }

    public DateOnly WeekSunday { get; }

    public int SplitShiftCount => Total(PlanningServiceCountKind.SplitShift);

    public int ReliefShiftCount => Total(PlanningServiceCountKind.ReliefShift);

    public ReadOnlyCollection<PlanningServiceCountSnapshot> ServiceTotals { get; }

    public ReadOnlyCollection<PlanningShiftDistributionSnapshot> ShiftDistributions
    {
        get;
    }

    private int Total(PlanningServiceCountKind kind) => ServiceTotals
        .Where(value => value.Kind == kind)
        .Sum(value => value.Count);

    private static int ServiceCount(
        PlanningEmployeeWeekSnapshot week,
        PlanningServiceColumnSnapshot column) => week.ServiceCounts
        .Single(value => Matches(value, column))
        .Count;

    private static bool Matches(
        PlanningServiceCountSnapshot count,
        PlanningServiceColumnSnapshot column) =>
        count.Kind == column.Kind
        && count.ShiftTypeId == column.ShiftTypeId
        && count.Code == column.Code;
}
