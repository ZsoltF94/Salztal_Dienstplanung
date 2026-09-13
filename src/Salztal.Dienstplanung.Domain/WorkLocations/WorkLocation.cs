namespace Salztal.Dienstplanung.Domain.WorkLocations;

public sealed class WorkLocation
{
    private WorkLocation(
        WorkLocationId id,
        WorkLocationName name,
        WorkLocationColor color)
    {
        Id = id;
        Name = name;
        Color = color;
    }

    public WorkLocationId Id { get; }

    public WorkLocationName Name { get; }

    public WorkLocationColor Color { get; }

    public static WorkLocationValidationResult Create(
        Guid id,
        string? name,
        string? colorCode)
    {
        List<WorkLocationValidationError> errors = [];

        if (!WorkLocationId.TryCreate(id, out WorkLocationId? workLocationId))
        {
            errors.Add(new WorkLocationValidationError(
                WorkLocationValidationCode.IdentifierRequired));
        }

        if (!WorkLocationName.TryCreate(name, out WorkLocationName? workLocationName))
        {
            errors.Add(new WorkLocationValidationError(
                WorkLocationValidationCode.NameRequired));
        }

        if (!WorkLocationColor.TryCreate(colorCode, out WorkLocationColor? workLocationColor))
        {
            errors.Add(new WorkLocationValidationError(
                WorkLocationValidationCode.ColorRequired));
        }

        if (errors.Count > 0)
        {
            return WorkLocationValidationResult.Failure(errors);
        }

        WorkLocation workLocation = new(
            workLocationId!,
            workLocationName!,
            workLocationColor!);

        return WorkLocationValidationResult.Success(workLocation);
    }

    public WorkLocationValidationResult WithDetails(string? name, string? colorCode)
    {
        return Create(Id.Value, name, colorCode);
    }
}
