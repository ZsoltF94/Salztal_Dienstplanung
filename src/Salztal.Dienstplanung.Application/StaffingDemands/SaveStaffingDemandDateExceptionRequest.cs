using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record SaveStaffingDemandDateExceptionRequest(
    Guid ExceptionId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    StaffingDemandDateExceptionKind Kind,
    TimeOnly? ActualStart,
    TimeOnly? ActualEnd,
    int? RequiredEmployeeCount,
    Guid? ExpectedCurrentExceptionId)
{
    public static SaveStaffingDemandDateExceptionRequest CreateAddition(
        Guid exceptionId,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int? requiredEmployeeCount,
        Guid? expectedCurrentExceptionId)
    {
        return new SaveStaffingDemandDateExceptionRequest(
            exceptionId,
            date,
            workLocationId,
            shiftTypeId,
            StaffingDemandDateExceptionKind.Add,
            actualStart,
            actualEnd,
            requiredEmployeeCount,
            expectedCurrentExceptionId);
    }

    public static SaveStaffingDemandDateExceptionRequest CreateReplacement(
        Guid exceptionId,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int? requiredEmployeeCount,
        Guid? expectedCurrentExceptionId)
    {
        return new SaveStaffingDemandDateExceptionRequest(
            exceptionId,
            date,
            workLocationId,
            shiftTypeId,
            StaffingDemandDateExceptionKind.Replace,
            actualStart,
            actualEnd,
            requiredEmployeeCount,
            expectedCurrentExceptionId);
    }

    public static SaveStaffingDemandDateExceptionRequest CreateRemoval(
        Guid exceptionId,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        Guid? expectedCurrentExceptionId)
    {
        return new SaveStaffingDemandDateExceptionRequest(
            exceptionId,
            date,
            workLocationId,
            shiftTypeId,
            StaffingDemandDateExceptionKind.Remove,
            null,
            null,
            null,
            expectedCurrentExceptionId);
    }
}
