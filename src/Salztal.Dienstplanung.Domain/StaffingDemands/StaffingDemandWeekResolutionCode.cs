namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public enum StaffingDemandWeekResolutionCode
{
    WeekStartMustBeMonday,
    WeekMustFitSevenDays,
    AdditionRequiresMissingStandard,
    ReplacementRequiresExistingStandard,
    RemovalRequiresExistingStandard,
    RequiredWorkMinutesOverflow,
}
