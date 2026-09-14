using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class EmployeeTypeSnapshot
{
    internal EmployeeTypeSnapshot(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes,
        string weeklyWorkTargetDisplay,
        IEnumerable<EmployeeTypeEligibilitySnapshot> shiftEligibilities)
    {
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTargetMinutes = weeklyWorkTargetMinutes;
        WeeklyWorkTargetDisplay = weeklyWorkTargetDisplay;
        ShiftEligibilities = Array.AsReadOnly(shiftEligibilities.ToArray());
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public int WeeklyWorkTargetMinutes { get; }

    public string WeeklyWorkTargetDisplay { get; }

    public ReadOnlyCollection<EmployeeTypeEligibilitySnapshot> ShiftEligibilities { get; }
}
