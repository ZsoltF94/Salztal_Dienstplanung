using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed class ShiftType
{
    private ShiftType(
        ShiftTypeId id,
        ShiftTypeName name,
        WorkLocationId workLocationId,
        ShiftTypeDisplay display,
        ShiftStandardTime standardTime)
    {
        Id = id;
        Name = name;
        WorkLocationId = workLocationId;
        Display = display;
        StandardTime = standardTime;
    }

    public ShiftTypeId Id { get; }

    public ShiftTypeName Name { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeDisplay Display { get; }

    public ShiftStandardTime StandardTime { get; }

    public static ShiftTypeValidationResult Create(
        Guid id,
        string? name,
        WorkLocationId? workLocationId,
        ShiftTypeDisplayKind displayKind,
        string? abbreviation,
        ShiftStandardTime? standardTime)
    {
        List<ShiftTypeValidationError> errors = [];

        if (!ShiftTypeId.TryCreate(id, out ShiftTypeId? shiftTypeId))
        {
            errors.Add(new ShiftTypeValidationError(
                ShiftTypeValidationCode.IdentifierRequired));
        }

        if (!ShiftTypeName.TryCreate(name, out ShiftTypeName? shiftTypeName))
        {
            errors.Add(new ShiftTypeValidationError(
                ShiftTypeValidationCode.NameRequired));
        }

        if (workLocationId is null)
        {
            errors.Add(new ShiftTypeValidationError(
                ShiftTypeValidationCode.WorkLocationRequired));
        }

        ShiftTypeDisplay? display = CreateDisplay(displayKind, abbreviation, errors);

        if (standardTime is null)
        {
            errors.Add(new ShiftTypeValidationError(
                ShiftTypeValidationCode.StandardTimeRequired));
        }

        if (errors.Count > 0)
        {
            return ShiftTypeValidationResult.Failure(errors);
        }

        ShiftType shiftType = new(
            shiftTypeId!,
            shiftTypeName!,
            workLocationId!,
            display!,
            standardTime!);

        return ShiftTypeValidationResult.Success(shiftType);
    }

    public ShiftTypeValidationResult WithStandardTime(ShiftStandardTime? standardTime)
    {
        return Create(
            Id.Value,
            Name.Value,
            WorkLocationId,
            Display.Kind,
            Display.Abbreviation,
            standardTime);
    }

    private static ShiftTypeDisplay? CreateDisplay(
        ShiftTypeDisplayKind displayKind,
        string? abbreviation,
        List<ShiftTypeValidationError> errors)
    {
        switch (displayKind)
        {
            case ShiftTypeDisplayKind.Abbreviation:
                if (string.IsNullOrWhiteSpace(abbreviation))
                {
                    errors.Add(new ShiftTypeValidationError(
                        ShiftTypeValidationCode.AbbreviationRequired));
                    return null;
                }

                return new ShiftTypeDisplay(displayKind, abbreviation.Trim());

            case ShiftTypeDisplayKind.ActualTime:
                if (!string.IsNullOrWhiteSpace(abbreviation))
                {
                    errors.Add(new ShiftTypeValidationError(
                        ShiftTypeValidationCode.AbbreviationNotAllowedForActualTime));
                    return null;
                }

                return new ShiftTypeDisplay(displayKind, null);

            default:
                errors.Add(new ShiftTypeValidationError(
                    ShiftTypeValidationCode.UnsupportedDisplayKind));
                return null;
        }
    }
}
