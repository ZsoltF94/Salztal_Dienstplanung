using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public sealed record AuxiliaryWeeklyMinimumCase
{
    public AuxiliaryWeeklyMinimumCase(
        Guid employeeId,
        DateOnly weekMonday,
        int assignedMinutes,
        bool hasEligibleDemand)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException(
                "An employee identifier is required.",
                nameof(employeeId));
        }

        if (weekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The week must start on Monday.",
                nameof(weekMonday));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(assignedMinutes);
        EmployeeId = employeeId;
        WeekMonday = weekMonday;
        AssignedMinutes = assignedMinutes;
        HasEligibleDemand = hasEligibleDemand;
    }

    public Guid EmployeeId { get; }

    public DateOnly WeekMonday { get; }

    public int AssignedMinutes { get; }

    public bool HasEligibleDemand { get; }
}

public sealed class AuxiliaryWeeklyMinimumObjective
{
    private static readonly int MinimumMinutes =
        ((WeeklyMinutesMinimumParameters)
            CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum.Parameters)
        .MinimumMinutes;

    public AuxiliaryWeeklyMinimumObjective(
        IEnumerable<AuxiliaryWeeklyMinimumCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        AuxiliaryWeeklyMinimumCase[] values = cases.ToArray();
        if (values.Any(value => value is null)
            || values.GroupBy(value => (value.EmployeeId, value.WeekMonday))
                .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Auxiliary minimum cases must be non-null and unique per employee-week.",
                nameof(cases));
        }

        Cases = Array.AsReadOnly(values
            .OrderBy(value => value.WeekMonday)
            .ThenBy(value => value.EmployeeId)
            .ToArray());
        AuxiliaryWeeklyMinimumCase[] violations = Cases
            .Where(value => value.HasEligibleDemand)
            .Where(value => value.AssignedMinutes < MinimumMinutes)
            .ToArray();
        ViolatedWeekCount = violations.Length;
        MissingMinutes = violations.Aggregate(
            0L,
            (total, value) => checked(total + MinimumMinutes - value.AssignedMinutes));
    }

    public static AuxiliaryWeeklyMinimumObjective Empty { get; } = new([]);

    public ReadOnlyCollection<AuxiliaryWeeklyMinimumCase> Cases { get; }

    public int ViolatedWeekCount { get; }

    public long MissingMinutes { get; }
}
