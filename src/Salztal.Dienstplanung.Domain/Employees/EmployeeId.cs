using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeId
{
    private EmployeeId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out EmployeeId? employeeId)
    {
        if (value == Guid.Empty)
        {
            employeeId = null;
            return false;
        }

        employeeId = new EmployeeId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
