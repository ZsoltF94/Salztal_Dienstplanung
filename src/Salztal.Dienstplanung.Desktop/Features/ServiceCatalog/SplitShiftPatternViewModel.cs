namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed record SplitShiftPatternViewModel(
    string DisplayCode,
    string WorkLocationName,
    string FirstShiftTypeName,
    string SecondShiftTypeName,
    string StandardBreakText,
    string StandardWorkText);
