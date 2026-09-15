using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public enum EmployeeTypeEligibilityTargetKind
{
    ShiftType,
    ShiftPattern,
}

public enum EmployeeTypeEligibilityMode
{
    Regular,
    ManualSuggestion,
}

public enum EmployeeTypeEligibilityActivation
{
    Always,
    ExplicitPlanningRunOption,
}

public enum EmployeeTypeDeletionConfirmation
{
    NotConfirmed,
    Confirmed,
}

public sealed record EmployeeTypeEligibilityRequest(
    Guid TargetId,
    EmployeeTypeEligibilityTargetKind TargetKind,
    EmployeeTypeEligibilityMode Mode,
    EmployeeTypeEligibilityActivation Activation);

public sealed class CreateEmployeeTypeRequest
{
    public CreateEmployeeTypeRequest(
        string? code,
        string? name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        IEnumerable<EmployeeTypeEligibilityRequest?>? shiftEligibilities)
    {
        Code = code;
        Name = name;
        WeeklyWorkTargetMinutes = weeklyWorkTargetMinutes;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        AbsenceDayValueMinutes = absenceDayValueMinutes;
        ShiftEligibilities = shiftEligibilities is null
            ? null
            : Array.AsReadOnly(shiftEligibilities.ToArray());
    }

    public string? Code { get; }

    public string? Name { get; }

    public int WeeklyWorkTargetMinutes { get; }

    public bool AllowsVacationAndSickness { get; }

    public int? AbsenceDayValueMinutes { get; }

    public ReadOnlyCollection<EmployeeTypeEligibilityRequest?>? ShiftEligibilities { get; }
}

public sealed class UpdateEmployeeTypeRequest
{
    public UpdateEmployeeTypeRequest(
        Guid employeeTypeId,
        string? name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        IEnumerable<EmployeeTypeEligibilityRequest?>? shiftEligibilities)
    {
        EmployeeTypeId = employeeTypeId;
        Name = name;
        WeeklyWorkTargetMinutes = weeklyWorkTargetMinutes;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        AbsenceDayValueMinutes = absenceDayValueMinutes;
        ShiftEligibilities = shiftEligibilities is null
            ? null
            : Array.AsReadOnly(shiftEligibilities.ToArray());
    }

    public Guid EmployeeTypeId { get; }

    public string? Name { get; }

    public int WeeklyWorkTargetMinutes { get; }

    public bool AllowsVacationAndSickness { get; }

    public int? AbsenceDayValueMinutes { get; }

    public ReadOnlyCollection<EmployeeTypeEligibilityRequest?>? ShiftEligibilities { get; }
}

public sealed record DeleteEmployeeTypeRequest(
    Guid EmployeeTypeId,
    EmployeeTypeDeletionConfirmation Confirmation);
