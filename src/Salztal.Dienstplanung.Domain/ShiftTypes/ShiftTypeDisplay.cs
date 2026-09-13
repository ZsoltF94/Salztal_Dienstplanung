namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed record ShiftTypeDisplay
{
    internal ShiftTypeDisplay(ShiftTypeDisplayKind kind, string? abbreviation)
    {
        Kind = kind;
        Abbreviation = abbreviation;
    }

    public ShiftTypeDisplayKind Kind { get; }

    public string? Abbreviation { get; }
}
