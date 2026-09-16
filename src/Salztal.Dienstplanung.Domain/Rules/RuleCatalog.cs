using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed class RuleCatalog
{
    private readonly ReadOnlyDictionary<RuleId, RuleDefinition> definitionsById;

    internal RuleCatalog(
        RuleCatalogVersion version,
        IEnumerable<RuleDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(definitions);

        RuleDefinition[] definitionArray = definitions.ToArray();
        if (definitionArray.Length == 0)
        {
            throw new ArgumentException("A rule catalog requires definitions.", nameof(definitions));
        }

        if (definitionArray.Any(definition => definition is null))
        {
            throw new ArgumentException(
                "A rule catalog cannot contain a null definition.",
                nameof(definitions));
        }

        Dictionary<RuleId, RuleDefinition> definitionsById =
            new(definitionArray.Length);

        foreach (RuleDefinition definition in definitionArray)
        {
            if (!definitionsById.TryAdd(definition.Id, definition))
            {
                throw new ArgumentException(
                    $"Duplicate rule identifier {definition.Id}.",
                    nameof(definitions));
            }
        }

        Version = version;
        Definitions = Array.AsReadOnly(definitionArray);
        this.definitionsById = new ReadOnlyDictionary<RuleId, RuleDefinition>(
            definitionsById);
    }

    public RuleCatalogVersion Version { get; }

    public IReadOnlyList<RuleDefinition> Definitions { get; }

    public RuleDefinitionLookupResult Find(RuleId ruleId)
    {
        ArgumentNullException.ThrowIfNull(ruleId);

        return definitionsById.TryGetValue(ruleId, out RuleDefinition? definition)
            ? RuleDefinitionLookupResult.Success(definition)
            : RuleDefinitionLookupResult.Failure(ruleId);
    }
}
