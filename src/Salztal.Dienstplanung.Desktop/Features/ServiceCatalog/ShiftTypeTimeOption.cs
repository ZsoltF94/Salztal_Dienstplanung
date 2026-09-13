using System.Globalization;

namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed record ShiftTypeTimeOption(TimeOnly Value)
{
    public string DisplayName => Value.ToString("HH:mm", CultureInfo.InvariantCulture);
}
