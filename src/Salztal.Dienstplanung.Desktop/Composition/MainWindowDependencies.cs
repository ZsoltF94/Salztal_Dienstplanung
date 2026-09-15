using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;

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
    IDeleteEmployeeStore DeleteEmployeeStore,
    ICreateEmployeeTypeStore CreateEmployeeTypeStore,
    IUpdateEmployeeTypeStore UpdateEmployeeTypeStore,
    IDeleteEmployeeTypeStore DeleteEmployeeTypeStore,
    IStaffingDemandReader StaffingDemandReader,
    IStandardStaffingDemandRevisionStore StandardStaffingDemandRevisionStore,
    IStaffingDemandDateExceptionStore StaffingDemandDateExceptionStore,
    IRemoveStaffingDemandDateExceptionStore RemoveStaffingDemandDateExceptionStore,
    IAvailabilityReader AvailabilityReader,
    ISetAvailabilityEntryStore SetAvailabilityEntryStore,
    IRemoveAvailabilityEntryStore RemoveAvailabilityEntryStore);
