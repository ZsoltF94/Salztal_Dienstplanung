using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

internal sealed class StaffingDemandCommandContext
{
    private StaffingDemandCommandContext(
        StandardStaffingDemandRevisionSet standardRevisions,
        StaffingDemandDateExceptionSet dateExceptions)
    {
        StandardRevisions = standardRevisions;
        DateExceptions = dateExceptions;
    }

    public StandardStaffingDemandRevisionSet StandardRevisions { get; }

    public StaffingDemandDateExceptionSet DateExceptions { get; }

    public static StaffingDemandCommandContextValidationResult Create(
        StaffingDemandReadData data,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId)
    {
        StandardStaffingDemandRevisionSetValidationResult standardResult =
            StandardStaffingDemandRevisionSet.Create(data.StandardRevisions);
        StaffingDemandDateExceptionSetValidationResult exceptionResult =
            StaffingDemandDateExceptionSet.Create(data.DateExceptions);
        if (!standardResult.IsSuccess || !exceptionResult.IsSuccess)
        {
            return StaffingDemandCommandContextValidationResult.FromFailure(
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.StoredDataInvalid,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.StoredDataInvalid,
                        "Die gespeicherten Bedarfsdaten sind widersprüchlich. Bitte laden Sie die Daten neu.")]));
        }

        StaffingDemandCatalogValidationResult catalogResult =
            StaffingDemandCatalogValidator.Validate(data);
        if (catalogResult.Errors.Count > 0)
        {
            return StaffingDemandCommandContextValidationResult.FromFailure(
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.CatalogInvalid,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.CatalogInvalid,
                        "Der Dienstkatalog ist widersprüchlich. Bitte laden Sie die Daten neu.")]));
        }

        if (!catalogResult.WorkLocationsById.ContainsKey(workLocationId))
        {
            return StaffingDemandCommandContextValidationResult.FromFailure(
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.NotFound,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.WorkLocationNotFound,
                        "Der ausgewählte Einsatzort wurde nicht gefunden.")]));
        }

        if (!catalogResult.ShiftTypesById.TryGetValue(
                shiftTypeId,
                out ShiftType? shiftType))
        {
            return StaffingDemandCommandContextValidationResult.FromFailure(
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.NotFound,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.ShiftTypeNotFound,
                        "Der ausgewählte Diensttyp wurde nicht gefunden.")]));
        }

        if (shiftType.WorkLocationId != workLocationId)
        {
            return StaffingDemandCommandContextValidationResult.FromFailure(
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.ValidationFailed,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.ShiftTypeWorkLocationMismatch,
                        "Der ausgewählte Diensttyp gehört nicht zum ausgewählten Einsatzort.")]));
        }

        return StaffingDemandCommandContextValidationResult.Success(
            new StaffingDemandCommandContext(
                standardResult.Value!,
                exceptionResult.Value!));
    }
}

internal sealed class StaffingDemandCommandContextValidationResult
{
    private StaffingDemandCommandContextValidationResult(
        StaffingDemandCommandContext? value,
        StaffingDemandCommandResult? failure)
    {
        Value = value;
        Failure = failure;
    }

    public StaffingDemandCommandContext? Value { get; }

    public StaffingDemandCommandResult? Failure { get; }

    public static StaffingDemandCommandContextValidationResult Success(
        StaffingDemandCommandContext value)
    {
        return new StaffingDemandCommandContextValidationResult(value, null);
    }

    public static StaffingDemandCommandContextValidationResult FromFailure(
        StaffingDemandCommandResult failure)
    {
        return new StaffingDemandCommandContextValidationResult(null, failure);
    }
}
