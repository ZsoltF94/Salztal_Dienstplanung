using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypeShiftEligibility
{
    private EmployeeTypeShiftEligibility(
        ShiftEligibilityTargetKind targetKind,
        ShiftTypeId? shiftTypeId,
        ShiftPatternId? shiftPatternId,
        ShiftEligibilityMode mode,
        ShiftEligibilityActivation activation)
    {
        TargetKind = targetKind;
        ShiftTypeId = shiftTypeId;
        ShiftPatternId = shiftPatternId;
        Mode = mode;
        Activation = activation;
    }

    public ShiftEligibilityTargetKind TargetKind { get; }

    public ShiftTypeId? ShiftTypeId { get; }

    public ShiftPatternId? ShiftPatternId { get; }

    public ShiftEligibilityMode Mode { get; }

    public ShiftEligibilityActivation Activation { get; }

    public static EmployeeTypeShiftEligibilityValidationResult CreateForShiftType(
        Guid shiftTypeId,
        ShiftEligibilityMode mode,
        IReadOnlyCollection<ShiftTypeId> knownShiftTypeIds)
    {
        ArgumentNullException.ThrowIfNull(knownShiftTypeIds);

        List<EmployeeTypeShiftEligibilityValidationError> errors = [];

        if (!ShiftTypeId.TryCreate(shiftTypeId, out ShiftTypeId? validatedShiftTypeId))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.IdentifierRequired));
        }
        else if (!knownShiftTypeIds.Contains(validatedShiftTypeId))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.UnknownShiftType));
        }

        if (!Enum.IsDefined(mode))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.UnsupportedMode));
        }

        if (errors.Count > 0)
        {
            return EmployeeTypeShiftEligibilityValidationResult.Failure(errors);
        }

        return EmployeeTypeShiftEligibilityValidationResult.Success(
            new EmployeeTypeShiftEligibility(
                ShiftEligibilityTargetKind.ShiftType,
                validatedShiftTypeId!,
                null,
                mode,
                ShiftEligibilityActivation.Always));
    }

    public static EmployeeTypeShiftEligibilityValidationResult CreateForShiftPattern(
        Guid shiftPatternId,
        ShiftEligibilityMode mode,
        ShiftEligibilityActivation activation,
        IReadOnlyCollection<ShiftPatternId> knownShiftPatternIds)
    {
        ArgumentNullException.ThrowIfNull(knownShiftPatternIds);

        List<EmployeeTypeShiftEligibilityValidationError> errors = [];

        if (!ShiftPatternId.TryCreate(
                shiftPatternId,
                out ShiftPatternId? validatedShiftPatternId))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.IdentifierRequired));
        }
        else if (!knownShiftPatternIds.Contains(validatedShiftPatternId))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.UnknownShiftPattern));
        }

        if (!Enum.IsDefined(mode))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.UnsupportedMode));
        }

        if (!Enum.IsDefined(activation))
        {
            errors.Add(new EmployeeTypeShiftEligibilityValidationError(
                EmployeeTypeShiftEligibilityValidationCode.UnsupportedActivation));
        }

        if (errors.Count > 0)
        {
            return EmployeeTypeShiftEligibilityValidationResult.Failure(errors);
        }

        return EmployeeTypeShiftEligibilityValidationResult.Success(
            new EmployeeTypeShiftEligibility(
                ShiftEligibilityTargetKind.ShiftPattern,
                null,
                validatedShiftPatternId!,
                mode,
                activation));
    }
}
