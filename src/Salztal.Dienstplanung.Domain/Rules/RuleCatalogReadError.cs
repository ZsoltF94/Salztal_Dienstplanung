namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleCatalogReadError(
    RuleCatalogReadCode Code,
    RuleCatalogVersion RequestedVersion);
