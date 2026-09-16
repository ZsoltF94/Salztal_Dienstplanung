namespace Salztal.Dienstplanung.Domain.Rules;

[Flags]
public enum FairDistributionSubjects
{
    None = 0,
    UnfavorableShifts = 1,
    SplitShifts = 2,
    Weekends = 4,
}
