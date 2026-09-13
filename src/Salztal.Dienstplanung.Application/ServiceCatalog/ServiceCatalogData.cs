using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class ServiceCatalogData
{
    public ServiceCatalogData(
        IEnumerable<WorkLocation> workLocations,
        IEnumerable<ShiftType> shiftTypes,
        SplitShiftPattern splitShiftPattern,
        ReliefShiftPattern reliefShiftPattern)
    {
        ArgumentNullException.ThrowIfNull(workLocations);
        ArgumentNullException.ThrowIfNull(shiftTypes);
        ArgumentNullException.ThrowIfNull(splitShiftPattern);
        ArgumentNullException.ThrowIfNull(reliefShiftPattern);

        WorkLocations = Array.AsReadOnly(workLocations.ToArray());
        ShiftTypes = Array.AsReadOnly(shiftTypes.ToArray());
        SplitShiftPattern = splitShiftPattern;
        ReliefShiftPattern = reliefShiftPattern;
    }

    public ReadOnlyCollection<WorkLocation> WorkLocations { get; }

    public ReadOnlyCollection<ShiftType> ShiftTypes { get; }

    public SplitShiftPattern SplitShiftPattern { get; }

    public ReliefShiftPattern ReliefShiftPattern { get; }
}
