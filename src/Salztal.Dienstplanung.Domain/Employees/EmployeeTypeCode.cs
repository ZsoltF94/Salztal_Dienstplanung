using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypeCode
{
    private EmployeeTypeCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out EmployeeTypeCode? employeeTypeCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            employeeTypeCode = null;
            return false;
        }

        employeeTypeCode = new EmployeeTypeCode(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
