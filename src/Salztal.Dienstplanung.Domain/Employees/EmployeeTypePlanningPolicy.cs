namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypePlanningPolicy
{
    private EmployeeTypePlanningPolicy(
        bool allowsAutomaticAssignment,
        bool requiresWeeklyManualAssignment,
        bool preservesManualAssignmentsOnGeneration,
        ManualSuggestionPriority manualSuggestionPriority)
    {
        AllowsAutomaticAssignment = allowsAutomaticAssignment;
        RequiresWeeklyManualAssignment = requiresWeeklyManualAssignment;
        PreservesManualAssignmentsOnGeneration = preservesManualAssignmentsOnGeneration;
        ManualSuggestionPriority = manualSuggestionPriority;
    }

    public static EmployeeTypePlanningPolicy Standard { get; } = new(
        true,
        false,
        false,
        ManualSuggestionPriority.Standard);

    public static EmployeeTypePlanningPolicy ServiceManagement { get; } = new(
        false,
        true,
        true,
        ManualSuggestionPriority.LastResort);

    public bool AllowsAutomaticAssignment { get; }

    public bool RequiresWeeklyManualAssignment { get; }

    public bool PreservesManualAssignmentsOnGeneration { get; }

    public ManualSuggestionPriority ManualSuggestionPriority { get; }
}
