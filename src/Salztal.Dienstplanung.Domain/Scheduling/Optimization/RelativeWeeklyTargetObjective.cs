using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public sealed record RelativeWeeklyTargetCase
{
    public RelativeWeeklyTargetCase(
        Guid employeeId,
        DateOnly weekMonday,
        int assignedMinutes,
        int targetMinutes)
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
        ArgumentOutOfRangeException.ThrowIfNegative(targetMinutes);
        EmployeeId = employeeId;
        WeekMonday = weekMonday;
        AssignedMinutes = assignedMinutes;
        TargetMinutes = targetMinutes;
    }

    public Guid EmployeeId { get; }

    public DateOnly WeekMonday { get; }

    public int AssignedMinutes { get; }

    public int TargetMinutes { get; }

    public long AbsoluteDeviationMinutes => Math.Abs(
        (long)AssignedMinutes - TargetMinutes);
}

public sealed class RelativeWeeklyTargetObjective
{
    public RelativeWeeklyTargetObjective(
        IEnumerable<RelativeWeeklyTargetCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        RelativeWeeklyTargetCase[] values = cases.ToArray();
        if (values.Any(value => value is null)
            || values.GroupBy(value => (value.EmployeeId, value.WeekMonday))
                .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Relative weekly target cases must be non-null and unique per employee-week.",
                nameof(cases));
        }

        Cases = Array.AsReadOnly(values
            .OrderBy(value => value.WeekMonday)
            .ThenBy(value => value.EmployeeId)
            .ToArray());
        ComparableCases = Array.AsReadOnly(Cases
            .Where(value => value.TargetMinutes > 0)
            .ToArray());
        ExcludedZeroTargetCaseCount = Cases.Count - ComparableCases.Count;
    }

    public static RelativeWeeklyTargetObjective Empty { get; } = new([]);

    public ReadOnlyCollection<RelativeWeeklyTargetCase> Cases { get; }

    public ReadOnlyCollection<RelativeWeeklyTargetCase> ComparableCases { get; }

    public int ExcludedZeroTargetCaseCount { get; }

    internal int CompareTo(RelativeWeeklyTargetObjective other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ValidateComparablePopulation(other);
        RelativeWeeklyTargetCase[] left = ComparableCases
            .Order(DescendingDeviationComparer.Instance)
            .ToArray();
        RelativeWeeklyTargetCase[] right = other.ComparableCases
            .Order(DescendingDeviationComparer.Instance)
            .ToArray();

        for (int index = 0; index < left.Length; index++)
        {
            int comparison = CompareFractions(left[index], right[index]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    private void ValidateComparablePopulation(RelativeWeeklyTargetObjective other)
    {
        if (Cases.Count != other.Cases.Count
            || !Cases.Zip(other.Cases).All(pair =>
                pair.First.EmployeeId == pair.Second.EmployeeId
                && pair.First.WeekMonday == pair.Second.WeekMonday
                && pair.First.TargetMinutes == pair.Second.TargetMinutes))
        {
            throw new ArgumentException(
                "Relative weekly target objectives require the same employee-week targets.",
                nameof(other));
        }
    }

    private static int CompareFractions(
        RelativeWeeklyTargetCase left,
        RelativeWeeklyTargetCase right)
    {
        Int128 leftScaled = (Int128)left.AbsoluteDeviationMinutes
            * right.TargetMinutes;
        Int128 rightScaled = (Int128)right.AbsoluteDeviationMinutes
            * left.TargetMinutes;
        return leftScaled.CompareTo(rightScaled);
    }

    private sealed class DescendingDeviationComparer :
        IComparer<RelativeWeeklyTargetCase>
    {
        internal static DescendingDeviationComparer Instance { get; } = new();

        public int Compare(
            RelativeWeeklyTargetCase? left,
            RelativeWeeklyTargetCase? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return 1;
            }

            if (right is null)
            {
                return -1;
            }

            int fractionComparison = CompareFractions(right, left);
            if (fractionComparison != 0)
            {
                return fractionComparison;
            }

            int weekComparison = left.WeekMonday.CompareTo(right.WeekMonday);
            return weekComparison != 0
                ? weekComparison
                : left.EmployeeId.CompareTo(right.EmployeeId);
        }
    }
}
