using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class RuleCatalogExampleScenarioTests
{
    private const string SolutionFileName = "Salztal.Dienstplanung.sln";
    private const string ExamplesRelativePath = "docs/decisions/S07_RULE_CATALOG_EXAMPLES.md";

    private static readonly string[] RequiredOutcomes =
    [
        "SATISFIED",
        "VIOLATED",
        "NOT_APPLICABLE",
    ];

    [Fact]
    public void ExamplesDocumentReferencesEveryCatalogRuleAndRequiredOutcomeExactlyOnce()
    {
        RuleCatalogReadResult result = InitialRuleCatalog.Read(InitialRuleCatalog.Version);
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(result.Value);
        string document = ReadExamplesDocument();

        foreach (RuleDefinition definition in catalog.Definitions)
        {
            foreach (string outcome in RequiredOutcomes)
            {
                string scenarioId = $"`S07-{definition.Id.Value}-{outcome}`";
                Assert.Equal(1, CountOccurrences(document, scenarioId));
            }
        }

        const string incompleteHistoryScenario =
            "`S07-MAX_CONSECUTIVE_WORKDAYS-NOT_FULLY_EVALUABLE`";
        Assert.Equal(1, CountOccurrences(document, incompleteHistoryScenario));
    }

    private static string ReadExamplesDocument()
    {
        DirectoryInfo repositoryRoot = FindRepositoryRoot();
        string documentPath = Path.Combine(
            repositoryRoot.FullName,
            ExamplesRelativePath.Replace('/', Path.DirectorySeparatorChar));

        return File.ReadAllText(documentPath);
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Der Repository-Stamm mit der erwarteten Solution wurde nicht gefunden.");
    }

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int searchStart = 0;

        while ((searchStart = source.IndexOf(value, searchStart, StringComparison.Ordinal)) >= 0)
        {
            count++;
            searchStart += value.Length;
        }

        return count;
    }
}
