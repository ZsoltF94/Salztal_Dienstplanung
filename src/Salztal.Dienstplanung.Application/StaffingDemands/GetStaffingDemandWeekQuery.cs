using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class GetStaffingDemandWeekQuery
{
    private readonly IStaffingDemandReader _reader;

    public GetStaffingDemandWeekQuery(IStaffingDemandReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<StaffingDemandWeekQueryResult> ExecuteAsync(
        DateOnly weekMonday,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StaffingDemandReadData data = await _reader.LoadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        StandardStaffingDemandRevisionSetValidationResult standardResult =
            StandardStaffingDemandRevisionSet.Create(data.StandardRevisions);
        if (!standardResult.IsSuccess)
        {
            return StaffingDemandWeekQueryResult.Failure(
                StaffingDemandWeekQueryStatus.StoredDataInvalid,
                standardResult.Errors.Select(
                    StaffingDemandWeekQueryErrors.FromStandardRevision));
        }

        StaffingDemandDateExceptionSetValidationResult exceptionResult =
            StaffingDemandDateExceptionSet.Create(data.DateExceptions);
        if (!exceptionResult.IsSuccess)
        {
            return StaffingDemandWeekQueryResult.Failure(
                StaffingDemandWeekQueryStatus.StoredDataInvalid,
                exceptionResult.Errors.Select(
                    StaffingDemandWeekQueryErrors.FromDateException));
        }

        StaffingDemandCatalogValidationResult catalogResult =
            StaffingDemandCatalogValidator.Validate(data);
        if (catalogResult.Errors.Count > 0)
        {
            return StaffingDemandWeekQueryResult.Failure(
                StaffingDemandWeekQueryStatus.CatalogInvalid,
                catalogResult.Errors);
        }

        StaffingDemandWeekResolutionResult weekResult = StaffingDemandWeek.Resolve(
            weekMonday,
            standardResult.Value!,
            exceptionResult.Value!);
        if (!weekResult.IsSuccess)
        {
            return StaffingDemandWeekQueryErrors.FromWeekResolution(weekResult.Errors);
        }

        return StaffingDemandWeekQueryResult.Success(
            new StaffingDemandWeekSnapshotProjector(catalogResult).Create(
                weekResult.Value!,
                standardResult.Value!,
                exceptionResult.Value!));
    }
}
