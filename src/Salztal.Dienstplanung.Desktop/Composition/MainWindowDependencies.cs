using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop.Composition;

internal sealed record MainWindowDependencies(
    IServiceCatalogReader ServiceCatalogReader,
    IWorkLocationUpdateStore WorkLocationUpdateStore,
    IShiftTypeStandardTimeUpdateStore ShiftTypeStandardTimeUpdateStore,
    IEmployeeReader EmployeeReader,
    ICreateEmployeeStore CreateEmployeeStore,
    IUpdateEmployeeNameStore UpdateEmployeeNameStore,
    IChangeEmployeeTypeStore ChangeEmployeeTypeStore,
    IDeactivateEmployeeStore DeactivateEmployeeStore,
    IReactivateEmployeeStore ReactivateEmployeeStore,
    IDeleteEmployeeStore DeleteEmployeeStore);
