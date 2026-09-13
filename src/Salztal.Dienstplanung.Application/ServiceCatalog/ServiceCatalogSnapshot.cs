using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class ServiceCatalogSnapshot
{
    internal ServiceCatalogSnapshot(
        IEnumerable<WorkLocationSnapshot> workLocations,
        IEnumerable<ShiftTypeSnapshot> shiftTypes,
        SplitShiftPatternSnapshot splitShiftPattern,
        ReliefShiftPatternSnapshot reliefShiftPattern)
    {
        WorkLocations = Array.AsReadOnly(workLocations.ToArray());
        ShiftTypes = Array.AsReadOnly(shiftTypes.ToArray());
        SplitShiftPattern = splitShiftPattern;
        ReliefShiftPattern = reliefShiftPattern;
    }

    public ReadOnlyCollection<WorkLocationSnapshot> WorkLocations { get; }

    public ReadOnlyCollection<ShiftTypeSnapshot> ShiftTypes { get; }

    public SplitShiftPatternSnapshot SplitShiftPattern { get; }

    public ReliefShiftPatternSnapshot ReliefShiftPattern { get; }
}
