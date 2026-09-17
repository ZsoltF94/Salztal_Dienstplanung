using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class DeterministicAutomaticScheduleAssignmentIdFactory :
    IAutomaticScheduleAssignmentIdFactory
{
    public Guid Create(AutomaticScheduleAssignmentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        StringBuilder canonical = new();
        Append(canonical, identity.SnapshotId.ToString("D"));
        Append(canonical, identity.EmployeeId.ToString("D"));
        Append(canonical, identity.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Append(canonical, ((int)identity.Kind).ToString(CultureInfo.InvariantCulture));
        Append(canonical, identity.PatternId?.ToString("D") ?? "-");

        foreach (AutomaticScheduleDemandSlotIdentity slot in identity.DemandSlots)
        {
            Append(canonical, slot.SourceId.ToString("D"));
            Append(canonical, slot.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Append(canonical, slot.WorkLocationId.ToString("D"));
            Append(canonical, slot.ShiftTypeId.ToString("D"));
            Append(canonical, slot.Ordinal.ToString(CultureInfo.InvariantCulture));
        }

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
        Span<byte> identifierBytes = hash.AsSpan(0, 16);
        identifierBytes[7] = (byte)((identifierBytes[7] & 0x0F) | 0x80);
        identifierBytes[8] = (byte)((identifierBytes[8] & 0x3F) | 0x80);
        Guid identifier = new(identifierBytes);
        return identifier == Guid.Empty
            ? throw new InvalidOperationException("Deterministic identifier cannot be empty.")
            : identifier;
    }

    private static void Append(StringBuilder builder, string value)
    {
        builder.Append(value.Length.ToString(CultureInfo.InvariantCulture));
        builder.Append(':');
        builder.Append(value);
        builder.Append('|');
    }
}
