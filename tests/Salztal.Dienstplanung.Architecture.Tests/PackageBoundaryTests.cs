using System.Linq;

namespace Salztal.Dienstplanung.Architecture.Tests;

public sealed class PackageBoundaryTests
{
    private const string DesktopProject = "Salztal.Dienstplanung.Desktop";
    private const string ExcelProject = "Salztal.Dienstplanung.Excel";
    private const string InfrastructureProject = "Salztal.Dienstplanung.Infrastructure";
    private const string PlanningProject = "Salztal.Dienstplanung.Planning";

    [Fact]
    public void DirectPackageReferencesWhenLoadedStayInResponsibleProjects()
    {
        foreach (ProjectDescriptor project in RepositoryLayout.ProductionProjects.Values)
        {
            foreach (string packageReference in project.PackageReferences)
            {
                string? responsibleProject = GetResponsibleProductionProject(packageReference);

                if (responsibleProject is not null)
                {
                    Assert.Equal(responsibleProject, project.Name);
                }

                Assert.False(
                    IsTestPackage(packageReference),
                    $"Testpaket {packageReference} ist im Produktionsprojekt {project.Name} referenziert.");
            }
        }

        foreach (ProjectDescriptor project in RepositoryLayout.TestProjects.Values)
        {
            foreach (string packageReference in project.PackageReferences)
            {
                Assert.Null(GetResponsibleProductionProject(packageReference));
            }
        }
    }

    [Theory]
    [InlineData("CommunityToolkit.Mvvm", DesktopProject)]
    [InlineData("Microsoft.EntityFrameworkCore.Design", InfrastructureProject)]
    [InlineData("Microsoft.EntityFrameworkCore.Sqlite", InfrastructureProject)]
    [InlineData("Google.OrTools", PlanningProject)]
    public void RequiredProductionPackageWhenLoadedExistsOnlyInResponsibleProject(
        string packageName,
        string responsibleProject)
    {
        string[] projectsWithPackage = RepositoryLayout.ProductionProjects.Values
            .Where(project => project.PackageReferences.Contains(packageName))
            .Select(project => project.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([responsibleProject], projectsWithPackage);
    }

    [Fact]
    public void XunitPackageWhenLoadedIsReferencedByAllAndOnlyTestProjects()
    {
        string[] projectsWithXunit = RepositoryLayout.TestProjects.Values
            .Where(project => project.PackageReferences.Contains("xunit.v3"))
            .Select(project => project.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            RepositoryLayout.TestProjects.Keys.Order(StringComparer.Ordinal),
            projectsWithXunit);
    }

    private static string? GetResponsibleProductionProject(string packageName)
    {
        if (packageName.Equals("CommunityToolkit.Mvvm", StringComparison.OrdinalIgnoreCase))
        {
            return DesktopProject;
        }

        if (packageName.StartsWith("Google.OrTools", StringComparison.OrdinalIgnoreCase))
        {
            return PlanningProject;
        }

        if (packageName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
            || packageName.StartsWith("Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase)
            || packageName.StartsWith("SQLitePCLRaw", StringComparison.OrdinalIgnoreCase))
        {
            return InfrastructureProject;
        }

        if (packageName.Equals("ClosedXML", StringComparison.OrdinalIgnoreCase)
            || packageName.StartsWith("DocumentFormat.OpenXml", StringComparison.OrdinalIgnoreCase))
        {
            return ExcelProject;
        }

        return null;
    }

    private static bool IsTestPackage(string packageName)
    {
        return packageName.StartsWith("xunit", StringComparison.OrdinalIgnoreCase)
            || packageName.StartsWith("Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase)
            || packageName.StartsWith("Microsoft.Testing.Platform", StringComparison.OrdinalIgnoreCase);
    }
}
