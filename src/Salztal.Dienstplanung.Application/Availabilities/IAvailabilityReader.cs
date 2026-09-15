namespace Salztal.Dienstplanung.Application.Availabilities;

public interface IAvailabilityReader
{
    public Task<AvailabilityReadData> LoadAsync(
        DateOnly periodMonday,
        DateOnly periodSunday,
        CancellationToken cancellationToken);
}
