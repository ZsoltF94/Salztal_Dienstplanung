using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Rules;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum PlanningInputChangeCategory
{
    EmployeesAndTypes,
    ShiftEligibilities,
    ServiceCatalog,
    StaffingDemands,
    AvailabilityEntries,
    ServiceManagementAssignments,
    RuleCatalog,
    RunOptions,
    History,
}

public sealed class PlanningInputComparison
{
    private PlanningInputComparison(
        ReadOnlyCollection<PlanningInputChangeCategory> changedCategories)
    {
        ChangedCategories = changedCategories;
    }

    public bool IsCurrent => ChangedCategories.Count == 0;

    public ReadOnlyCollection<PlanningInputChangeCategory> ChangedCategories { get; }

    public static PlanningInputComparison Compare(
        PlanningInputSnapshot stored,
        PlanningInputSnapshot current)
    {
        ArgumentNullException.ThrowIfNull(stored);
        ArgumentNullException.ThrowIfNull(current);
        List<PlanningInputChangeCategory> changes = [];

        AddIfChanged(
            changes,
            PlanningInputChangeCategory.EmployeesAndTypes,
            !stored.Employees.SequenceEqual(current.Employees)
            || !EmployeeTypeDetailsEqual(stored.EmployeeTypes, current.EmployeeTypes));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.ShiftEligibilities,
            !EligibilitiesEqual(stored.EmployeeTypes, current.EmployeeTypes));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.ServiceCatalog,
            !ServiceCatalogEqual(stored.ServiceCatalog, current.ServiceCatalog));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.StaffingDemands,
            !stored.DemandSlots.SequenceEqual(current.DemandSlots));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.AvailabilityEntries,
            !stored.AvailabilityEntries.SequenceEqual(current.AvailabilityEntries));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.ServiceManagementAssignments,
            !AssignmentsEqual(
                stored.ServiceManagementAssignments,
                current.ServiceManagementAssignments));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.RuleCatalog,
            !RuleCatalogEqual(stored.RuleCatalog, current.RuleCatalog));
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.RunOptions,
            stored.RunOptions != current.RunOptions);
        AddIfChanged(
            changes,
            PlanningInputChangeCategory.History,
            !HistoryEqual(stored.History, current.History));

        return new PlanningInputComparison(Array.AsReadOnly(changes.ToArray()));
    }

    private static bool EmployeeTypeDetailsEqual(
        ReadOnlyCollection<PlanningEmployeeTypeSnapshot> first,
        ReadOnlyCollection<PlanningEmployeeTypeSnapshot> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        for (int index = 0; index < first.Count; index++)
        {
            PlanningEmployeeTypeSnapshot left = first[index];
            PlanningEmployeeTypeSnapshot right = second[index];
            if (!(left.Id == right.Id
                && left.Code == right.Code
                && left.Name == right.Name
                && left.WeeklyWorkTargetMinutes == right.WeeklyWorkTargetMinutes
                && left.AllowsVacationAndSickness == right.AllowsVacationAndSickness
                && left.AbsenceDayValueMinutes == right.AbsenceDayValueMinutes
                && left.PlanningRole == right.PlanningRole
                && left.AllowsAutomaticAssignment == right.AllowsAutomaticAssignment
                && left.RequiresWeeklyManualAssignment
                    == right.RequiresWeeklyManualAssignment
                && left.PreservesManualAssignmentsOnGeneration
                    == right.PreservesManualAssignmentsOnGeneration
                && left.ManualSuggestionPriority == right.ManualSuggestionPriority))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EligibilitiesEqual(
        ReadOnlyCollection<PlanningEmployeeTypeSnapshot> first,
        ReadOnlyCollection<PlanningEmployeeTypeSnapshot> second)
    {
        return first.Count == second.Count
            && first.Zip(second).All(pair =>
                pair.First.Id == pair.Second.Id
                && pair.First.Eligibilities.SequenceEqual(pair.Second.Eligibilities));
    }

    private static bool ServiceCatalogEqual(
        PlanningServiceCatalogSnapshot first,
        PlanningServiceCatalogSnapshot second)
    {
        return first.WorkLocations.SequenceEqual(second.WorkLocations)
            && first.ShiftTypes.SequenceEqual(second.ShiftTypes)
            && first.SplitShiftPattern == second.SplitShiftPattern
            && first.ReliefShiftPattern == second.ReliefShiftPattern;
    }

    private static bool AssignmentsEqual(
        ReadOnlyCollection<ScheduleAssignmentSnapshot> first,
        ReadOnlyCollection<ScheduleAssignmentSnapshot> second)
    {
        return first.Count == second.Count
            && first.Zip(second).All(pair =>
                pair.First.AssignmentId == pair.Second.AssignmentId
                && pair.First.EmployeeId == pair.Second.EmployeeId
                && pair.First.Date == pair.Second.Date
                && pair.First.Kind == pair.Second.Kind
                && pair.First.Origin == pair.Second.Origin
                && pair.First.PatternId == pair.Second.PatternId
                && pair.First.WorkMinutes == pair.Second.WorkMinutes
                && pair.First.IsProtectedFromAutomaticGeneration
                    == pair.Second.IsProtectedFromAutomaticGeneration
                && pair.First.Segments.SequenceEqual(pair.Second.Segments)
                && pair.First.Coverages.SequenceEqual(pair.Second.Coverages));
    }

    private static bool RuleCatalogEqual(
        RuleCatalogSnapshot first,
        RuleCatalogSnapshot second)
    {
        return first.Version == second.Version
            && first.Definitions.Count == second.Definitions.Count
            && first.Definitions.Zip(second.Definitions).All(pair =>
                pair.First.Id == pair.Second.Id
                && pair.First.Family == pair.Second.Family
                && pair.First.Scope == pair.Second.Scope
                && pair.First.AutomaticEffect == pair.Second.AutomaticEffect
                && pair.First.ManualEffect == pair.Second.ManualEffect
                && pair.First.Priority == pair.Second.Priority
                && pair.First.Parameters == pair.Second.Parameters
                && pair.First.DescriptionKey == pair.Second.DescriptionKey);
    }

    private static bool HistoryEqual(
        PlanningHistorySnapshot first,
        PlanningHistorySnapshot second)
    {
        return first.Completeness == second.Completeness
            && first.Days.Count == second.Days.Count
            && first.Days.Zip(second.Days).All(pair =>
                pair.First.Date == pair.Second.Date
                && pair.First.Status == pair.Second.Status
                && pair.First.Assignments.SequenceEqual(pair.Second.Assignments));
    }

    private static void AddIfChanged(
        List<PlanningInputChangeCategory> changes,
        PlanningInputChangeCategory category,
        bool changed)
    {
        if (changed)
        {
            changes.Add(category);
        }
    }
}
