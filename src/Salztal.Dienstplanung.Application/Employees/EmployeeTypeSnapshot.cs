using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public enum EmployeeTypePlanningRoleKind
{
    Normal,
    ServiceManagement,
    Auxiliary,
}

public sealed class EmployeeTypeSnapshot
{
    internal EmployeeTypeSnapshot(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes,
        string weeklyWorkTargetDisplay,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        EmployeeTypePlanningRoleKind planningRole,
        IEnumerable<EmployeeTypeEligibilitySnapshot> shiftEligibilities)
    {
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTargetMinutes = weeklyWorkTargetMinutes;
        WeeklyWorkTargetDisplay = weeklyWorkTargetDisplay;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        AbsenceDayValueMinutes = absenceDayValueMinutes;
        PlanningRole = planningRole;
        ShiftEligibilities = Array.AsReadOnly(shiftEligibilities.ToArray());
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public int WeeklyWorkTargetMinutes { get; }

    public string WeeklyWorkTargetDisplay { get; }

    public bool AllowsVacationAndSickness { get; }

    public int? AbsenceDayValueMinutes { get; }

    public EmployeeTypePlanningRoleKind PlanningRole { get; }

    public ReadOnlyCollection<EmployeeTypeEligibilitySnapshot> ShiftEligibilities { get; }
}
