using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

internal sealed class RecordingAutomaticScheduleResetActions
    : IAutomaticScheduleResetActions
{
    public AutomaticScheduleDiscardOutcome Outcome { get; set; } = new(
        AutomaticScheduleDiscardStatus.Succeeded,
        "Der automatische Plan wurde vollst\u00e4ndig verworfen.");

    public Exception? Exception { get; set; }

    public int CallCount { get; private set; }

    public DiscardAutomaticScheduleRequest? Request { get; private set; }

    public Task<AutomaticScheduleDiscardOutcome> DiscardAsync(
        DiscardAutomaticScheduleRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        Request = request;
        return Exception is null
            ? Task.FromResult(Outcome)
            : Task.FromException<AutomaticScheduleDiscardOutcome>(Exception);
    }
}

internal static class AutomaticScheduleResetTestData
{
    public static AutomaticScheduleGenerationContext AcceptedContext(
        int assignments = 7,
        int dayOffs = 3) =>
        AutomaticScheduleGenerationTestData.ReadyContext() with
        {
            AcceptedAutomaticSchedule = new AcceptedAutomaticScheduleSnapshot(
                assignments,
                dayOffs),
        };
}
