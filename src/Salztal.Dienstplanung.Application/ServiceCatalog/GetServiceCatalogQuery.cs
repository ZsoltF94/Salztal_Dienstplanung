using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class GetServiceCatalogQuery
{
    private readonly IServiceCatalogReader _reader;

    public GetServiceCatalogQuery(IServiceCatalogReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<ServiceCatalogSnapshot> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ServiceCatalogData catalog = await _reader.LoadAsync(cancellationToken);

        return new ServiceCatalogSnapshot(
            catalog.WorkLocations.Select(CreateWorkLocationSnapshot),
            catalog.ShiftTypes.Select(CreateShiftTypeSnapshot),
            CreateSplitShiftPatternSnapshot(catalog.SplitShiftPattern),
            CreateReliefShiftPatternSnapshot(catalog.ReliefShiftPattern));
    }

    private static WorkLocationSnapshot CreateWorkLocationSnapshot(WorkLocation workLocation)
    {
        return new WorkLocationSnapshot(
            workLocation.Id.Value,
            workLocation.Name.Value,
            workLocation.Color.Code);
    }

    private static ShiftTypeSnapshot CreateShiftTypeSnapshot(ShiftType shiftType)
    {
        return new ShiftTypeSnapshot(
            shiftType.Id.Value,
            shiftType.Name.Value,
            shiftType.WorkLocationId.Value,
            shiftType.Display.Abbreviation,
            shiftType.Display.Kind == ShiftTypeDisplayKind.ActualTime,
            shiftType.StandardTime.Start,
            shiftType.StandardTime.End,
            shiftType.StandardTime.DurationMinutes);
    }

    private static SplitShiftPatternSnapshot CreateSplitShiftPatternSnapshot(
        SplitShiftPattern pattern)
    {
        return new SplitShiftPatternSnapshot(
            pattern.Id.Value,
            pattern.DisplayCode,
            pattern.FirstShiftTypeId.Value,
            pattern.SecondShiftTypeId.Value,
            pattern.WorkLocationId.Value,
            pattern.StandardBreakMinutes,
            pattern.StandardWorkMinutes);
    }

    private static ReliefShiftPatternSnapshot CreateReliefShiftPatternSnapshot(
        ReliefShiftPattern pattern)
    {
        return new ReliefShiftPatternSnapshot(
            pattern.Id.Value,
            pattern.DisplayCode,
            pattern.DisplayColorCode,
            pattern.AllowedDay,
            pattern.FirstShiftTypeId.Value,
            pattern.FirstWorkLocationId.Value,
            pattern.SecondShiftTypeId.Value,
            pattern.SecondWorkLocationId.Value,
            pattern.SwitchRule == ReliefShiftSwitchRule.EndOfFirstActualDemand,
            pattern.HasInterruption);
    }
}
