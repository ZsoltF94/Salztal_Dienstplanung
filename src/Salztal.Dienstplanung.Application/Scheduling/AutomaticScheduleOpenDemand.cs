namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticScheduleOpenDemandKind
{
    FullyUncovered,
    PartiallyUncoveredReliefShift,
}

public sealed record AutomaticScheduleOpenDemand
{
    public AutomaticScheduleOpenDemand(
        Guid sourceId,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        int ordinal,
        TimeOnly uncoveredStart,
        TimeOnly uncoveredEnd,
        int uncoveredMinutes,
        AutomaticScheduleOpenDemandKind kind)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException("Demand source identifier is required.", nameof(sourceId));
        }

        if (workLocationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Work-location identifier is required.",
                nameof(workLocationId));
        }

        if (shiftTypeId == Guid.Empty)
        {
            throw new ArgumentException("Shift-type identifier is required.", nameof(shiftTypeId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(uncoveredMinutes);
        if (uncoveredEnd <= uncoveredStart)
        {
            throw new ArgumentException("Uncovered demand time must have a positive duration.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        int actualMinutes = (int)(uncoveredEnd - uncoveredStart).TotalMinutes;
        if (uncoveredMinutes != actualMinutes)
        {
            throw new ArgumentException(
                "Uncovered minutes must match the uncovered time range.",
                nameof(uncoveredMinutes));
        }

        SourceId = sourceId;
        Date = date;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
        Ordinal = ordinal;
        UncoveredStart = uncoveredStart;
        UncoveredEnd = uncoveredEnd;
        UncoveredMinutes = uncoveredMinutes;
        Kind = kind;
    }

    public Guid SourceId { get; }

    public DateOnly Date { get; }

    public Guid WorkLocationId { get; }

    public Guid ShiftTypeId { get; }

    public int Ordinal { get; }

    public TimeOnly UncoveredStart { get; }

    public TimeOnly UncoveredEnd { get; }

    public int UncoveredMinutes { get; }

    public AutomaticScheduleOpenDemandKind Kind { get; }
}
