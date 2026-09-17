using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

public sealed record ScheduleEvaluationEligibility(
    ShiftEligibilityTargetKind TargetKind,
    Guid TargetId,
    ShiftEligibilityMode Mode,
    ShiftEligibilityActivation Activation);

public sealed record ScheduleEvaluationWeekTarget
{
    public ScheduleEvaluationWeekTarget(
        DateOnly weekMonday,
        int effectiveTargetMinutes)
    {
        if (weekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "An evaluation week must start on Monday.",
                nameof(weekMonday));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(effectiveTargetMinutes);
        WeekMonday = weekMonday;
        EffectiveTargetMinutes = effectiveTargetMinutes;
    }

    public DateOnly WeekMonday { get; }

    public int EffectiveTargetMinutes { get; }
}

public sealed class ScheduleEvaluationEmployee
{
    public ScheduleEvaluationEmployee(
        EmployeeId employeeId,
        EmployeeTypePlanningRole planningRole,
        bool allowsAutomaticAssignment,
        IEnumerable<ScheduleEvaluationEligibility> eligibilities,
        IEnumerable<ScheduleEvaluationWeekTarget> weekTargets)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(eligibilities);
        ArgumentNullException.ThrowIfNull(weekTargets);
        if (!Enum.IsDefined(planningRole))
        {
            throw new ArgumentOutOfRangeException(nameof(planningRole));
        }

        ScheduleEvaluationEligibility[] eligibilityValues = eligibilities.ToArray();
        ScheduleEvaluationWeekTarget[] weekTargetValues = weekTargets.ToArray();
        if (eligibilityValues.Any(value => value is null)
            || weekTargetValues.Any(value => value is null))
        {
            throw new ArgumentException(
                "Evaluation employee collections cannot contain null values.");
        }

        if (weekTargetValues
            .Select(value => value.WeekMonday)
            .Distinct()
            .Count() != weekTargetValues.Length)
        {
            throw new ArgumentException(
                "Evaluation week targets must have unique Mondays.",
                nameof(weekTargets));
        }

        EmployeeId = employeeId;
        PlanningRole = planningRole;
        AllowsAutomaticAssignment = allowsAutomaticAssignment;
        Eligibilities = Array.AsReadOnly(eligibilityValues);
        WeekTargets = Array.AsReadOnly(
            weekTargetValues.OrderBy(value => value.WeekMonday).ToArray());
    }

    public EmployeeId EmployeeId { get; }

    public EmployeeTypePlanningRole PlanningRole { get; }

    public bool AllowsAutomaticAssignment { get; }

    public ReadOnlyCollection<ScheduleEvaluationEligibility> Eligibilities { get; }

    public ReadOnlyCollection<ScheduleEvaluationWeekTarget> WeekTargets { get; }
}
