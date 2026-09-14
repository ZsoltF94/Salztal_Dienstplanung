using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeLastName
{
    private EmployeeLastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out EmployeeLastName? lastName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            lastName = null;
            return false;
        }

        lastName = new EmployeeLastName(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
