using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

internal sealed class StaffingDemandDateExceptionEntity
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public Guid WorkLocationId { get; set; }

    public Guid ShiftTypeId { get; set; }

    public StaffingDemandDateExceptionKind Kind { get; set; }

    public int? ActualStartMinutes { get; set; }

    public int? ActualEndMinutes { get; set; }

    public int? RequiredEmployeeCount { get; set; }

    public WorkLocationEntity WorkLocation { get; set; } = null!;

    public ShiftTypeEntity ShiftType { get; set; } = null!;
}
