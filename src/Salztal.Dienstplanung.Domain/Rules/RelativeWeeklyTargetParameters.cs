namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RelativeWeeklyTargetParameters : RuleParameters
{
    internal RelativeWeeklyTargetParameters(int auxiliaryTargetMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(auxiliaryTargetMinutes);
        AuxiliaryTargetMinutes = auxiliaryTargetMinutes;
    }

    public int AuxiliaryTargetMinutes { get; }

    public bool UsesEffectiveNormalWeeklyTarget { get; } = true;

    public bool ExcludesZeroMinuteTargets { get; } = true;
}
