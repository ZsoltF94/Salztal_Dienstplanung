using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypeName
{
    private EmployeeTypeName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out EmployeeTypeName? employeeTypeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            employeeTypeName = null;
            return false;
        }

        employeeTypeName = new EmployeeTypeName(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
