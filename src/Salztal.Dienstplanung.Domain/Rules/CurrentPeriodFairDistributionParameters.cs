namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record CurrentPeriodFairDistributionParameters : RuleParameters
{
    internal CurrentPeriodFairDistributionParameters(
        int periodWeeks,
        FairDistributionSubjects subjects)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(periodWeeks);

        const FairDistributionSubjects allDefinedSubjects =
            FairDistributionSubjects.UnfavorableShifts
            | FairDistributionSubjects.SplitShifts
            | FairDistributionSubjects.Weekends;

        if (subjects == FairDistributionSubjects.None
            || (subjects & ~allDefinedSubjects) != FairDistributionSubjects.None)
        {
            throw new ArgumentOutOfRangeException(nameof(subjects));
        }

        PeriodWeeks = periodWeeks;
        Subjects = subjects;
    }

    public int PeriodWeeks { get; }

    public FairDistributionSubjects Subjects { get; }
}
