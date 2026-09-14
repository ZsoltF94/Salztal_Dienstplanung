using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StaffingDemandId
{
    private StaffingDemandId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out StaffingDemandId? staffingDemandId)
    {
        if (value == Guid.Empty)
        {
            staffingDemandId = null;
            return false;
        }

        staffingDemandId = new StaffingDemandId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
