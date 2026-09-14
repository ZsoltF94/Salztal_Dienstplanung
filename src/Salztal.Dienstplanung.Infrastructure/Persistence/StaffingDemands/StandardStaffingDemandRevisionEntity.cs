using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

internal sealed class StandardStaffingDemandRevisionEntity
{
    public Guid Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public Guid WorkLocationId { get; set; }

    public Guid ShiftTypeId { get; set; }

    public DateOnly EffectiveFromMonday { get; set; }

    public int CorrectionSequence { get; set; }

    public StandardStaffingDemandRevisionKind Kind { get; set; }

    public int? ActualStartMinutes { get; set; }

    public int? ActualEndMinutes { get; set; }

    public int? RequiredEmployeeCount { get; set; }

    public WorkLocationEntity WorkLocation { get; set; } = null!;

    public ShiftTypeEntity ShiftType { get; set; } = null!;
}
