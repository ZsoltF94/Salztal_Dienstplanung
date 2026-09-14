using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class RemoveStaffingDemandDateExceptionCommand
{
    private readonly IStaffingDemandReader _reader;
    private readonly IRemoveStaffingDemandDateExceptionStore _store;

    public RemoveStaffingDemandDateExceptionCommand(
        IStaffingDemandReader reader,
        IRemoveStaffingDemandDateExceptionStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<StaffingDemandCommandResult> ExecuteAsync(
        RemoveStaffingDemandDateExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ExpectedExceptionId == Guid.Empty)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                [new StaffingDemandCommandError(
                    StaffingDemandCommandErrorCode.ExpectedIdentifierRequired,
                    "Die erwartete Ausnahmekennung ist ungültig.")]);
        }

        StaffingDemandDateExceptionValidationResult keyResult =
            StaffingDemandDateException.CreateRemoval(
                request.ExpectedExceptionId,
                request.Date,
                request.WorkLocationId,
                request.ShiftTypeId);
        if (!keyResult.IsSuccess)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                keyResult.Errors.Select(
                    StaffingDemandCommandErrors.FromDateExceptionValidation));
        }

        StaffingDemandDateException expectedKey = keyResult.Value!;
        StaffingDemandReadData data = await _reader.LoadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        StaffingDemandCommandContextValidationResult contextResult =
            StaffingDemandCommandContext.Create(
                data,
                expectedKey.Key.WorkLocationId,
                expectedKey.Key.ShiftTypeId);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        StaffingDemandDateException? current = data.DateExceptions
            .SingleOrDefault(candidate => candidate.Key == expectedKey.Key);
        if (current is null)
        {
            return StaffingDemandCommandResult.Success();
        }

        if (current.Id.Value != request.ExpectedExceptionId)
        {
            return StaffingDemandCommandErrors.Conflict();
        }

        StaffingDemandWriteStoreResult storeResult = await _store.RemoveAsync(
            current,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return storeResult == StaffingDemandWriteStoreResult.NotFound
            ? StaffingDemandCommandResult.Success()
            : StaffingDemandCommandErrors.FromStoreResult(storeResult);
    }
}
