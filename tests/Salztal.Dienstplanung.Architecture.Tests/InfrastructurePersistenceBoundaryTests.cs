using System.IO;
using System.Reflection;

namespace Salztal.Dienstplanung.Architecture.Tests;

public sealed class InfrastructurePersistenceBoundaryTests
{
    private const string InfrastructureProject = "Salztal.Dienstplanung.Infrastructure";
    private const string ExpectedDbContext =
        "Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.ServiceCatalogDbContext";
    private const string ExpectedMigrationDirectory =
        "src/Salztal.Dienstplanung.Infrastructure/Persistence/ServiceCatalog/Migrations";

    [Fact]
    public void InfrastructureAssemblyWhenInspectedContainsOneSharedDbContext()
    {
        Assembly assembly = RepositoryLayout.LoadProductionAssembly(InfrastructureProject);
        string[] dbContextTypes = assembly
            .GetTypes()
            .Where(IsDbContext)
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([ExpectedDbContext], dbContextTypes);
    }

    [Fact]
    public void InfrastructureSourcesWhenInspectedContainOneMigrationDirectory()
    {
        string infrastructureDirectory = Path.Combine(
            RepositoryLayout.Root.FullName,
            "src",
            InfrastructureProject);
        string[] migrationDirectories = Directory
            .EnumerateDirectories(infrastructureDirectory, "Migrations", SearchOption.AllDirectories)
            .Select(RepositoryLayout.GetRepositoryRelativePath)
            .Select(path => path.Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([ExpectedMigrationDirectory], migrationDirectories);
    }

    private static bool IsDbContext(Type type)
    {
        for (Type? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.FullName == "Microsoft.EntityFrameworkCore.DbContext")
            {
                return true;
            }
        }

        return false;
    }
}
