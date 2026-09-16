using Salztal.Dienstplanung.Application.Rules;

namespace Salztal.Dienstplanung.Application.Tests.Rules;

public sealed class RuleCatalogCombinationScenarioTests
{
    private const string SolutionFileName = "Salztal.Dienstplanung.sln";
    private const string ExamplesRelativePath = "docs/decisions/S07_RULE_CATALOG_EXAMPLES.md";

    private static readonly string[] CombinationScenarioIds =
    [
        "S07-COMB-WEEKLY-CORRIDOR-NO-OVERSTAFFING",
        "S07-COMB-VACATION-COVERAGE",
        "S07-COMB-RED-X-BLACK-X",
        "S07-COMB-AH-PHASES",
        "S07-COMB-TYPE1-PREREQUISITE",
        "S07-COMB-SPLIT-SHIFT-D",
        "S07-COMB-RELIEF-SPR",
        "S07-COMB-MANUAL-DEVIATION",
        "S07-COMB-STRUCTURE-BLOCK",
    ];

    [Fact]
    public async Task ApplicationCatalogAndExamplesShareCompleteStableScenarioReferences()
    {
        RuleCatalogSnapshot snapshot = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);
        string document = ReadExamplesDocument();

        Assert.Equal(28, snapshot.Definitions.Count);
        Assert.All(
            snapshot.Definitions,
            definition =>
            {
                string scenarioPrefix = $"`S07-{definition.Id}-";
                int expectedCount = definition.Id == "MAX_CONSECUTIVE_WORKDAYS" ? 4 : 3;
                Assert.Equal(expectedCount, CountOccurrences(document, scenarioPrefix));
            });
        Assert.All(
            CombinationScenarioIds,
            scenarioId => Assert.Equal(1, CountOccurrences(document, $"`{scenarioId}`")));
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
