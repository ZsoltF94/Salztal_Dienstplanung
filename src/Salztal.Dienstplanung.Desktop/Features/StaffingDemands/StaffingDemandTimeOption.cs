using System.Globalization;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed record StaffingDemandTimeOption(TimeOnly Value)
{
    public string DisplayName => Value.ToString("HH:mm", CultureInfo.InvariantCulture);
}
