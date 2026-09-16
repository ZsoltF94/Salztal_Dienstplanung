namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record ReliefShiftEmergencyParameters : RuleParameters
{
    internal ReliefShiftEmergencyParameters(
        DayOfWeek allowedDay,
        ReliefShiftDemandCondition demandCondition,
        ReliefShiftCoverageStart coverageStart)
    {
        if (!Enum.IsDefined(allowedDay))
        {
            throw new ArgumentOutOfRangeException(nameof(allowedDay));
        }

        if (!Enum.IsDefined(demandCondition))
        {
            throw new ArgumentOutOfRangeException(nameof(demandCondition));
        }

        if (!Enum.IsDefined(coverageStart))
        {
            throw new ArgumentOutOfRangeException(nameof(coverageStart));
        }

        AllowedDay = allowedDay;
        DemandCondition = demandCondition;
        CoverageStart = coverageStart;
    }

    public DayOfWeek AllowedDay { get; }

    public ReliefShiftDemandCondition DemandCondition { get; }

    public ReliefShiftCoverageStart CoverageStart { get; }
}
