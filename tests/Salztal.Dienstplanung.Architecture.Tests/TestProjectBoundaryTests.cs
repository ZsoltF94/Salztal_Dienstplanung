using System.Collections.Generic;
using System.Linq;

namespace Salztal.Dienstplanung.Architecture.Tests;

public sealed class TestProjectBoundaryTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> ExpectedReferences =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            ["Salztal.Dienstplanung.Domain.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Domain"),
            ["Salztal.Dienstplanung.Application.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Application"),
            ["Salztal.Dienstplanung.Planning.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Planning"),
            ["Salztal.Dienstplanung.Infrastructure.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Infrastructure"),
            ["Salztal.Dienstplanung.Excel.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Excel"),
            ["Salztal.Dienstplanung.Desktop.Tests"] = CreateReferenceSet("Salztal.Dienstplanung.Desktop"),
            ["Salztal.Dienstplanung.Architecture.Tests"] = CreateReferenceSet(
                "Salztal.Dienstplanung.Application",
                "Salztal.Dienstplanung.Desktop",
                "Salztal.Dienstplanung.Domain",
                "Salztal.Dienstplanung.Excel",
                "Salztal.Dienstplanung.Infrastructure",
                "Salztal.Dienstplanung.Planning"),
        };

    [Fact]
    public void TestProjectsWhenLoadedMatchApprovedTestStructure()
    {
        Assert.Equal(
            ExpectedReferences.Keys.Order(StringComparer.Ordinal),
            RepositoryLayout.TestProjects.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TestProjectReferencesWhenLoadedMatchModuleBoundaries()
    {
        foreach ((string projectName, IReadOnlySet<string> expectedReferences) in ExpectedReferences)
        {
            ProjectDescriptor project = RepositoryLayout.TestProjects[projectName];

            Assert.Equal(
                expectedReferences.Order(StringComparer.Ordinal),
                project.ProjectReferences.Order(StringComparer.Ordinal));
        }
    }

    private static HashSet<string> CreateReferenceSet(params string[] projectNames)
    {
        return projectNames.ToHashSet(StringComparer.Ordinal);
    }
}
