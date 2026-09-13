namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed record ReliefShiftPatternViewModel(
    string DisplayCode,
    string DisplayColorCode,
    string DisplayColorName,
    string AllowedDayName,
    string FirstShiftTypeName,
    string FirstWorkLocationName,
    string SecondShiftTypeName,
    string SecondWorkLocationName,
    string SwitchRuleText,
    string InterruptionText)
{
    public string Code => DisplayColorCode;
}
