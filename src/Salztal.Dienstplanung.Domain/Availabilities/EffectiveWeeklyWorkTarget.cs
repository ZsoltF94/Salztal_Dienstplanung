namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed record EffectiveWeeklyWorkTarget
{
    private EffectiveWeeklyWorkTarget(int minutes)
    {
        Minutes = minutes;
    }

    public int Minutes { get; }

    internal static EffectiveWeeklyWorkTarget Create(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        return new EffectiveWeeklyWorkTarget(minutes);
    }

    public override string ToString()
    {
        return Minutes.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
