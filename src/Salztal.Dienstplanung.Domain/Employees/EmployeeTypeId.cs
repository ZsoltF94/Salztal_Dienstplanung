using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypeId
{
    private EmployeeTypeId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out EmployeeTypeId? employeeTypeId)
    {
        if (value == Guid.Empty)
        {
            employeeTypeId = null;
            return false;
        }

        employeeTypeId = new EmployeeTypeId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
