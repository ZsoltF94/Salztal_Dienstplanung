using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed record AutomaticScheduleReportSource
{
    public AutomaticScheduleReportSource(
        string identity,
        string sourceDisplay,
        AutomaticScheduleGenerationReport report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDisplay);
        ArgumentNullException.ThrowIfNull(report);
        Identity = identity.Trim();
        SourceDisplay = sourceDisplay.Trim();
        Report = report;
    }

    public string Identity { get; }

    public string SourceDisplay { get; }

    public AutomaticScheduleGenerationReport Report { get; }
}

internal interface IAutomaticScheduleReportPresenter
{
    public void SetSource(
        AutomaticScheduleReportSource? source,
        string unavailableMessage);

    public void ShowCurrent();
}

internal sealed class NullAutomaticScheduleReportPresenter
    : IAutomaticScheduleReportPresenter
{
    public static NullAutomaticScheduleReportPresenter Instance { get; } = new();

    private NullAutomaticScheduleReportPresenter()
    {
    }

    public void SetSource(
        AutomaticScheduleReportSource? source,
        string unavailableMessage)
    {
    }

    public void ShowCurrent()
    {
    }
}
