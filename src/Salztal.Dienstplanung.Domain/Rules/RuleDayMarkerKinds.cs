namespace Salztal.Dienstplanung.Domain.Rules;

[Flags]
public enum RuleDayMarkerKinds
{
    None = 0,
    Vacation = 1,
    Sickness = 2,
    GuaranteedDayOff = 4,
    GeneratedDayOff = 8,
}
