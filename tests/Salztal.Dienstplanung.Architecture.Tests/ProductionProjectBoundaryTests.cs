using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Salztal.Dienstplanung.Architecture.Tests;

public sealed class ProductionProjectBoundaryTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> ExpectedReferences =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            ["Salztal.Dienstplanung.Domain"] = CreateReferenceSet(),
            ["Salztal.Dienstplanung.Application"] = CreateReferenceSet("Salztal.Dienstplanung.Domain"),
            ["Salztal.Dienstplanung.Planning"] = CreateReferenceSet(
                "Salztal.Dienstplanung.Application",
                "Salztal.Dienstplanung.Domain"),
            ["Salztal.Dienstplanung.Infrastructure"] = CreateReferenceSet(
                "Salztal.Dienstplanung.Application",
                "Salztal.Dienstplanung.Domain"),
            ["Salztal.Dienstplanung.Excel"] = CreateReferenceSet("Salztal.Dienstplanung.Application"),
            ["Salztal.Dienstplanung.Desktop"] = CreateReferenceSet(
                "Salztal.Dienstplanung.Application",
                "Salztal.Dienstplanung.Excel",
                "Salztal.Dienstplanung.Infrastructure",
                "Salztal.Dienstplanung.Planning"),
        };

    [Fact]
    public void ProductionProjectsWhenLoadedMatchApprovedModuleSet()
    {
        Assert.Equal(
            ExpectedReferences.Keys.Order(StringComparer.Ordinal),
            RepositoryLayout.ProductionProjects.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ProductionProjectReferencesWhenLoadedMatchApprovedDirections()
    {
        foreach ((string projectName, IReadOnlySet<string> expectedReferences) in ExpectedReferences)
        {
            ProjectDescriptor project = RepositoryLayout.ProductionProjects[projectName];

            Assert.Equal(
                expectedReferences.Order(StringComparer.Ordinal),
                project.ProjectReferences.Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void ProductionProjectGraphWhenTraversedContainsNoCycle()
    {
        HashSet<string> visitedProjects = new(StringComparer.Ordinal);
        HashSet<string> projectsBeingVisited = new(StringComparer.Ordinal);

        foreach (string projectName in RepositoryLayout.ProductionProjects.Keys)
        {
            VisitProject(projectName, visitedProjects, projectsBeingVisited);
        }
    }

    [Fact]
    public void CompiledProductionAssembliesWhenInspectedReferenceOnlyApprovedModules()
    {
        HashSet<string> productionProjectNames = RepositoryLayout.ProductionProjects.Keys
            .ToHashSet(StringComparer.Ordinal);

        foreach ((string projectName, IReadOnlySet<string> allowedReferences) in ExpectedReferences)
        {
            Assembly assembly = RepositoryLayout.LoadProductionAssembly(projectName);
            IEnumerable<string> productionAssemblyReferences = assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null && productionProjectNames.Contains(name))
                .Select(name => name!);

            foreach (string assemblyReference in productionAssemblyReferences)
            {
                Assert.Contains(assemblyReference, allowedReferences);
            }
        }
    }

    private static HashSet<string> CreateReferenceSet(params string[] projectNames)
    {
        return projectNames.ToHashSet(StringComparer.Ordinal);
    }

    private static void VisitProject(
        string projectName,
        ISet<string> visitedProjects,
        ISet<string> projectsBeingVisited)
    {
        if (visitedProjects.Contains(projectName))
        {
            return;
        }

        Assert.True(
            projectsBeingVisited.Add(projectName),
            $"Zirkuläre Produktionsprojektreferenz bei {projectName} erkannt.");

        foreach (string referencedProject in RepositoryLayout.ProductionProjects[projectName].ProjectReferences)
        {
            VisitProject(referencedProject, visitedProjects, projectsBeingVisited);
        }

        projectsBeingVisited.Remove(projectName);
        visitedProjects.Add(projectName);
    }
}
