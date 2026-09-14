using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

internal static class StaffingDemandWeekQueryErrors
{
    public static StaffingDemandWeekQueryError FromStandardRevision(
        StandardStaffingDemandRevisionSetValidationError error)
    {
        StaffingDemandWeekQueryErrorCode code = error.Code ==
            StandardStaffingDemandRevisionSetValidationCode
                .DuplicateKeyEffectiveMondayAndCorrectionSequence
                ? StaffingDemandWeekQueryErrorCode.DuplicateStandardRevision
                : StaffingDemandWeekQueryErrorCode.InvalidStandardRevisionSequence;

        string message = code == StaffingDemandWeekQueryErrorCode.DuplicateStandardRevision
            ? $"Für denselben Wochenstandard ist ab {error.EffectiveFromMonday:dd.MM.yyyy} mehr als eine Revision gespeichert."
            : $"Die gespeicherte Reihenfolge der Wochenstandard-Revisionen ist ab {error.EffectiveFromMonday:dd.MM.yyyy} widersprüchlich.";

        return new StaffingDemandWeekQueryError(code, message);
    }

    public static StaffingDemandWeekQueryError FromDateException(
        StaffingDemandDateExceptionSetValidationError error)
    {
        return new StaffingDemandWeekQueryError(
            StaffingDemandWeekQueryErrorCode.DuplicateDateException,
            $"Für den Bedarf am {error.Key.Date:dd.MM.yyyy} ist mehr als eine Datumsausnahme gespeichert.");
    }

    public static StaffingDemandWeekQueryResult FromWeekResolution(
        IEnumerable<StaffingDemandWeekResolutionError> errors)
    {
        StaffingDemandWeekQueryError[] mappedErrors = errors
            .Select(FromWeekResolutionError)
            .ToArray();
        bool isInputValidation = mappedErrors.All(error =>
            error.Code is StaffingDemandWeekQueryErrorCode.WeekStartMustBeMonday
                or StaffingDemandWeekQueryErrorCode.WeekMustFitSevenDays);

        return StaffingDemandWeekQueryResult.Failure(
            isInputValidation
                ? StaffingDemandWeekQueryStatus.ValidationFailed
                : StaffingDemandWeekQueryStatus.StoredDataInvalid,
            mappedErrors);
    }

    private static StaffingDemandWeekQueryError FromWeekResolutionError(
        StaffingDemandWeekResolutionError error)
    {
        return error.Code switch
        {
            StaffingDemandWeekResolutionCode.WeekStartMustBeMonday => new(
                StaffingDemandWeekQueryErrorCode.WeekStartMustBeMonday,
                "Die ausgewählte Woche muss an einem Montag beginnen."),
            StaffingDemandWeekResolutionCode.WeekMustFitSevenDays => new(
                StaffingDemandWeekQueryErrorCode.WeekMustFitSevenDays,
                "Ab dem ausgewählten Montag kann keine vollständige Sieben-Tage-Woche gebildet werden."),
            StaffingDemandWeekResolutionCode.RequiredWorkMinutesOverflow => new(
                StaffingDemandWeekQueryErrorCode.RequiredWorkMinutesOverflow,
                "Die bestätigten Bedarfsminuten überschreiten den zulässigen Zahlenbereich."),
            StaffingDemandWeekResolutionCode.AdditionRequiresMissingStandard
                or StaffingDemandWeekResolutionCode.ReplacementRequiresExistingStandard
                or StaffingDemandWeekResolutionCode.RemovalRequiresExistingStandard => new(
                    StaffingDemandWeekQueryErrorCode.InvalidDateExceptionApplication,
                    $"Die Datumsausnahme am {error.Key!.Date:dd.MM.yyyy} passt nicht zum wirksamen Wochenstandard."),
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand resolution code: {error.Code}"),
        };
    }
}
