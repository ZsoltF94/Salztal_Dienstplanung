namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public interface IShiftPattern
{
    public ShiftPatternId Id { get; }

    public string DisplayCode { get; }
}
