using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class GetAvailabilityPeriodQuery
{
    private const int PeriodDayCount = 21;

    private readonly IAvailabilityReader _reader;

    public GetAvailabilityPeriodQuery(IAvailabilityReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<AvailabilityPeriodQueryResult> ExecuteAsync(
        DateOnly selectedDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DateOnly periodMonday = GetMonday(selectedDate);
        if (periodMonday.DayNumber > DateOnly.MaxValue.DayNumber - (PeriodDayCount - 1))
        {
            return AvailabilityPeriodQueryResult.Failure(
                AvailabilityPeriodQueryStatus.ValidationFailed,
                [AvailabilityPeriodQueryErrors.PeriodDoesNotFit()]);
        }

        DateOnly periodSunday = periodMonday.AddDays(PeriodDayCount - 1);
        AvailabilityReadData data = await _reader.LoadAsync(
            periodMonday,
            periodSunday,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        AvailabilityEntrySetValidationResult entrySetResult =
            AvailabilityEntrySet.Create(data.Entries.Select(item => item.Entry));
        if (!entrySetResult.IsSuccess)
        {
            return AvailabilityPeriodQueryResult.Failure(
                AvailabilityPeriodQueryStatus.StoredDataInvalid,
                entrySetResult.Errors.Select(
                    AvailabilityPeriodQueryErrors.FromDuplicateEntry));
        }

        AvailabilityPeriodQueryError[] catalogErrors =
            AvailabilityPeriodQueryErrors.ValidateCatalog(data).ToArray();
        if (catalogErrors.Length > 0)
        {
            return AvailabilityPeriodQueryResult.Failure(
                AvailabilityPeriodQueryStatus.CatalogInvalid,
                catalogErrors);
        }


        AvailabilityPeriodQueryError[] versionErrors = data.Entries
            .Where(item => item.ChangeVersion <= 0)
            .Select(AvailabilityPeriodQueryErrors.InvalidChangeVersion)
            .ToArray();
        if (versionErrors.Length > 0)
        {
            return AvailabilityPeriodQueryResult.Failure(
                AvailabilityPeriodQueryStatus.StoredDataInvalid,
                versionErrors);
        }

        Dictionary<EmployeeTypeId, EmployeeType> employeeTypes = data.EmployeeTypes
            .ToDictionary(employeeType => employeeType.Id);
        Dictionary<(EmployeeId EmployeeId, DateOnly Date), AvailabilityEntryReadItem>
            entryItems = data.Entries.ToDictionary(item =>
                (item.Entry.EmployeeId, item.Entry.Date));
        Dictionary<(EmployeeId EmployeeId, DateOnly WeekMonday), WeeklyAvailability>
            weeks = [];
        List<AvailabilityPeriodQueryError> storedDataErrors = [];

        foreach (Employee employee in data.Employees)
        {
            EmployeeType employeeType = employeeTypes[employee.EmployeeTypeId];
            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly weekMonday = periodMonday.AddDays(weekIndex * 7);
                WeeklyAvailabilityValidationResult weekResult =
                    WeeklyAvailability.Calculate(
                        employee,
                        employeeType,
                        weekMonday,
                        entrySetResult.Value!);

                if (!weekResult.IsSuccess)
                {
                    storedDataErrors.AddRange(
                        weekResult.Errors.Select(error =>
                            AvailabilityPeriodQueryErrors.FromWeeklyAvailability(
                                employee,
                                error)));
                    continue;
                }

                weeks.Add((employee.Id, weekMonday), weekResult.Value!);
            }
        }

        if (storedDataErrors.Count > 0)
        {
            return AvailabilityPeriodQueryResult.Failure(
                AvailabilityPeriodQueryStatus.StoredDataInvalid,
                storedDataErrors);
        }

        AvailabilityPeriodSnapshot snapshot = new AvailabilityPeriodSnapshotProjector(
            data,
            entryItems,
            employeeTypes,
            weeks).Create(periodMonday, periodSunday);

        return AvailabilityPeriodQueryResult.Success(snapshot);
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
