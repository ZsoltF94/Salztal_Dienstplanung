using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class RuleCatalogExampleScenarioTests
{
    private const string SolutionFileName = "Salztal.Dienstplanung.sln";
    private const string ExamplesRelativePath = "docs/decisions/S07_RULE_CATALOG_EXAMPLES.md";
    private const string VersionTwoExamplesRelativePath =
        "docs/decisions/S09_RULE_CATALOG_V2_EXAMPLES.md";

    private static readonly string[] RequiredOutcomes =
    [
        "SATISFIED",
        "VIOLATED",
        "NOT_APPLICABLE",
    ];

    [Fact]
    public void ExamplesDocumentReferencesEveryCatalogRuleAndRequiredOutcomeExactlyOnce()
    {
        RuleCatalogReadResult result = InitialRuleCatalog.Read(
            InitialRuleCatalog.VersionOne);
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

    [Fact]
    public void VersionTwoExamplesCoverEveryCurrentRuleThroughVersionedInheritance()
    {
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        string versionOneDocument = ReadDocument(ExamplesRelativePath);
        string versionTwoDocument = ReadDocument(VersionTwoExamplesRelativePath);
        HashSet<string> versionTwoOnlyRuleIds =
        [
            CurrentSoftRuleDefinitions.MinimizeReliefShifts.Id.Value,
            CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum.Id.Value,
            CurrentSoftRuleDefinitions.RelativeWeeklyTarget.Id.Value,
        ];

        foreach (RuleDefinition definition in catalog.Definitions)
        {
            string document = versionTwoOnlyRuleIds.Contains(definition.Id.Value)
                ? versionTwoDocument
                : versionOneDocument;
            string prefix = versionTwoOnlyRuleIds.Contains(definition.Id.Value)
                ? "S09"
                : "S07";
            foreach (string outcome in RequiredOutcomes)
            {
                string scenarioId = $"`{prefix}-{definition.Id.Value}-{outcome}`";
                Assert.Equal(1, CountOccurrences(document, scenarioId));
            }
        }

        Assert.DoesNotContain("`S09-AH_WEEKLY_TARGET-", versionTwoDocument);
    }

    private static string ReadExamplesDocument()
    {
        return ReadDocument(ExamplesRelativePath);
    }

    private static string ReadDocument(string relativePath)
    {
        DirectoryInfo repositoryRoot = FindRepositoryRoot();
        string documentPath = Path.Combine(
            repositoryRoot.FullName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

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
