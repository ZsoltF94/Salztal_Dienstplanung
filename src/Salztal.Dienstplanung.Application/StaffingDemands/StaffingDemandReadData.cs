using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class StaffingDemandReadData
{
    public StaffingDemandReadData(
        IEnumerable<StandardStaffingDemandRevision> standardRevisions,
        IEnumerable<StaffingDemandDateException> dateExceptions,
        ServiceCatalogData serviceCatalog)
    {
        ArgumentNullException.ThrowIfNull(standardRevisions);
        ArgumentNullException.ThrowIfNull(dateExceptions);
        ArgumentNullException.ThrowIfNull(serviceCatalog);

        StandardRevisions = Array.AsReadOnly(standardRevisions.ToArray());
        DateExceptions = Array.AsReadOnly(dateExceptions.ToArray());
        ServiceCatalog = serviceCatalog;
    }

    public ReadOnlyCollection<StandardStaffingDemandRevision> StandardRevisions { get; }

    public ReadOnlyCollection<StaffingDemandDateException> DateExceptions { get; }

    public ServiceCatalogData ServiceCatalog { get; }
}
