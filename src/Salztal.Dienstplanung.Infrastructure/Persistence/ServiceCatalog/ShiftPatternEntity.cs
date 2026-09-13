using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ShiftPatternEntity
{
    public Guid Id { get; set; }

    public StoredShiftPatternKind Kind { get; set; }

    public string DisplayCode { get; set; } = string.Empty;

    public string? DisplayColorCode { get; set; }

    public DayOfWeek? AllowedDay { get; set; }

    public ReliefShiftSwitchRule? SwitchRule { get; set; }

    public bool HasInterruption { get; set; }

    public Guid FirstShiftTypeId { get; set; }

    public Guid SecondShiftTypeId { get; set; }

    public ShiftTypeEntity FirstShiftType { get; set; } = null!;

    public ShiftTypeEntity SecondShiftType { get; set; } = null!;
}
