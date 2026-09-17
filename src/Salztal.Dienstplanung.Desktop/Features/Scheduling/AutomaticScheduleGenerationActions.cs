using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal interface IAutomaticScheduleGenerationActions
{
    public Task<AutomaticScheduleGenerationOutcome> GenerateAsync(
        GenerateAutomaticScheduleRequest request,
        CancellationToken cancellationToken);

    public Task<AutomaticScheduleAcceptanceOutcome> AcceptAsync(
        AcceptAutomaticScheduleProposalRequest request,
        CancellationToken cancellationToken);

    public bool DiscardPreview();
}

internal sealed class AutomaticScheduleGenerationActions
    : IAutomaticScheduleGenerationActions
{
    private readonly GenerateAutomaticScheduleCommand _generateCommand;
    private readonly AcceptAutomaticScheduleProposalCommand _acceptCommand;

    public AutomaticScheduleGenerationActions(
        GenerateAutomaticScheduleCommand generateCommand,
        AcceptAutomaticScheduleProposalCommand acceptCommand)
    {
        ArgumentNullException.ThrowIfNull(generateCommand);
        ArgumentNullException.ThrowIfNull(acceptCommand);
        _generateCommand = generateCommand;
        _acceptCommand = acceptCommand;
    }

    public async Task<AutomaticScheduleGenerationOutcome> GenerateAsync(
        GenerateAutomaticScheduleRequest request,
        CancellationToken cancellationToken)
    {
        AutomaticScheduleGenerationResult result =
            await _generateCommand.ExecuteAsync(request, cancellationToken);
        return new AutomaticScheduleGenerationOutcome(
            result.Status,
            result.Message,
            result.Preview,
            result.PlanningErrors);
    }

    public async Task<AutomaticScheduleAcceptanceOutcome> AcceptAsync(
        AcceptAutomaticScheduleProposalRequest request,
        CancellationToken cancellationToken)
    {
        AutomaticScheduleAcceptanceResult result =
            await _acceptCommand.ExecuteAsync(request, cancellationToken);
        return new AutomaticScheduleAcceptanceOutcome(result.Status, result.Message);
    }

    public bool DiscardPreview()
    {
        return _generateCommand.DiscardPreview();
    }
}

internal sealed record AutomaticScheduleGenerationOutcome(
    AutomaticScheduleGenerationStatus Status,
    string Message,
    AutomaticSchedulePreview? Preview,
    IReadOnlyList<AutomaticScheduleError> PlanningErrors);

internal sealed record AutomaticScheduleAcceptanceOutcome(
    AutomaticScheduleAcceptanceStatus Status,
    string Message);
