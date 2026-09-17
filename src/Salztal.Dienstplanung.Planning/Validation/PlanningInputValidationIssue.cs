namespace Salztal.Dienstplanung.Planning.Validation;

internal sealed record PlanningInputValidationIssue(
    PlanningInputValidationCode Code,
    string? RuleId = null,
    Guid? TechnicalEntityId = null,
    DateOnly? Date = null,
    int? CatalogVersion = null,
    string? Parameter = null);
