using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed class RuleDefinitionValidationResult
{
    private RuleDefinitionValidationResult(
        RuleDefinition? value,
        ReadOnlyCollection<RuleDefinitionValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public RuleDefinition? Value { get; }

    public IReadOnlyList<RuleDefinitionValidationError> Errors { get; }

    internal static RuleDefinitionValidationResult Success(RuleDefinition value)
    {
        return new RuleDefinitionValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<RuleDefinitionValidationError>()));
    }

    internal static RuleDefinitionValidationResult Failure(
        IEnumerable<RuleDefinitionValidationError> errors)
    {
        return new RuleDefinitionValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
