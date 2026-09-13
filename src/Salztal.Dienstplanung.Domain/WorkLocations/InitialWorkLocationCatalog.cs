using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.WorkLocations;

public static class InitialWorkLocationCatalog
{
    private static readonly ReadOnlyCollection<WorkLocation> InitialLocations = Array.AsReadOnly(
        new[]
        {
            CreateInitial(
                new Guid("6d38f48b-f938-4aaa-82bd-c59372f599f4"),
                "Cafeteria",
                "yellow"),
            CreateInitial(
                new Guid("2d2bf3b2-4744-4b56-8515-f33adf9f2161"),
                "Restaurant",
                "red"),
        });

    public static WorkLocation Cafeteria => InitialLocations[0];

    public static WorkLocation Restaurant => InitialLocations[1];

    public static IReadOnlyList<WorkLocation> All => InitialLocations;

    private static WorkLocation CreateInitial(Guid id, string name, string colorCode)
    {
        WorkLocationValidationResult result = WorkLocation.Create(id, name, colorCode);

        return result.Value
            ?? throw new InvalidOperationException("An initial work location is invalid.");
    }
}
