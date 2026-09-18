using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Tests.Availabilities;

public sealed class GetAvailabilityPeriodQueryTests
{
    private static readonly Guid EmployeeIdentifier =
        new("ff645803-8ff7-4652-a382-0125900ca651");

    private static readonly DateOnly FirstMonday = new(2026, 12, 21);

    [Fact]
    public async Task ExecuteWhenInventoryIsEmptyReturnsImmutableTwentyOneDayPeriod()
    {
        FakeAvailabilityReader reader = new(CreateReadData([], [], []));
        GetAvailabilityPeriodQuery query = new(reader);

        AvailabilityPeriodQueryResult result = await query.ExecuteAsync(
            new DateOnly(2026, 12, 23),
            TestContext.Current.CancellationToken);

        AvailabilityPeriodSnapshot period = Assert.IsType<AvailabilityPeriodSnapshot>(
            result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(FirstMonday, period.PeriodMonday);
        Assert.Equal(new DateOnly(2027, 1, 10), period.PeriodSunday);
        Assert.Equal(21, period.Days.Count);
        Assert.Equal(FirstMonday, period.Days[0].Date);
        Assert.Equal(DayOfWeek.Monday, period.Days[0].DayOfWeek);
        Assert.Equal(new DateOnly(2027, 1, 10), period.Days[^1].Date);
        Assert.Empty(period.Employees);
        Assert.True(((ICollection<AvailabilityPeriodDaySnapshot>)period.Days).IsReadOnly);
        Assert.True(
            ((ICollection<AvailabilityPeriodEmployeeSnapshot>)period.Employees).IsReadOnly);
        Assert.Equal([(FirstMonday, new DateOnly(2027, 1, 10))], reader.RequestedPeriods);
    }

    [Fact]
    public async Task ExecuteWhenEmployeesExistReturnsOnlyActiveEmployees()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee active = CreateEmployee(EmployeeIdentifier, type, true);
        Employee inactive = CreateEmployee(
            new Guid("5e036a8e-9c28-4cdf-b75e-c23e4da4f61a"),
            type,
            false);
        FakeAvailabilityReader reader = new(CreateReadData(
            [inactive, active],
            [type],
            []));

        AvailabilityPeriodSnapshot period = await ExecuteSuccessfully(reader);

        AvailabilityPeriodEmployeeSnapshot row = Assert.Single(period.Employees);
        Assert.Equal(active.Id.Value, row.EmployeeId);
        Assert.Equal("Erika Muster", row.DisplayName);
        Assert.Equal(type.Id.Value, row.EmployeeTypeId);
        Assert.Equal("Typ25", row.EmployeeTypeCode);
        Assert.Equal("Restaurant - 25 Stunden", row.EmployeeTypeName);
        Assert.Equal(EmployeeTypePlanningRoleKind.Normal, row.PlanningRole);
        Assert.True(row.AllowsVacationAndSickness);
        Assert.Equal(300, row.AbsenceDayValueMinutes);
    }

    [Fact]
    public async Task ExecuteProjectsEveryEmployeeTypePlanningRole()
    {
        Guid serviceManagementId = new("1462617e-f0c8-45df-b90f-e58be36a302c");
        Guid auxiliaryId = new("848338fd-6a72-4c20-8265-bab9dd91c469");
        Employee normal = CreateEmployee(
            EmployeeIdentifier,
            InitialEmployeeTypeCatalog.Type25,
            true);
        Employee serviceManagement = CreateEmployee(
            serviceManagementId,
            InitialEmployeeTypeCatalog.Type1,
            true);
        Employee auxiliary = CreateEmployee(
            auxiliaryId,
            InitialEmployeeTypeCatalog.TypeAh1,
            true);
        FakeAvailabilityReader reader = new(CreateReadData(
            [normal, serviceManagement, auxiliary],
            [
                InitialEmployeeTypeCatalog.Type25,
                InitialEmployeeTypeCatalog.Type1,
                InitialEmployeeTypeCatalog.TypeAh1,
            ],
            []));

        AvailabilityPeriodSnapshot period = await ExecuteSuccessfully(reader);

        Dictionary<Guid, EmployeeTypePlanningRoleKind> roles = period.Employees
            .ToDictionary(employee => employee.EmployeeId, employee => employee.PlanningRole);
        Assert.Equal(EmployeeTypePlanningRoleKind.Normal, roles[EmployeeIdentifier]);
        Assert.Equal(
            EmployeeTypePlanningRoleKind.ServiceManagement,
            roles[serviceManagementId]);
        Assert.Equal(EmployeeTypePlanningRoleKind.Auxiliary, roles[auxiliaryId]);
    }

    [Fact]
    public async Task ExecuteWhenEntriesExistProjectsStructuredCurrentKinds()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(EmployeeIdentifier, type, true);
        AvailabilityEntry vacation = CreateEntry(
            EmployeeIdentifier,
            FirstMonday,
            AvailabilityEntryKind.Vacation);
        AvailabilityEntry fixedDayOff = CreateEntry(
            EmployeeIdentifier,
            FirstMonday.AddDays(2),
            AvailabilityEntryKind.FixedDayOff);
        FakeAvailabilityReader reader = new(CreateReadData(
            [employee],
            [type],
            [vacation, fixedDayOff]));

        AvailabilityPeriodSnapshot period = await ExecuteSuccessfully(reader);

        AvailabilityPeriodEmployeeSnapshot row = Assert.Single(period.Employees);
        Assert.Collection(
            row.Entries,
            entry =>
            {
                Assert.Equal(FirstMonday, entry.Date);
                Assert.Equal(AvailabilityDayEntryKind.Vacation, entry.Kind);
                Assert.Equal(1, entry.ChangeVersion);
            },
            entry =>
            {
                Assert.Equal(FirstMonday.AddDays(2), entry.Date);
                Assert.Equal(AvailabilityDayEntryKind.FixedDayOff, entry.Kind);
                Assert.Equal(2, entry.ChangeVersion);
            });
        Assert.True(((ICollection<AvailabilityPeriodEntrySnapshot>)row.Entries).IsReadOnly);
    }

    [Fact]
    public async Task ExecuteCalculatesThreeWeeksSeparately()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(EmployeeIdentifier, type, true);
        FakeAvailabilityReader reader = new(CreateReadData(
            [employee],
            [type],
            [
                CreateEntry(
                    EmployeeIdentifier,
                    FirstMonday,
                    AvailabilityEntryKind.Vacation),
                CreateEntry(
                    EmployeeIdentifier,
                    FirstMonday.AddDays(7),
                    AvailabilityEntryKind.Sickness),
                CreateEntry(
                    EmployeeIdentifier,
                    FirstMonday.AddDays(8),
                    AvailabilityEntryKind.Sickness),
                CreateEntry(
                    EmployeeIdentifier,
                    FirstMonday.AddDays(14),
                    AvailabilityEntryKind.FixedDayOff),
            ]));

        AvailabilityPeriodSnapshot period = await ExecuteSuccessfully(reader);

        AvailabilityPeriodEmployeeSnapshot row = Assert.Single(period.Employees);
        Assert.Collection(
            row.Weeks,
            week => AssertWeek(week, FirstMonday, 1_500, 1_200),
            week => AssertWeek(week, FirstMonday.AddDays(7), 1_500, 900),
            week => AssertWeek(week, FirstMonday.AddDays(14), 1_500, 1_500));
        Assert.True(((ICollection<AvailabilityPeriodWeekSnapshot>)row.Weeks).IsReadOnly);
    }

    [Fact]
    public async Task ExecuteForSundayAndNextPeriodUsesTheirRespectiveMondays()
    {
        FakeAvailabilityReader reader = new(CreateReadData([], [], []));
        GetAvailabilityPeriodQuery query = new(reader);

        AvailabilityPeriodQueryResult first = await query.ExecuteAsync(
            new DateOnly(2026, 12, 27),
            TestContext.Current.CancellationToken);
        AvailabilityPeriodQueryResult second = await query.ExecuteAsync(
            new DateOnly(2027, 1, 11),
            TestContext.Current.CancellationToken);

        Assert.Equal(FirstMonday, first.Value?.PeriodMonday);
        Assert.Equal(new DateOnly(2027, 1, 11), second.Value?.PeriodMonday);
        Assert.Equal(2, reader.LoadCallCount);
    }

    [Fact]
    public async Task ExecuteWhenAlreadyCancelledDoesNotRead()
    {
        FakeAvailabilityReader reader = new(CreateReadData([], [], []));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<AvailabilityPeriodQueryResult> execution = new GetAvailabilityPeriodQuery(reader)
            .ExecuteAsync(FirstMonday, cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
    }

    [Fact]
    public async Task ExecuteWhenCancelledDuringReadDoesNotProjectSnapshot()
    {
        using CancellationTokenSource cancellation = new();
        FakeAvailabilityReader reader = new(
            CreateReadData([], [], []),
            () => cancellation.Cancel());

        Task<AvailabilityPeriodQueryResult> execution = new GetAvailabilityPeriodQuery(reader)
            .ExecuteAsync(FirstMonday, cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public async Task ExecuteWhenPeriodCannotContainTwentyOneDaysReturnsValidationError()
    {
        FakeAvailabilityReader reader = new(CreateReadData([], [], []));

        AvailabilityPeriodQueryResult result = await new GetAvailabilityPeriodQuery(reader)
            .ExecuteAsync(DateOnly.MaxValue, TestContext.Current.CancellationToken);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.ValidationFailed, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.PeriodMustFitTwentyOneDays,
            error.Code);
        Assert.Equal(0, reader.LoadCallCount);
    }

    [Fact]
    public async Task ExecuteWhenEmployeeTypeIsMissingReturnsVisibleCatalogError()
    {
        Employee employee = CreateEmployee(
            EmployeeIdentifier,
            InitialEmployeeTypeCatalog.Type25,
            true);
        FakeAvailabilityReader reader = new(CreateReadData([employee], [], []));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(AvailabilityPeriodQueryErrorCode.EmployeeTypeMissing, error.Code);
        Assert.Equal(EmployeeIdentifier, error.EmployeeId);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Fact]
    public async Task ExecuteWhenEmployeeIdentifierIsDuplicatedReturnsVisibleCatalogError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(EmployeeIdentifier, type, true);
        FakeAvailabilityReader reader = new(CreateReadData(
            [employee, employee],
            [type],
            []));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.DuplicateEmployeeIdentifier,
            error.Code);
    }

    [Fact]
    public async Task ExecuteWhenEmployeeTypeIdentifierIsDuplicatedReturnsVisibleCatalogError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        FakeAvailabilityReader reader = new(CreateReadData([], [type, type], []));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.DuplicateEmployeeTypeIdentifier,
            error.Code);
    }

    [Fact]
    public async Task ExecuteWhenEntryReferencesUnknownEmployeeReturnsVisibleCatalogError()
    {
        Guid unknownEmployeeId = new("96e31c77-3bef-4e33-a042-d6f98f04100f");
        AvailabilityEntry entry = CreateEntry(
            unknownEmployeeId,
            FirstMonday,
            AvailabilityEntryKind.FixedDayOff);
        FakeAvailabilityReader reader = new(CreateReadData(
            [],
            InitialEmployeeTypeCatalog.All,
            [entry]));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.AvailabilityEmployeeMissing,
            error.Code);
        Assert.Equal(unknownEmployeeId, error.EmployeeId);
        Assert.Equal(FirstMonday, error.Date);
    }

    [Fact]
    public async Task ExecuteWhenCurrentEntryIsDuplicatedReturnsStoredDataError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(EmployeeIdentifier, type, true);
        AvailabilityEntry entry = CreateEntry(
            EmployeeIdentifier,
            FirstMonday,
            AvailabilityEntryKind.Vacation);
        FakeAvailabilityReader reader = new(CreateReadData(
            [employee],
            [type],
            [entry, entry]));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.DuplicateEmployeeAndDate,
            error.Code);
    }

    [Fact]
    public async Task ExecuteWhenAuxiliaryEntryUsesVacationReturnsStoredDataError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.TypeAh1;
        Employee employee = CreateEmployee(EmployeeIdentifier, type, true);
        FakeAvailabilityReader reader = new(CreateReadData(
            [employee],
            [type],
            [
                CreateEntry(
                    EmployeeIdentifier,
                    FirstMonday,
                    AvailabilityEntryKind.Vacation),
            ]));

        AvailabilityPeriodQueryResult result = await Execute(reader);

        AvailabilityPeriodQueryError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityPeriodQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            AvailabilityPeriodQueryErrorCode.VacationAndSicknessNotAllowed,
            error.Code);
        Assert.Equal(EmployeeIdentifier, error.EmployeeId);
        Assert.Equal(FirstMonday, error.Date);
    }

    private static async Task<AvailabilityPeriodSnapshot> ExecuteSuccessfully(
        FakeAvailabilityReader reader)
    {
        AvailabilityPeriodQueryResult result = await Execute(reader);
        return Assert.IsType<AvailabilityPeriodSnapshot>(result.Value);
    }

    private static Task<AvailabilityPeriodQueryResult> Execute(
        FakeAvailabilityReader reader)
    {
        return new GetAvailabilityPeriodQuery(reader).ExecuteAsync(
            FirstMonday,
            TestContext.Current.CancellationToken);
    }

    private static void AssertWeek(
        AvailabilityPeriodWeekSnapshot week,
        DateOnly monday,
        int uncutMinutes,
        int effectiveMinutes)
    {
        Assert.Equal(monday, week.WeekMonday);
        Assert.Equal(monday.AddDays(6), week.WeekSunday);
        Assert.Equal(uncutMinutes, week.UncutWorkTargetMinutes);
        Assert.Equal(effectiveMinutes, week.EffectiveWorkTargetMinutes);
    }

    private static AvailabilityReadData CreateReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType> employeeTypes,
        IEnumerable<AvailabilityEntry> entries)
    {
        return new AvailabilityReadData(
            employees,
            employeeTypes,
            entries.Select((entry, index) => new AvailabilityEntryReadItem(
                entry,
                index + 1)));
    }

    private static Employee CreateEmployee(
        Guid id,
        EmployeeType employeeType,
        bool isActive)
    {
        Employee employee = Assert.IsType<Employee>(
            Employee.Create(id, "Erika", "Muster", employeeType.Id.Value).Value);
        return isActive ? employee : employee.Deactivate();
    }

    private static AvailabilityEntry CreateEntry(
        Guid employeeId,
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(employeeId, date, kind).Value);
    }

    private sealed class FakeAvailabilityReader : IAvailabilityReader
    {
        private readonly AvailabilityReadData _data;
        private readonly Action? _afterRead;

        public FakeAvailabilityReader(
            AvailabilityReadData data,
            Action? afterRead = null)
        {
            _data = data;
            _afterRead = afterRead;
        }

        public int LoadCallCount { get; private set; }

        public List<(DateOnly Monday, DateOnly Sunday)> RequestedPeriods { get; } = [];

        public Task<AvailabilityReadData> LoadAsync(
            DateOnly periodMonday,
            DateOnly periodSunday,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            RequestedPeriods.Add((periodMonday, periodSunday));
            _afterRead?.Invoke();
            return Task.FromResult(_data);
        }
    }
}
