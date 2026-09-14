using System.Globalization;
using Salztal.Dienstplanung.Application.StaffingDemands;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StaffingDemandItemViewModel
{
    public StaffingDemandItemViewModel(StaffingDemandItemSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        SourceId = snapshot.SourceId;
        SourceKind = snapshot.SourceKind;
        SourceDisplay = snapshot.SourceDisplay;
        Date = snapshot.Date;
        WorkLocationId = snapshot.WorkLocationId;
        WorkLocationName = snapshot.WorkLocationName;
        WorkLocationColorCode = snapshot.WorkLocationColorCode;
        ShiftTypeId = snapshot.ShiftTypeId;
        ShiftTypeName = snapshot.ShiftTypeName;
        ShiftTypeAbbreviation = snapshot.ShiftTypeAbbreviation;
        ShiftTypeUsesActualTimeAsDisplay = snapshot.ShiftTypeUsesActualTimeAsDisplay;
        ActualStart = snapshot.ActualStart;
        ActualEnd = snapshot.ActualEnd;
        RequiredEmployeeCount = snapshot.RequiredEmployeeCount;
        DurationMinutes = snapshot.DurationMinutes;
        RequiredWorkMinutes = snapshot.RequiredWorkMinutes;
    }

    public Guid SourceId { get; }

    public StaffingDemandSourceSnapshotKind SourceKind { get; }

    public string SourceDisplay { get; }

    public DateOnly Date { get; }

    public Guid WorkLocationId { get; }

    public string WorkLocationName { get; }

    public string WorkLocationColorCode { get; }

    public Guid ShiftTypeId { get; }

    public string ShiftTypeName { get; }

    public string? ShiftTypeAbbreviation { get; }

    public bool ShiftTypeUsesActualTimeAsDisplay { get; }

    public TimeOnly ActualStart { get; }

    public TimeOnly ActualEnd { get; }

    public int RequiredEmployeeCount { get; }

    public int DurationMinutes { get; }

    public long RequiredWorkMinutes { get; }

    public string ActualTimeDisplay => string.Create(
        CultureInfo.InvariantCulture,
        $"{ActualStart:HH\\:mm}–{ActualEnd:HH\\:mm} Uhr");

    public string RequiredEmployeeCountDisplay => RequiredEmployeeCount == 1
        ? "1 Person"
        : $"{RequiredEmployeeCount} Personen";

    public string DurationDisplay => StaffingDemandDisplayFormatter.FormatMinutes(
        DurationMinutes);

    public string RequiredWorkDisplay => StaffingDemandDisplayFormatter.FormatMinutes(
        RequiredWorkMinutes);

    public bool IsDateException => SourceKind ==
        StaffingDemandSourceSnapshotKind.DateException;
}
