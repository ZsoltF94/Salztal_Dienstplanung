using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record RequiredEmployeeCount
{
    private RequiredEmployeeCount(int value)
    {
        Value = value;
    }

    public int Value { get; }

    internal static RequiredEmployeeCountValidationCode? TryCreate(
        int value,
        [NotNullWhen(true)] out RequiredEmployeeCount? requiredEmployeeCount)
    {
        if (value <= 0)
        {
            requiredEmployeeCount = null;
            return RequiredEmployeeCountValidationCode.MustBePositive;
        }

        requiredEmployeeCount = new RequiredEmployeeCount(value);
        return null;
    }

    public override string ToString()
    {
        return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
