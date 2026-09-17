namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public readonly record struct RuleViolationMagnitude
{
    public RuleViolationMagnitude(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }
}
