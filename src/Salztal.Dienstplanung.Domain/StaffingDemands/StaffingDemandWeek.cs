using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandWeek
{
    private static readonly DateOnly LatestCompleteWeekStart =
        DateOnly.MaxValue.AddDays(-6);

    private StaffingDemandWeek(
        DateOnly weekMonday,
        ReadOnlyCollection<EffectiveStaffingDemand> demands,
        ReadOnlyCollection<StaffingDemandDayWorkLocationSummary> daySummaries,
        ReadOnlyCollection<StaffingDemandWorkLocationWeekSummary> workLocationSummaries,
        long totalRequiredWorkMinutes)
    {
        WeekMonday = weekMonday;
        Demands = demands;
        DaySummaries = daySummaries;
        WorkLocationSummaries = workLocationSummaries;
        TotalRequiredWorkMinutes = totalRequiredWorkMinutes;
    }

    public DateOnly WeekMonday { get; }

    public IReadOnlyList<EffectiveStaffingDemand> Demands { get; }

    public IReadOnlyList<StaffingDemandDayWorkLocationSummary> DaySummaries { get; }

    public IReadOnlyList<StaffingDemandWorkLocationWeekSummary> WorkLocationSummaries { get; }

    public long TotalRequiredWorkMinutes { get; }

    public static StaffingDemandWeekResolutionResult Resolve(
        DateOnly weekMonday,
        StandardStaffingDemandRevisionSet standardRevisions,
        StaffingDemandDateExceptionSet dateExceptions)
    {
        ArgumentNullException.ThrowIfNull(standardRevisions);
        ArgumentNullException.ThrowIfNull(dateExceptions);

        List<StaffingDemandWeekResolutionError> errors = [];

        if (weekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            errors.Add(new StaffingDemandWeekResolutionError(
                StaffingDemandWeekResolutionCode.WeekStartMustBeMonday));
        }

        if (weekMonday > LatestCompleteWeekStart)
        {
            errors.Add(new StaffingDemandWeekResolutionError(
                StaffingDemandWeekResolutionCode.WeekMustFitSevenDays));
        }

        if (errors.Count > 0)
        {
            return StaffingDemandWeekResolutionResult.Failure(errors);
        }

        DateOnly weekSunday = weekMonday.AddDays(6);
        Dictionary<StaffingDemandDateKey, EffectiveStaffingDemand> effectiveDemands =
            ResolveStandards(weekMonday, standardRevisions);

        foreach (StaffingDemandDateException dateException in dateExceptions.Exceptions
                     .Where(exception => exception.Key.Date >= weekMonday)
                     .Where(exception => exception.Key.Date <= weekSunday))
        {
            ApplyDateException(dateException, effectiveDemands, errors);
        }

        if (errors.Count > 0)
        {
            return StaffingDemandWeekResolutionResult.Failure(errors);
        }

        try
        {
            return StaffingDemandWeekResolutionResult.Success(
                CreateResolvedWeek(weekMonday, effectiveDemands.Values));
        }
        catch (OverflowException)
        {
            return StaffingDemandWeekResolutionResult.Failure(
                [
                    new StaffingDemandWeekResolutionError(
                        StaffingDemandWeekResolutionCode.RequiredWorkMinutesOverflow),
                ]);
        }
    }

    private static Dictionary<StaffingDemandDateKey, EffectiveStaffingDemand>
        ResolveStandards(
            DateOnly weekMonday,
            StandardStaffingDemandRevisionSet standardRevisions)
    {
        Dictionary<StaffingDemandDateKey, EffectiveStaffingDemand> effectiveDemands = [];
        StandardStaffingDemandKey[] standardKeys = standardRevisions.Revisions
            .Select(revision => revision.Key)
            .Distinct()
            .ToArray();

        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            DateOnly date = weekMonday.AddDays(dayOffset);

            foreach (StandardStaffingDemandKey standardKey in standardKeys
                         .Where(key => key.DayOfWeek == date.DayOfWeek))
            {
                StandardStaffingDemandRevision? revision =
                    standardRevisions.FindEffectiveRevision(standardKey, date);
                if (revision is null)
                {
                    continue;
                }

                StaffingDemandDateKey dateKey = StaffingDemandDateKey.CreateValidated(
                    date,
                    standardKey.WorkLocationId,
                    standardKey.ShiftTypeId);
                effectiveDemands.Add(
                    dateKey,
                    EffectiveStaffingDemand.FromStandard(date, revision));
            }
        }

        return effectiveDemands;
    }

    private static void ApplyDateException(
        StaffingDemandDateException dateException,
        Dictionary<StaffingDemandDateKey, EffectiveStaffingDemand> effectiveDemands,
        List<StaffingDemandWeekResolutionError> errors)
    {
        bool hasStandard = effectiveDemands.ContainsKey(dateException.Key);
        StaffingDemandWeekResolutionCode? errorCode = dateException.Kind switch
        {
            StaffingDemandDateExceptionKind.Add when hasStandard =>
                StaffingDemandWeekResolutionCode.AdditionRequiresMissingStandard,
            StaffingDemandDateExceptionKind.Replace when !hasStandard =>
                StaffingDemandWeekResolutionCode.ReplacementRequiresExistingStandard,
            StaffingDemandDateExceptionKind.Remove when !hasStandard =>
                StaffingDemandWeekResolutionCode.RemovalRequiresExistingStandard,
            _ => null,
        };

        if (errorCode is not null)
        {
            errors.Add(new StaffingDemandWeekResolutionError(
                errorCode.Value,
                dateException.Key));
            return;
        }

        if (dateException.Kind == StaffingDemandDateExceptionKind.Remove)
        {
            effectiveDemands.Remove(dateException.Key);
            return;
        }

        effectiveDemands[dateException.Key] =
            EffectiveStaffingDemand.FromDateException(dateException);
    }

    private static StaffingDemandWeek CreateResolvedWeek(
        DateOnly weekMonday,
        IEnumerable<EffectiveStaffingDemand> effectiveDemands)
    {
        EffectiveStaffingDemand[] orderedDemands = effectiveDemands
            .OrderBy(demand => demand.Date)
            .ThenBy(demand => demand.WorkLocationId.Value)
            .ThenBy(demand => demand.ShiftTypeId.Value)
            .ToArray();

        StaffingDemandDayWorkLocationSummary[] daySummaries = orderedDemands
            .GroupBy(demand => (demand.Date, demand.WorkLocationId))
            .Select(
                group => new StaffingDemandDayWorkLocationSummary(
                    group.Key.Date,
                    group.Key.WorkLocationId,
                    group.Sum(demand => demand.RequiredWorkMinutes)))
            .OrderBy(summary => summary.Date)
            .ThenBy(summary => summary.WorkLocationId.Value)
            .ToArray();

        StaffingDemandWorkLocationWeekSummary[] workLocationSummaries = orderedDemands
            .GroupBy(demand => demand.WorkLocationId)
            .Select(
                group => new StaffingDemandWorkLocationWeekSummary(
                    group.Key,
                    group.Sum(demand => demand.RequiredWorkMinutes)))
            .OrderBy(summary => summary.WorkLocationId.Value)
            .ToArray();

        return new StaffingDemandWeek(
            weekMonday,
            Array.AsReadOnly(orderedDemands),
            Array.AsReadOnly(daySummaries),
            Array.AsReadOnly(workLocationSummaries),
            orderedDemands.Sum(demand => demand.RequiredWorkMinutes));
    }
}
