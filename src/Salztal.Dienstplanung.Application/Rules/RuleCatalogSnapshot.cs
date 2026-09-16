using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Rules;

public sealed class RuleCatalogSnapshot
{
    public RuleCatalogSnapshot(
        int version,
        IEnumerable<RuleDefinitionSnapshot> definitions)
    {
        Version = version;
        Definitions = Array.AsReadOnly(definitions.ToArray());
    }

    public int Version { get; }

    public ReadOnlyCollection<RuleDefinitionSnapshot> Definitions { get; }
}
