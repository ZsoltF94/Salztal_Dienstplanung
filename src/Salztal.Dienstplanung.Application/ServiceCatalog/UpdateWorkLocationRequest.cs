namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed record UpdateWorkLocationRequest(
    Guid WorkLocationId,
    string? Name,
    string? ColorCode);
