using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record PlanningRunOptions(bool EnableAuxiliaryReliefShift)
{
    public static PlanningRunOptions Default { get; } = new(false);
}

public sealed record PlanningEmployeeSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    Guid EmployeeTypeId);

public sealed record PlanningEmployeeTypeEligibilitySnapshot(
    ShiftEligibilityTargetKind TargetKind,
    Guid TargetId,
    ShiftEligibilityMode Mode,
    ShiftEligibilityActivation Activation);

public sealed class PlanningEmployeeTypeSnapshot
{
    public PlanningEmployeeTypeSnapshot(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        EmployeeTypePlanningRole planningRole,
        bool allowsAutomaticAssignment,
        bool requiresWeeklyManualAssignment,
        bool preservesManualAssignmentsOnGeneration,
        ManualSuggestionPriority manualSuggestionPriority,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot> eligibilities)
    {
        ArgumentNullException.ThrowIfNull(eligibilities);
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTargetMinutes = weeklyWorkTargetMinutes;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        AbsenceDayValueMinutes = absenceDayValueMinutes;
        PlanningRole = planningRole;
        AllowsAutomaticAssignment = allowsAutomaticAssignment;
        RequiresWeeklyManualAssignment = requiresWeeklyManualAssignment;
        PreservesManualAssignmentsOnGeneration = preservesManualAssignmentsOnGeneration;
        ManualSuggestionPriority = manualSuggestionPriority;
        Eligibilities = Array.AsReadOnly(eligibilities.ToArray());
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public int WeeklyWorkTargetMinutes { get; }

    public bool AllowsVacationAndSickness { get; }

    public int? AbsenceDayValueMinutes { get; }

    public EmployeeTypePlanningRole PlanningRole { get; }

    public bool AllowsAutomaticAssignment { get; }

    public bool RequiresWeeklyManualAssignment { get; }

    public bool PreservesManualAssignmentsOnGeneration { get; }

    public ManualSuggestionPriority ManualSuggestionPriority { get; }

    public ReadOnlyCollection<PlanningEmployeeTypeEligibilitySnapshot> Eligibilities
    {
        get;
    }
}

public sealed record PlanningAvailabilityEntrySnapshot(
    Guid EmployeeId,
    DateOnly Date,
    AvailabilityEntryKind Kind,
    long ChangeVersion);

public sealed record PlanningWorkLocationSnapshot(
    Guid Id,
    string Name,
    string ColorCode);

public sealed record PlanningShiftTypeSnapshot(
    Guid Id,
    string Name,
    Guid WorkLocationId,
    ShiftTypeDisplayKind DisplayKind,
    string? Abbreviation,
    TimeOnly StandardStart,
    TimeOnly StandardEnd);

public sealed record PlanningSplitShiftPatternSnapshot(
    Guid Id,
    Guid WorkLocationId,
    Guid FirstShiftTypeId,
    Guid SecondShiftTypeId,
    int StandardBreakMinutes,
    int StandardWorkMinutes);

public sealed record PlanningReliefShiftPatternSnapshot(
    Guid Id,
    DayOfWeek AllowedDay,
    Guid FirstWorkLocationId,
    Guid FirstShiftTypeId,
    Guid SecondWorkLocationId,
    Guid SecondShiftTypeId,
    ReliefShiftSwitchRule SwitchRule,
    bool HasInterruption);

public sealed class PlanningServiceCatalogSnapshot
{
    public PlanningServiceCatalogSnapshot(
        IEnumerable<PlanningWorkLocationSnapshot> workLocations,
        IEnumerable<PlanningShiftTypeSnapshot> shiftTypes,
        PlanningSplitShiftPatternSnapshot splitShiftPattern,
        PlanningReliefShiftPatternSnapshot reliefShiftPattern)
    {
        ArgumentNullException.ThrowIfNull(workLocations);
        ArgumentNullException.ThrowIfNull(shiftTypes);
        ArgumentNullException.ThrowIfNull(splitShiftPattern);
        ArgumentNullException.ThrowIfNull(reliefShiftPattern);
        WorkLocations = Array.AsReadOnly(workLocations.ToArray());
        ShiftTypes = Array.AsReadOnly(shiftTypes.ToArray());
        SplitShiftPattern = splitShiftPattern;
        ReliefShiftPattern = reliefShiftPattern;
    }

    public ReadOnlyCollection<PlanningWorkLocationSnapshot> WorkLocations { get; }

    public ReadOnlyCollection<PlanningShiftTypeSnapshot> ShiftTypes { get; }

    public PlanningSplitShiftPatternSnapshot SplitShiftPattern { get; }

    public PlanningReliefShiftPatternSnapshot ReliefShiftPattern { get; }
}

public sealed class PlanningInputSnapshot
{
    public PlanningInputSnapshot(
        Guid id,
        Guid draftId,
        long draftVersion,
        DateOnly periodMonday,
        DateOnly periodSunday,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<PlanningEmployeeTypeSnapshot> employeeTypes,
        PlanningServiceCatalogSnapshot serviceCatalog,
        IEnumerable<PlanningAvailabilityEntrySnapshot> availabilityEntries,
        IEnumerable<ScheduleDemandSlotSnapshot> demandSlots,
        IEnumerable<ScheduleAssignmentSnapshot> serviceManagementAssignments,
        RuleCatalogSnapshot ruleCatalog,
        PlanningRunOptions runOptions,
        PlanningHistorySnapshot history)
    {
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(availabilityEntries);
        ArgumentNullException.ThrowIfNull(demandSlots);
        ArgumentNullException.ThrowIfNull(serviceManagementAssignments);
        ArgumentNullException.ThrowIfNull(ruleCatalog);
        ArgumentNullException.ThrowIfNull(runOptions);
        ArgumentNullException.ThrowIfNull(history);
        Id = id;
        DraftId = draftId;
        DraftVersion = draftVersion;
        PeriodMonday = periodMonday;
        PeriodSunday = periodSunday;
        Employees = Array.AsReadOnly(employees.ToArray());
        EmployeeTypes = Array.AsReadOnly(employeeTypes.ToArray());
        ServiceCatalog = serviceCatalog;
        AvailabilityEntries = Array.AsReadOnly(availabilityEntries.ToArray());
        DemandSlots = Array.AsReadOnly(demandSlots.ToArray());
        ServiceManagementAssignments = Array.AsReadOnly(
            serviceManagementAssignments.ToArray());
        RuleCatalog = ruleCatalog;
        RunOptions = runOptions;
        History = history;
    }

    public Guid Id { get; }

    public Guid DraftId { get; }

    public long DraftVersion { get; }

    public DateOnly PeriodMonday { get; }

    public DateOnly PeriodSunday { get; }

    public ReadOnlyCollection<PlanningEmployeeSnapshot> Employees { get; }

    public ReadOnlyCollection<PlanningEmployeeTypeSnapshot> EmployeeTypes { get; }

    public PlanningServiceCatalogSnapshot ServiceCatalog { get; }

    public ReadOnlyCollection<PlanningAvailabilityEntrySnapshot> AvailabilityEntries
    {
        get;
    }

    public ReadOnlyCollection<ScheduleDemandSlotSnapshot> DemandSlots { get; }

    public ReadOnlyCollection<ScheduleAssignmentSnapshot> ServiceManagementAssignments
    {
        get;
    }

    public RuleCatalogSnapshot RuleCatalog { get; }

    public PlanningRunOptions RunOptions { get; }

    public PlanningHistorySnapshot History { get; }
}
