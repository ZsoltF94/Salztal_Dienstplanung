using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Salztal.Dienstplanung.Architecture.Tests;

internal sealed record ProjectDescriptor(
    string Name,
    string FilePath,
    IReadOnlySet<string> ProjectReferences,
    IReadOnlySet<string> PackageReferences);

internal static class RepositoryLayout
{
    private const string SolutionFileName = "Salztal.Dienstplanung.sln";

    private static readonly Lazy<DirectoryInfo> RepositoryRoot = new(FindRepositoryRoot);
    private static readonly Lazy<IReadOnlyDictionary<string, ProjectDescriptor>> ProductionProjectCache =
        new(() => LoadProjects(Path.Combine(Root.FullName, "src")));
    private static readonly Lazy<IReadOnlyDictionary<string, ProjectDescriptor>> TestProjectCache =
        new(() => LoadProjects(Path.Combine(Root.FullName, "tests")));

    internal static DirectoryInfo Root => RepositoryRoot.Value;

    internal static IReadOnlyDictionary<string, ProjectDescriptor> ProductionProjects =>
        ProductionProjectCache.Value;

    internal static IReadOnlyDictionary<string, ProjectDescriptor> TestProjects => TestProjectCache.Value;

    internal static Assembly LoadProductionAssembly(string projectName)
    {
        return Assembly.Load(new AssemblyName(projectName));
    }

    internal static string GetRepositoryRelativePath(string path)
    {
        return Path.GetRelativePath(Root.FullName, path);
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

        throw new InvalidOperationException("Der Repository-Stamm mit der erwarteten Solution wurde nicht gefunden.");
    }

    private static Dictionary<string, ProjectDescriptor> LoadProjects(string directoryPath)
    {
        return Directory
            .EnumerateFiles(directoryPath, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsBuildArtifactPath(path))
            .Select(LoadProject)
            .ToDictionary(project => project.Name, StringComparer.Ordinal);
    }

    private static ProjectDescriptor LoadProject(string projectFilePath)
    {
        XDocument projectDocument = XDocument.Load(projectFilePath);
        string projectName = Path.GetFileNameWithoutExtension(projectFilePath);

        HashSet<string> projectReferences = projectDocument
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFileNameWithoutExtension(value!))
            .ToHashSet(StringComparer.Ordinal);

        HashSet<string> packageReferences = projectDocument
            .Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new ProjectDescriptor(projectName, projectFilePath, projectReferences, packageReferences);
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
