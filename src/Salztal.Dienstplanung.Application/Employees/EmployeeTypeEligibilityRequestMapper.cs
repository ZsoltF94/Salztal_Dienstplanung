using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Employees;

internal sealed record EmployeeTypeEligibilityMappingResult(
    IReadOnlyList<EmployeeTypeShiftEligibility> Eligibilities,
    IReadOnlyList<EmployeeTypeCommandError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}

internal static class EmployeeTypeEligibilityRequestMapper
{
    public static EmployeeTypeEligibilityMappingResult Map(
        IEnumerable<EmployeeTypeEligibilityRequest?>? requests,
        ServiceCatalogData serviceCatalog)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);

        if (requests is null)
        {
            return Failure(
                EmployeeTypeCommandErrorCode.ShiftEligibilityRequired,
                "Die Einsatzfreigaben konnten nicht vollständig gelesen werden.");
        }

        List<EmployeeTypeShiftEligibility> eligibilities = [];
        List<EmployeeTypeCommandError> errors = [];

        foreach (EmployeeTypeEligibilityRequest? request in requests)
        {
            MapRequest(request, serviceCatalog, eligibilities, errors);
        }

        return new EmployeeTypeEligibilityMappingResult(
            Array.AsReadOnly(eligibilities.ToArray()),
            Array.AsReadOnly(errors.ToArray()));
    }

    private static void MapRequest(
        EmployeeTypeEligibilityRequest? request,
        ServiceCatalogData serviceCatalog,
        List<EmployeeTypeShiftEligibility> eligibilities,
        List<EmployeeTypeCommandError> errors)
    {
        if (request is null)
        {
            errors.Add(Error(
                EmployeeTypeCommandErrorCode.ShiftEligibilityRequired,
                "Die Einsatzfreigaben konnten nicht vollständig gelesen werden."));
            return;
        }

        if (!Enum.IsDefined(request.TargetKind))
        {
            errors.Add(Error(
                EmployeeTypeCommandErrorCode.EligibilityTargetRequired,
                "Die Art einer Einsatzfreigabe wird nicht unterstützt."));
            return;
        }

        if (!TryMapMode(request.Mode, out ShiftEligibilityMode mode))
        {
            errors.Add(Error(
                EmployeeTypeCommandErrorCode.UnsupportedEligibilityMode,
                "Die Art der Einsatzfreigabe wird nicht unterstützt."));
            return;
        }

        if (!TryMapActivation(
                request.Activation,
                out ShiftEligibilityActivation activation))
        {
            errors.Add(Error(
                EmployeeTypeCommandErrorCode.UnsupportedEligibilityActivation,
                "Die Aktivierung der Einsatzfreigabe wird nicht unterstützt."));
            return;
        }

        if (request.TargetKind == EmployeeTypeEligibilityTargetKind.ShiftType
            && activation != ShiftEligibilityActivation.Always)
        {
            errors.Add(Error(
                EmployeeTypeCommandErrorCode.UnsupportedEligibilityActivation,
                "Normale Dienstfreigaben dürfen nicht von einer Planungslaufoption abhängen."));
            return;
        }

        EmployeeTypeShiftEligibilityValidationResult validation = request.TargetKind switch
        {
            EmployeeTypeEligibilityTargetKind.ShiftType =>
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    request.TargetId,
                    mode,
                    serviceCatalog.ShiftTypes.Select(shiftType => shiftType.Id).ToArray()),
            EmployeeTypeEligibilityTargetKind.ShiftPattern =>
                EmployeeTypeShiftEligibility.CreateForShiftPattern(
                    request.TargetId,
                    mode,
                    activation,
                    CreateKnownShiftPatternIds(serviceCatalog)),
            _ => throw new InvalidOperationException("Unsupported eligibility target kind."),
        };

        if (validation.IsSuccess)
        {
            eligibilities.Add(validation.Value!);
            return;
        }

        foreach (EmployeeTypeShiftEligibilityValidationError error in validation.Errors)
        {
            errors.Add(FromValidation(error));
        }
    }

    private static IReadOnlyCollection<ShiftPatternId> CreateKnownShiftPatternIds(
        ServiceCatalogData serviceCatalog)
    {
        return
        [
            serviceCatalog.SplitShiftPattern.Id,
            serviceCatalog.ReliefShiftPattern.Id,
        ];
    }

    private static bool TryMapMode(
        EmployeeTypeEligibilityMode value,
        out ShiftEligibilityMode mode)
    {
        mode = value switch
        {
            EmployeeTypeEligibilityMode.Regular => ShiftEligibilityMode.Regular,
            EmployeeTypeEligibilityMode.ManualSuggestion =>
                ShiftEligibilityMode.ManualSuggestion,
            _ => default,
        };

        return Enum.IsDefined(value);
    }

    private static bool TryMapActivation(
        EmployeeTypeEligibilityActivation value,
        out ShiftEligibilityActivation activation)
    {
        activation = value switch
        {
            EmployeeTypeEligibilityActivation.Always =>
                ShiftEligibilityActivation.Always,
            EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption =>
                ShiftEligibilityActivation.ExplicitPlanningRunOption,
            _ => default,
        };

        return Enum.IsDefined(value);
    }

    private static EmployeeTypeCommandError FromValidation(
        EmployeeTypeShiftEligibilityValidationError error)
    {
        return error.Code switch
        {
            EmployeeTypeShiftEligibilityValidationCode.IdentifierRequired => Error(
                EmployeeTypeCommandErrorCode.EligibilityTargetRequired,
                "Eine Einsatzfreigabe besitzt keine gültige Kennung."),
            EmployeeTypeShiftEligibilityValidationCode.UnknownShiftType => Error(
                EmployeeTypeCommandErrorCode.UnknownShiftType,
                "Ein ausgewählter Diensttyp wurde nicht gefunden. Bitte laden Sie die Daten neu."),
            EmployeeTypeShiftEligibilityValidationCode.UnknownShiftPattern => Error(
                EmployeeTypeCommandErrorCode.UnknownShiftPattern,
                "Ein ausgewähltes Einsatzmuster wurde nicht gefunden. Bitte laden Sie die Daten neu."),
            EmployeeTypeShiftEligibilityValidationCode.UnsupportedMode => Error(
                EmployeeTypeCommandErrorCode.UnsupportedEligibilityMode,
                "Die Art der Einsatzfreigabe wird nicht unterstützt."),
            EmployeeTypeShiftEligibilityValidationCode.UnsupportedActivation => Error(
                EmployeeTypeCommandErrorCode.UnsupportedEligibilityActivation,
                "Normale Dienstfreigaben dürfen nicht von einer Planungslaufoption abhängen."),
            _ => throw new InvalidOperationException(
                $"Unsupported shift-eligibility validation code: {error.Code}"),
        };
    }

    private static EmployeeTypeEligibilityMappingResult Failure(
        EmployeeTypeCommandErrorCode code,
        string message)
    {
        return new EmployeeTypeEligibilityMappingResult(
            Array.Empty<EmployeeTypeShiftEligibility>(),
            [Error(code, message)]);
    }

    private static EmployeeTypeCommandError Error(
        EmployeeTypeCommandErrorCode code,
        string message)
    {
        return new EmployeeTypeCommandError(code, message);
    }
}
