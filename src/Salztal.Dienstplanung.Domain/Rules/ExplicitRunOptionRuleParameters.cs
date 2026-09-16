using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record ExplicitRunOptionRuleParameters : RuleParameters
{
    internal ExplicitRunOptionRuleParameters(ShiftEligibilityActivation activation)
    {
        if (!Enum.IsDefined(activation))
        {
            throw new ArgumentOutOfRangeException(nameof(activation));
        }

        Activation = activation;
    }

    public ShiftEligibilityActivation Activation { get; }
}
