namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AutomaticScheduleDayOffProposal
{
    public AutomaticScheduleDayOffProposal(Guid employeeId, DateOnly date)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("Employee identifier is required.", nameof(employeeId));
        }

        EmployeeId = employeeId;
        Date = date;
    }

    public Guid EmployeeId { get; }

    public DateOnly Date { get; }
}
