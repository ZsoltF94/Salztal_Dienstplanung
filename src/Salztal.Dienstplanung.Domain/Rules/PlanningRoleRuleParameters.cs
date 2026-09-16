using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record PlanningRoleRuleParameters : RuleParameters
{
    internal PlanningRoleRuleParameters(EmployeeTypePlanningRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        Role = role;
    }

    public EmployeeTypePlanningRole Role { get; }
}
