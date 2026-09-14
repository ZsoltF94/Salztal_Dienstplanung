using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.Employees;

public sealed class EmployeeReadQueriesTests
{
    [Fact]
    public async Task OverviewWhenEmployeesExistReturnsImmutableCurrentTypeValues()
    {
        EmployeeType currentType = CreateUpdatedType25();
        Employee first = CreateEmployee(
            new Guid("5a7b267c-7041-4f61-91fa-eccd729d401f"),
            currentType.Id.Value);
        Employee second = CreateEmployee(
            new Guid("f49c1653-057d-4263-8ae7-3cfa95d821b3"),
            currentType.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([first, second], [currentType]));
        GetEmployeeOverviewQuery query = new(reader);

        EmployeeOverviewSnapshot result = await query.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Employees.Count);
        Assert.True(((ICollection<EmployeeOverviewItemSnapshot>)result.Employees).IsReadOnly);
        Assert.All(result.Employees, employee =>
        {
            Assert.Equal("Erika Beispiel", employee.DisplayName);
            Assert.Equal(currentType.Id.Value, employee.EmployeeTypeId);
            Assert.Equal("Typ25", employee.EmployeeTypeCode);
            Assert.Equal("Restaurant aktuell", employee.EmployeeTypeName);
            Assert.Equal(1_560, employee.WeeklyWorkTargetMinutes);
            Assert.Equal("26 Stunden", employee.WeeklyWorkTargetDisplay);
        });
        Assert.NotEqual(result.Employees[0].Id, result.Employees[1].Id);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public async Task OverviewWhenNoEmployeesExistReturnsEmptyImmutableSnapshot()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        GetEmployeeOverviewQuery query = new(reader);

        EmployeeOverviewSnapshot result = await query.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Empty(result.Employees);
        Assert.True(((ICollection<EmployeeOverviewItemSnapshot>)result.Employees).IsReadOnly);
    }

    [Fact]
    public async Task DetailsWhenEmployeeExistsReturnsCurrentTypeAndEligibilityDisplay()
    {
        Employee employee = CreateEmployee(
            new Guid("87569d90-7a89-490e-a878-c21c94a6bb7b"),
            InitialEmployeeTypeCatalog.TypeAh2.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData(
            [employee],
            InitialEmployeeTypeCatalog.All));
        GetEmployeeDetailsQuery query = new(reader);

        EmployeeDetailsSnapshot? result = await query.ExecuteAsync(
            employee.Id.Value,
            TestContext.Current.CancellationToken);

        EmployeeDetailsSnapshot details = Assert.IsType<EmployeeDetailsSnapshot>(result);
        Assert.Equal("Erika", details.FirstName);
        Assert.Equal("Beispiel", details.LastName);
        Assert.Equal("Erika Beispiel", details.DisplayName);
        Assert.True(details.IsActive);
        Assert.Equal("TypAH2", details.EmployeeType.Code);
        Assert.Equal("10 Stunden", details.EmployeeType.WeeklyWorkTargetDisplay);
        Assert.True(
            ((ICollection<EmployeeTypeEligibilitySnapshot>)details.EmployeeType
                .ShiftEligibilities).IsReadOnly);
        Assert.Contains(
            details.EmployeeType.ShiftEligibilities,
            eligibility => eligibility.TargetName == "Frühdienst"
                && eligibility.AvailabilityDisplay
                    == "Nur als manueller Lösungsvorschlag");
        Assert.Contains(
            details.EmployeeType.ShiftEligibilities,
            eligibility => eligibility.TargetName == "Spr"
                && eligibility.IsShiftPattern
                && eligibility.AvailabilityDisplay
                    == "Nur bei aktivierter Planungslaufoption");
    }

    [Fact]
    public async Task DetailsWhenEmployeeDoesNotExistReturnsNull()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        GetEmployeeDetailsQuery query = new(reader);

        EmployeeDetailsSnapshot? result = await query.ExecuteAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public async Task DetailsWhenIdentifierIsEmptyReturnsNullWithoutReading()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        GetEmployeeDetailsQuery query = new(reader);

        EmployeeDetailsSnapshot? result = await query.ExecuteAsync(
            Guid.Empty,
            TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(0, reader.LoadCallCount);
    }

    [Fact]
    public async Task TypeCatalogWhenTypesExistReturnsReadableImmutableSnapshot()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        GetEmployeeTypeCatalogQuery query = new(reader);

        EmployeeTypeCatalogSnapshot result = await query.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(8, result.EmployeeTypes.Count);
        Assert.True(((ICollection<EmployeeTypeSnapshot>)result.EmployeeTypes).IsReadOnly);

        EmployeeTypeSnapshot type25 = Assert.Single(
            result.EmployeeTypes,
            employeeType => employeeType.Code == "Typ25");
        Assert.Equal("Restaurant - 25 Stunden", type25.Name);
        Assert.Equal(1_500, type25.WeeklyWorkTargetMinutes);
        Assert.Equal("25 Stunden", type25.WeeklyWorkTargetDisplay);
        Assert.Collection(
            type25.ShiftEligibilities,
            eligibility => Assert.Equal("Frühdienst", eligibility.TargetName),
            eligibility => Assert.Equal("Spätdienst", eligibility.TargetName),
            eligibility => Assert.Equal("D", eligibility.TargetName));
        Assert.All(
            type25.ShiftEligibilities,
            eligibility => Assert.Equal("Regulär zulässig", eligibility.AvailabilityDisplay));
    }

    [Fact]
    public async Task TypeCatalogWhenNoTypesExistReturnsEmptyImmutableSnapshot()
    {
        FakeEmployeeReader reader = new(CreateReadData([], []));
        GetEmployeeTypeCatalogQuery query = new(reader);

        EmployeeTypeCatalogSnapshot result = await query.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Empty(result.EmployeeTypes);
        Assert.True(((ICollection<EmployeeTypeSnapshot>)result.EmployeeTypes).IsReadOnly);
    }

    [Theory]
    [InlineData(EmployeeQueryKind.Overview)]
    [InlineData(EmployeeQueryKind.Details)]
    [InlineData(EmployeeQueryKind.TypeCatalog)]
    public async Task QueryWhenCancelledDoesNotRead(EmployeeQueryKind queryKind)
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task execution = queryKind switch
        {
            EmployeeQueryKind.Overview =>
                new GetEmployeeOverviewQuery(reader).ExecuteAsync(cancellation.Token),
            EmployeeQueryKind.Details =>
                new GetEmployeeDetailsQuery(reader).ExecuteAsync(
                    Guid.NewGuid(),
                    cancellation.Token),
            EmployeeQueryKind.TypeCatalog =>
                new GetEmployeeTypeCatalogQuery(reader).ExecuteAsync(cancellation.Token),
            _ => throw new ArgumentOutOfRangeException(nameof(queryKind), queryKind, null),
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
    }

    private static EmployeeReadData CreateReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType> employeeTypes)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new EmployeeReadData(employees, employeeTypes, serviceCatalog);
    }

    private static Employee CreateEmployee(Guid id, Guid employeeTypeId)
    {
        return Assert.IsType<Employee>(
            Employee.Create(id, "Erika", "Beispiel", employeeTypeId).Value);
    }

    private static EmployeeType CreateUpdatedType25()
    {
        EmployeeType original = InitialEmployeeTypeCatalog.Type25;

        return Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                original.Id.Value,
                original.Code.Value,
                "Restaurant aktuell",
                1_560,
                original.ShiftEligibilities,
                original.PlanningPolicy).Value);
    }

    public enum EmployeeQueryKind
    {
        Overview,
        Details,
        TypeCatalog,
    }

    private sealed class FakeEmployeeReader : IEmployeeReader
    {
        private readonly EmployeeReadData _data;

        public FakeEmployeeReader(EmployeeReadData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }
}
