using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

internal static class StaffingDemandCatalogValidator
{
    public static StaffingDemandCatalogValidationResult Validate(
        StaffingDemandReadData data)
    {
        List<StaffingDemandWeekQueryError> errors = [];
        Dictionary<WorkLocationId, WorkLocation> workLocationsById = [];
        Dictionary<ShiftTypeId, ShiftType> shiftTypesById = [];

        AddWorkLocations(data, workLocationsById, errors);
        AddShiftTypes(data, workLocationsById, shiftTypesById, errors);

        foreach ((WorkLocationId WorkLocationId, ShiftTypeId ShiftTypeId) reference in
                 data.StandardRevisions
                     .Select(revision => (
                         revision.Key.WorkLocationId,
                         revision.Key.ShiftTypeId))
                     .Concat(data.DateExceptions.Select(exception => (
                         exception.Key.WorkLocationId,
                         exception.Key.ShiftTypeId)))
                     .Distinct())
        {
            ValidateReference(reference, workLocationsById, shiftTypesById, errors);
        }

        return new StaffingDemandCatalogValidationResult(
            workLocationsById,
            shiftTypesById,
            errors);
    }

    private static void AddWorkLocations(
        StaffingDemandReadData data,
        Dictionary<WorkLocationId, WorkLocation> workLocationsById,
        List<StaffingDemandWeekQueryError> errors)
    {
        foreach (IGrouping<WorkLocationId, WorkLocation> group in
                 data.ServiceCatalog.WorkLocations.GroupBy(location => location.Id))
        {
            if (group.Count() > 1)
            {
                errors.Add(new StaffingDemandWeekQueryError(
                    StaffingDemandWeekQueryErrorCode.DuplicateWorkLocation,
                    $"Der Einsatzort mit der Kennung {group.Key.Value} ist mehrfach gespeichert."));
                continue;
            }

            workLocationsById.Add(group.Key, group.Single());
        }
    }

    private static void AddShiftTypes(
        StaffingDemandReadData data,
        Dictionary<WorkLocationId, WorkLocation> workLocationsById,
        Dictionary<ShiftTypeId, ShiftType> shiftTypesById,
        List<StaffingDemandWeekQueryError> errors)
    {
        foreach (IGrouping<ShiftTypeId, ShiftType> group in
                 data.ServiceCatalog.ShiftTypes.GroupBy(shiftType => shiftType.Id))
        {
            if (group.Count() > 1)
            {
                errors.Add(new StaffingDemandWeekQueryError(
                    StaffingDemandWeekQueryErrorCode.DuplicateShiftType,
                    $"Der Diensttyp mit der Kennung {group.Key.Value} ist mehrfach gespeichert."));
                continue;
            }

            ShiftType shiftType = group.Single();
            shiftTypesById.Add(group.Key, shiftType);
            if (!workLocationsById.ContainsKey(shiftType.WorkLocationId))
            {
                errors.Add(new StaffingDemandWeekQueryError(
                    StaffingDemandWeekQueryErrorCode.ShiftTypeWorkLocationNotFound,
                    $"Der Einsatzort des Diensttyps {shiftType.Id.Value} ist nicht im Dienstkatalog vorhanden."));
            }
        }
    }

    private static void ValidateReference(
        (WorkLocationId WorkLocationId, ShiftTypeId ShiftTypeId) reference,
        Dictionary<WorkLocationId, WorkLocation> workLocationsById,
        Dictionary<ShiftTypeId, ShiftType> shiftTypesById,
        List<StaffingDemandWeekQueryError> errors)
    {
        bool hasWorkLocation = workLocationsById.ContainsKey(reference.WorkLocationId);
        bool hasShiftType = shiftTypesById.TryGetValue(
            reference.ShiftTypeId,
            out ShiftType? shiftType);

        if (!hasWorkLocation)
        {
            errors.Add(new StaffingDemandWeekQueryError(
                StaffingDemandWeekQueryErrorCode.WorkLocationNotFound,
                $"Der Bedarf verweist auf den unbekannten Einsatzort {reference.WorkLocationId.Value}."));
        }

        if (!hasShiftType)
        {
            errors.Add(new StaffingDemandWeekQueryError(
                StaffingDemandWeekQueryErrorCode.ShiftTypeNotFound,
                $"Der Bedarf verweist auf den unbekannten Diensttyp {reference.ShiftTypeId.Value}."));
        }
        else if (hasWorkLocation && shiftType!.WorkLocationId != reference.WorkLocationId)
        {
            errors.Add(new StaffingDemandWeekQueryError(
                StaffingDemandWeekQueryErrorCode.ShiftTypeWorkLocationMismatch,
                $"Diensttyp {reference.ShiftTypeId.Value} gehört nicht zum Einsatzort {reference.WorkLocationId.Value}."));
        }
    }
}

internal sealed record StaffingDemandCatalogValidationResult(
    IReadOnlyDictionary<WorkLocationId, WorkLocation> WorkLocationsById,
    IReadOnlyDictionary<ShiftTypeId, ShiftType> ShiftTypesById,
    IReadOnlyList<StaffingDemandWeekQueryError> Errors);
