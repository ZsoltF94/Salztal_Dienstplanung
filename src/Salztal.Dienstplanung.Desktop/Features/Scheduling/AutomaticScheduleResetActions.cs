using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal interface IAutomaticScheduleResetActions
{
    public Task<AutomaticScheduleDiscardOutcome> DiscardAsync(
        DiscardAutomaticScheduleRequest request,
        CancellationToken cancellationToken);
}

internal sealed class AutomaticScheduleResetActions : IAutomaticScheduleResetActions
{
    private readonly DiscardAutomaticScheduleCommand _command;

    public AutomaticScheduleResetActions(DiscardAutomaticScheduleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _command = command;
    }

    public async Task<AutomaticScheduleDiscardOutcome> DiscardAsync(
        DiscardAutomaticScheduleRequest request,
        CancellationToken cancellationToken)
    {
        AutomaticScheduleDiscardResult result = await _command.ExecuteAsync(
            request,
            cancellationToken);
        return new AutomaticScheduleDiscardOutcome(result.Status, result.Message);
    }
}

internal sealed record AutomaticScheduleDiscardOutcome(
    AutomaticScheduleDiscardStatus Status,
    string Message);
