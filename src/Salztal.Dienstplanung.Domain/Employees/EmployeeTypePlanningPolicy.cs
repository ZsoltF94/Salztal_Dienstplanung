namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypePlanningPolicy
{
    private EmployeeTypePlanningPolicy(
        EmployeeTypePlanningRole role,
        bool allowsAutomaticAssignment,
        bool requiresWeeklyManualAssignment,
        bool preservesManualAssignmentsOnGeneration,
        ManualSuggestionPriority manualSuggestionPriority)
    {
        Role = role;
        AllowsAutomaticAssignment = allowsAutomaticAssignment;
        RequiresWeeklyManualAssignment = requiresWeeklyManualAssignment;
        PreservesManualAssignmentsOnGeneration = preservesManualAssignmentsOnGeneration;
        ManualSuggestionPriority = manualSuggestionPriority;
    }

    public static EmployeeTypePlanningPolicy Standard { get; } = new(
        EmployeeTypePlanningRole.Normal,
        true,
        false,
        false,
        ManualSuggestionPriority.Standard);

    public static EmployeeTypePlanningPolicy ServiceManagement { get; } = new(
        EmployeeTypePlanningRole.ServiceManagement,
        false,
        true,
        true,
        ManualSuggestionPriority.LastResort);

    public static EmployeeTypePlanningPolicy Auxiliary { get; } = new(
        EmployeeTypePlanningRole.Auxiliary,
        true,
        false,
        false,
        ManualSuggestionPriority.Standard);

    public EmployeeTypePlanningRole Role { get; }

    public bool AllowsAutomaticAssignment { get; }

    public bool RequiresWeeklyManualAssignment { get; }

    public bool PreservesManualAssignmentsOnGeneration { get; }

    public ManualSuggestionPriority ManualSuggestionPriority { get; }
}
