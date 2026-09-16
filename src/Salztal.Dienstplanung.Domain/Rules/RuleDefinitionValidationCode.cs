namespace Salztal.Dienstplanung.Domain.Rules;

public enum RuleDefinitionValidationCode
{
    IdentifierInvalid,
    DescriptionKeyRequired,
    FamilyUnknown,
    ScopeUnknown,
    AutomaticEffectUnknown,
    ManualEffectUnknown,
    PriorityUnknown,
    ParametersRequired,
    EffectCombinationInvalid,
    PriorityRequired,
    PriorityNotAllowed,
}
