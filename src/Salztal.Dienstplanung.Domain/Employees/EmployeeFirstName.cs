using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeFirstName
{
    private EmployeeFirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out EmployeeFirstName? firstName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            firstName = null;
            return false;
        }

        firstName = new EmployeeFirstName(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
