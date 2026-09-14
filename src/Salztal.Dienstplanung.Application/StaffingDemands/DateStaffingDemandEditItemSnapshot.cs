namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record DateStaffingDemandEditItemSnapshot(
    DateOnly Date,
    Guid WorkLocationId,
    string WorkLocationName,
    string WorkLocationColorCode,
    Guid ShiftTypeId,
    string ShiftTypeName,
    TimeOnly ShiftTypeStandardStart,
    TimeOnly ShiftTypeStandardEnd,
    bool HasRegularStandard,
    TimeOnly? RegularActualStart,
    TimeOnly? RegularActualEnd,
    int? RegularRequiredEmployeeCount,
    bool HasEffectiveDemand,
    bool HasDateException,
    bool IsNoDemandChange,
    Guid? ExpectedCurrentExceptionId,
    TimeOnly? ActualStart,
    TimeOnly? ActualEnd,
    int? RequiredEmployeeCount);
