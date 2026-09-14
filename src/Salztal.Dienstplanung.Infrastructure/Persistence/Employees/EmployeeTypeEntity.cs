using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeTypeEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int WeeklyWorkTargetMinutes { get; set; }

    public bool AllowsAutomaticAssignment { get; set; }

    public bool RequiresWeeklyManualAssignment { get; set; }

    public bool PreservesManualAssignmentsOnGeneration { get; set; }

    public ManualSuggestionPriority ManualSuggestionPriority { get; set; }
}
