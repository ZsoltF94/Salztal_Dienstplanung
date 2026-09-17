namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal interface IScheduleFeedbackDelay
{
    public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken);
}

internal sealed class ScheduleFeedbackDelay : IScheduleFeedbackDelay
{
    public Task DelayAsync(
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        return Task.Delay(duration, cancellationToken);
    }
}
