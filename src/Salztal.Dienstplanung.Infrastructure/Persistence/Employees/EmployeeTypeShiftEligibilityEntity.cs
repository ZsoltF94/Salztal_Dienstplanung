using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeTypeShiftEligibilityEntity
{
    public int Id { get; set; }

    public Guid EmployeeTypeId { get; set; }

    public ShiftEligibilityTargetKind TargetKind { get; set; }

    public Guid? ShiftTypeId { get; set; }

    public Guid? ShiftPatternId { get; set; }

    public ShiftEligibilityMode Mode { get; set; }

    public ShiftEligibilityActivation Activation { get; set; }

    public EmployeeTypeEntity EmployeeType { get; set; } = null!;

    public ShiftTypeEntity? ShiftType { get; set; }

    public ShiftPatternEntity? ShiftPattern { get; set; }
}
