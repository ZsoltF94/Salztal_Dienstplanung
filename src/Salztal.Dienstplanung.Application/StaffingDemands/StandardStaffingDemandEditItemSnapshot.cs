namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record StandardStaffingDemandEditItemSnapshot(
    DateOnly EffectiveFromMonday,
    DayOfWeek DayOfWeek,
    Guid WorkLocationId,
    string WorkLocationName,
    Guid ShiftTypeId,
    string ShiftTypeName,
    TimeOnly ShiftTypeStandardStart,
    TimeOnly ShiftTypeStandardEnd,
    bool HasEffectiveStandard,
    bool HasRevisionAtEffectiveMonday,
    Guid? ExpectedCurrentRevisionId,
    TimeOnly? ActualStart,
    TimeOnly? ActualEnd,
    int? RequiredEmployeeCount);
