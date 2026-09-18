using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Employees;

internal sealed class EmployeeSnapshotProjector
{
    private readonly Dictionary<EmployeeTypeId, EmployeeType> _employeeTypes;
    private readonly Dictionary<ShiftTypeId, string> _shiftTypeNames;
    private readonly Dictionary<ShiftPatternId, string> _shiftPatternNames;

    public EmployeeSnapshotProjector(EmployeeReadData data)
    {
        _employeeTypes = data.EmployeeTypes.ToDictionary(employeeType => employeeType.Id);
        _shiftTypeNames = data.ServiceCatalog.ShiftTypes.ToDictionary(
            shiftType => shiftType.Id,
            shiftType => shiftType.Name.Value);
        _shiftPatternNames = new Dictionary<ShiftPatternId, string>
        {
            [data.ServiceCatalog.SplitShiftPattern.Id] =
                data.ServiceCatalog.SplitShiftPattern.DisplayCode,
            [data.ServiceCatalog.ReliefShiftPattern.Id] =
                data.ServiceCatalog.ReliefShiftPattern.DisplayCode,
        };
    }

    public EmployeeOverviewItemSnapshot CreateOverviewItem(Employee employee)
    {
        EmployeeType employeeType = ResolveEmployeeType(employee.EmployeeTypeId);

        return new EmployeeOverviewItemSnapshot(
            employee.Id.Value,
            employee.FirstName.Value,
            employee.LastName.Value,
            employee.DisplayName,
            employee.IsActive,
            employeeType.Id.Value,
            employeeType.Code.Value,
            employeeType.Name.Value,
            employeeType.WeeklyWorkTarget.Minutes,
            CreateWeeklyWorkTargetDisplay(employeeType.WeeklyWorkTarget.Minutes));
    }

    public EmployeeDetailsSnapshot CreateDetails(Employee employee)
    {
        return new EmployeeDetailsSnapshot(
            employee.Id.Value,
            employee.FirstName.Value,
            employee.LastName.Value,
            employee.DisplayName,
            employee.IsActive,
            CreateEmployeeType(ResolveEmployeeType(employee.EmployeeTypeId)));
    }

    public EmployeeTypeSnapshot CreateEmployeeType(EmployeeType employeeType)
    {
        return new EmployeeTypeSnapshot(
            employeeType.Id.Value,
            employeeType.Code.Value,
            employeeType.Name.Value,
            employeeType.WeeklyWorkTarget.Minutes,
            CreateWeeklyWorkTargetDisplay(employeeType.WeeklyWorkTarget.Minutes),
            employeeType.AbsencePolicy.AllowsVacationAndSickness,
            employeeType.AbsencePolicy.DayValue?.Minutes,
            EmployeeTypePlanningRoleMapper.ToSnapshotKind(employeeType.PlanningPolicy.Role),
            employeeType.ShiftEligibilities.Select(CreateShiftEligibility));
    }

    private EmployeeType ResolveEmployeeType(EmployeeTypeId employeeTypeId)
    {
        return _employeeTypes.TryGetValue(employeeTypeId, out EmployeeType? employeeType)
            ? employeeType
            : throw new InvalidOperationException(
                $"Employee type '{employeeTypeId}' is missing from the read data.");
    }

    private EmployeeTypeEligibilitySnapshot CreateShiftEligibility(
        EmployeeTypeShiftEligibility eligibility)
    {
        bool isShiftPattern = eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern;
        Guid targetId;
        string targetName;

        if (isShiftPattern)
        {
            ShiftPatternId shiftPatternId = eligibility.ShiftPatternId
                ?? throw new InvalidOperationException(
                    "A shift-pattern eligibility has no shift-pattern identifier.");
            targetId = shiftPatternId.Value;
            targetName = _shiftPatternNames.TryGetValue(shiftPatternId, out string? name)
                ? name
                : throw new InvalidOperationException(
                    $"Shift pattern '{shiftPatternId}' is missing from the service catalog.");
        }
        else
        {
            ShiftTypeId shiftTypeId = eligibility.ShiftTypeId
                ?? throw new InvalidOperationException(
                    "A shift-type eligibility has no shift-type identifier.");
            targetId = shiftTypeId.Value;
            targetName = _shiftTypeNames.TryGetValue(shiftTypeId, out string? name)
                ? name
                : throw new InvalidOperationException(
                    $"Shift type '{shiftTypeId}' is missing from the service catalog.");
        }

        return new EmployeeTypeEligibilitySnapshot(
            targetId,
            targetName,
            isShiftPattern
                ? EmployeeTypeEligibilityTargetKind.ShiftPattern
                : EmployeeTypeEligibilityTargetKind.ShiftType,
            CreateEligibilityMode(eligibility.Mode),
            CreateEligibilityActivation(eligibility.Activation),
            CreateAvailabilityDisplay(eligibility));
    }

    private static EmployeeTypeEligibilityMode CreateEligibilityMode(
        ShiftEligibilityMode mode)
    {
        return mode switch
        {
            ShiftEligibilityMode.Regular => EmployeeTypeEligibilityMode.Regular,
            ShiftEligibilityMode.ManualSuggestion =>
                EmployeeTypeEligibilityMode.ManualSuggestion,
            _ => throw new InvalidOperationException(
                $"Unsupported shift-eligibility mode: {mode}"),
        };
    }

    private static EmployeeTypeEligibilityActivation CreateEligibilityActivation(
        ShiftEligibilityActivation activation)
    {
        return activation switch
        {
            ShiftEligibilityActivation.Always =>
                EmployeeTypeEligibilityActivation.Always,
            ShiftEligibilityActivation.ExplicitPlanningRunOption =>
                EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption,
            _ => throw new InvalidOperationException(
                $"Unsupported shift-eligibility activation: {activation}"),
        };
    }

    private static string CreateAvailabilityDisplay(
        EmployeeTypeShiftEligibility eligibility)
    {
        return (eligibility.Mode, eligibility.Activation) switch
        {
            (ShiftEligibilityMode.Regular, ShiftEligibilityActivation.Always) =>
                "Regulär zulässig",
            (ShiftEligibilityMode.ManualSuggestion, ShiftEligibilityActivation.Always) =>
                "Nur als manueller Lösungsvorschlag",
            (ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.ExplicitPlanningRunOption) =>
                "Nur bei aktivierter Planungslaufoption",
            (ShiftEligibilityMode.ManualSuggestion,
                ShiftEligibilityActivation.ExplicitPlanningRunOption) =>
                "Nur als manueller Lösungsvorschlag bei aktivierter Planungslaufoption",
            _ => throw new InvalidOperationException("The shift eligibility is unsupported."),
        };
    }

    private static string CreateWeeklyWorkTargetDisplay(int minutes)
    {
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;

        if (remainingMinutes == 0)
        {
            return hours == 1 ? "1 Stunde" : $"{hours} Stunden";
        }

        if (hours == 0)
        {
            return remainingMinutes == 1
                ? "1 Minute"
                : $"{remainingMinutes} Minuten";
        }

        string hourDisplay = hours == 1 ? "1 Stunde" : $"{hours} Stunden";
        string minuteDisplay = remainingMinutes == 1
            ? "1 Minute"
            : $"{remainingMinutes} Minuten";

        return $"{hourDisplay} {minuteDisplay}";
    }
}
