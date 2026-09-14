using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Salztal.Dienstplanung.Architecture.Tests;

public sealed class DesktopCompositionBoundaryTests
{
    private const string CompositionNamespace = "Salztal.Dienstplanung.Desktop.Composition";

    private static readonly string[] ForbiddenTechnicalNamespaces =
    [
        "Salztal.Dienstplanung.Excel",
        "Salztal.Dienstplanung.Infrastructure",
        "Salztal.Dienstplanung.Planning",
    ];

    [Fact]
    public void DesktopSourcesOutsideCompositionWhenScannedDoNotUseTechnicalModules()
    {
        string desktopDirectory = Path.Combine(
            RepositoryLayout.Root.FullName,
            "src",
            "Salztal.Dienstplanung.Desktop");

        List<string> violations = [];

        foreach (string sourceFile in EnumerateSourceFiles(desktopDirectory, "*.cs"))
        {
            string[] lines = File.ReadAllLines(sourceFile);
            string? declaredNamespace = GetDeclaredNamespace(lines);

            if (IsCompositionNamespace(declaredNamespace))
            {
                continue;
            }

            foreach (string line in lines.Where(IsRelevantCodeLine))
            {
                string? forbiddenNamespace = ForbiddenTechnicalNamespaces.FirstOrDefault(
                    candidate => line.Contains(candidate, StringComparison.Ordinal));

                if (forbiddenNamespace is not null)
                {
                    violations.Add(
                        $"{RepositoryLayout.GetRepositoryRelativePath(sourceFile)} verwendet {forbiddenNamespace}.");
                }
            }
        }

        foreach (string xamlFile in EnumerateSourceFiles(desktopDirectory, "*.xaml"))
        {
            string content = File.ReadAllText(xamlFile);
            string? forbiddenNamespace = ForbiddenTechnicalNamespaces.FirstOrDefault(
                candidate => content.Contains(candidate, StringComparison.Ordinal));

            if (forbiddenNamespace is not null)
            {
                violations.Add(
                    $"{RepositoryLayout.GetRepositoryRelativePath(xamlFile)} verwendet {forbiddenNamespace}.");
            }
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void DesktopFeatureSourcesWhenScannedDoNotReferenceOtherFeatureViewModels()
    {
        string featureDirectory = Path.Combine(
            RepositoryLayout.Root.FullName,
            "src",
            "Salztal.Dienstplanung.Desktop",
            "Features");
        string employeeDirectory = Path.Combine(featureDirectory, "Employees");
        string serviceCatalogDirectory = Path.Combine(featureDirectory, "ServiceCatalog");

        AssertFeatureDoesNotReference(
            employeeDirectory,
            "Salztal.Dienstplanung.Desktop.Features.ServiceCatalog");
        AssertFeatureDoesNotReference(
            serviceCatalogDirectory,
            "Salztal.Dienstplanung.Desktop.Features.Employees");
    }

    private static void AssertFeatureDoesNotReference(
        string featureDirectory,
        string forbiddenNamespace)
    {
        string[] violations = EnumerateSourceFiles(featureDirectory, "*.cs")
            .Where(sourceFile => File.ReadAllText(sourceFile).Contains(
                forbiddenNamespace,
                StringComparison.Ordinal))
            .Select(RepositoryLayout.GetRepositoryRelativePath)
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<string> EnumerateSourceFiles(string directory, string searchPattern)
    {
        return Directory
            .EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories)
            .Where(path => !IsBuildArtifactPath(path));
    }

    private static string? GetDeclaredNamespace(IEnumerable<string> lines)
    {
        const string namespaceKeyword = "namespace ";

        string? namespaceLine = lines
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith(namespaceKeyword, StringComparison.Ordinal));

        if (namespaceLine is null)
        {
            return null;
        }

        return namespaceLine[namespaceKeyword.Length..].TrimEnd(';', '{', ' ');
    }

    private static bool IsCompositionNamespace(string? namespaceName)
    {
        return namespaceName is not null
            && (namespaceName.Equals(CompositionNamespace, StringComparison.Ordinal)
                || namespaceName.StartsWith($"{CompositionNamespace}.", StringComparison.Ordinal));
    }

    private static bool IsRelevantCodeLine(string line)
    {
        string trimmedLine = line.TrimStart();
        return !trimmedLine.StartsWith("//", StringComparison.Ordinal);
    }

    private static bool IsBuildArtifactPath(string path)
    {
        string normalizedPath = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        string binSegment = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
        string objSegment = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";

        return normalizedPath.Contains(binSegment, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.Contains(objSegment, StringComparison.OrdinalIgnoreCase);
    }
}
