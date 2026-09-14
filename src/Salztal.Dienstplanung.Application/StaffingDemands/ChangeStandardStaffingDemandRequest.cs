using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record ChangeStandardStaffingDemandRequest(
    Guid RevisionId,
    DayOfWeek DayOfWeek,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    DateOnly EffectiveFromMonday,
    StandardStaffingDemandRevisionKind Kind,
    TimeOnly? ActualStart,
    TimeOnly? ActualEnd,
    int? RequiredEmployeeCount,
    Guid? ExpectedCurrentRevisionId)
{
    public static ChangeStandardStaffingDemandRequest CreateAddition(
        Guid revisionId,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int? requiredEmployeeCount,
        Guid? expectedCurrentRevisionId)
    {
        return new ChangeStandardStaffingDemandRequest(
            revisionId,
            dayOfWeek,
            workLocationId,
            shiftTypeId,
            effectiveFromMonday,
            StandardStaffingDemandRevisionKind.Add,
            actualStart,
            actualEnd,
            requiredEmployeeCount,
            expectedCurrentRevisionId);
    }

    public static ChangeStandardStaffingDemandRequest CreateReplacement(
        Guid revisionId,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int? requiredEmployeeCount,
        Guid? expectedCurrentRevisionId)
    {
        return new ChangeStandardStaffingDemandRequest(
            revisionId,
            dayOfWeek,
            workLocationId,
            shiftTypeId,
            effectiveFromMonday,
            StandardStaffingDemandRevisionKind.Replace,
            actualStart,
            actualEnd,
            requiredEmployeeCount,
            expectedCurrentRevisionId);
    }

    public static ChangeStandardStaffingDemandRequest CreateRemoval(
        Guid revisionId,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        Guid? expectedCurrentRevisionId)
    {
        return new ChangeStandardStaffingDemandRequest(
            revisionId,
            dayOfWeek,
            workLocationId,
            shiftTypeId,
            effectiveFromMonday,
            StandardStaffingDemandRevisionKind.Remove,
            null,
            null,
            null,
            expectedCurrentRevisionId);
    }
}
