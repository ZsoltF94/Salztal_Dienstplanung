using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Planning.Candidates;

internal readonly record struct PlanningDemandKey(
    Guid SourceId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal) : IComparable<PlanningDemandKey>
{
    public string TechnicalKey => string.Join(
        "_",
        SourceId.ToString("N", CultureInfo.InvariantCulture),
        Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
        WorkLocationId.ToString("N", CultureInfo.InvariantCulture),
        ShiftTypeId.ToString("N", CultureInfo.InvariantCulture),
        Ordinal.ToString(CultureInfo.InvariantCulture));

    public int CompareTo(PlanningDemandKey other)
    {
        int result = SourceId.CompareTo(other.SourceId);
        if (result != 0)
        {
            return result;
        }

        result = Date.CompareTo(other.Date);
        if (result != 0)
        {
            return result;
        }

        result = WorkLocationId.CompareTo(other.WorkLocationId);
        if (result != 0)
        {
            return result;
        }

        result = ShiftTypeId.CompareTo(other.ShiftTypeId);
        return result != 0 ? result : Ordinal.CompareTo(other.Ordinal);
    }

    public static PlanningDemandKey From(ScheduleDemandSlotSnapshot slot) => new(
        slot.SourceId,
        slot.Date,
        slot.WorkLocationId,
        slot.ShiftTypeId,
        slot.Ordinal);

    public static PlanningDemandKey From(ScheduleDemandCoverageSnapshot coverage) => new(
        coverage.DemandSourceId,
        coverage.Date,
        coverage.WorkLocationId,
        coverage.ShiftTypeId,
        coverage.Ordinal);
}
