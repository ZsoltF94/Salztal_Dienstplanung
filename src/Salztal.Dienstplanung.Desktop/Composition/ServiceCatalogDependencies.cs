using Salztal.Dienstplanung.Application.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop.Composition;

internal sealed record ServiceCatalogDependencies(
    IServiceCatalogReader Reader,
    IWorkLocationUpdateStore WorkLocationUpdateStore,
    IShiftTypeStandardTimeUpdateStore ShiftTypeStandardTimeUpdateStore);
