using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules;

namespace Salztal.Dienstplanung.Planning.Validation;

internal sealed class PlanningInputValidator
{
    private readonly InitialRuleTranslationRegistry ruleRegistry;

    public PlanningInputValidator(InitialRuleTranslationRegistry ruleRegistry)
    {
        ArgumentNullException.ThrowIfNull(ruleRegistry);
        this.ruleRegistry = ruleRegistry;
    }

    public PlanningInputValidationResult Validate(PlanningInputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        List<PlanningInputValidationIssue> issues = [];
        issues.AddRange(RuleTranslationRegistryValidator.Validate(ruleRegistry).Select(issue =>
            issue with
            {
                CatalogVersion = snapshot.RuleCatalog.Version,
            }));
        ValidateRuleCatalog(snapshot.RuleCatalog, issues);
        ValidateIdentityAndPeriod(snapshot, issues);
        ValidateEmployees(snapshot, issues);
        ValidateServiceCatalog(snapshot, issues);
        ValidateAvailability(snapshot, issues);
        ValidateDemandSlots(snapshot, issues);
        ValidateHistory(snapshot, issues);
        ValidateProtectedAssignments(snapshot, issues);
        if (IsValidPeriod(snapshot))
        {
            ValidateServiceManagementReadiness(snapshot, issues);
        }

        return new PlanningInputValidationResult(issues.Distinct());
    }

    private void ValidateRuleCatalog(
        RuleCatalogSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value
            ?? throw new InvalidOperationException("The initial rule catalog is unavailable.");
        int supportedVersion = catalog.Version.Value;
        if (snapshot.Version != supportedVersion)
        {
            issues.Add(new PlanningInputValidationIssue(
                PlanningInputValidationCode.UnknownCatalogVersion,
                CatalogVersion: snapshot.Version));
        }

        Dictionary<string, RuleDefinition> knownDefinitions = catalog.Definitions
            .ToDictionary(definition => definition.Id.Value, StringComparer.Ordinal);
        HashSet<string> translatedRuleIds = ruleRegistry.Descriptors
            .Select(descriptor => descriptor.RuleId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (IGrouping<string, RuleDefinitionSnapshot> group in
                 snapshot.Definitions.GroupBy(
                     definition => definition.Id ?? string.Empty,
                     StringComparer.Ordinal))
        {
            if (group.Count() > 1)
            {
                issues.Add(new PlanningInputValidationIssue(
                    PlanningInputValidationCode.DuplicateRule,
                    group.Key,
                    CatalogVersion: snapshot.Version));
            }

            if (!knownDefinitions.TryGetValue(group.Key, out RuleDefinition? expected))
            {
                issues.Add(new PlanningInputValidationIssue(
                    PlanningInputValidationCode.UnknownRule,
                    group.Key,
                    CatalogVersion: snapshot.Version));
                continue;
            }

            if (!translatedRuleIds.Contains(group.Key))
            {
                issues.Add(new PlanningInputValidationIssue(
                    PlanningInputValidationCode.RuleNotTranslated,
                    group.Key,
                    CatalogVersion: snapshot.Version));
            }

            if (group.Any(actual => !Matches(expected, actual)))
            {
                issues.Add(new PlanningInputValidationIssue(
                    PlanningInputValidationCode.RuleDefinitionMismatch,
                    group.Key,
                    CatalogVersion: snapshot.Version));
            }
        }

        HashSet<string> suppliedRuleIds = snapshot.Definitions
            .Select(definition => definition.Id ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        foreach (string missingRuleId in knownDefinitions.Keys
                     .Where(ruleId => !suppliedRuleIds.Contains(ruleId))
                     .Order(StringComparer.Ordinal))
        {
            issues.Add(new PlanningInputValidationIssue(
                PlanningInputValidationCode.RuleNotTranslated,
                missingRuleId,
                CatalogVersion: snapshot.Version,
                Parameter: "MissingFromSnapshot"));
        }
    }

    private static bool Matches(
        RuleDefinition expected,
        RuleDefinitionSnapshot actual)
    {
        return actual.Id == expected.Id.Value
            && actual.Family == expected.Family
            && actual.Scope == expected.Scope
            && actual.AutomaticEffect == expected.AutomaticEffect
            && actual.ManualEffect == expected.ManualEffect
            && actual.Priority == expected.Priority
            && Equals(actual.Parameters, expected.Parameters)
            && actual.DescriptionKey == expected.DescriptionKey.Value;
    }

    private static void ValidateIdentityAndPeriod(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        if (snapshot.Id == Guid.Empty)
        {
            Add(issues, PlanningInputValidationCode.InvalidSnapshotIdentity, parameter: "SnapshotId");
        }

        if (snapshot.DraftId == Guid.Empty)
        {
            Add(issues, PlanningInputValidationCode.InvalidSnapshotIdentity, parameter: "DraftId");
        }

        if (snapshot.DraftVersion <= 0)
        {
            Add(issues, PlanningInputValidationCode.InvalidSnapshotIdentity, parameter: "DraftVersion");
        }

        if (!IsValidPeriod(snapshot))
        {
            Add(issues, PlanningInputValidationCode.InvalidPeriod);
        }
    }

    private static bool IsValidPeriod(PlanningInputSnapshot snapshot) =>
        snapshot.PeriodMonday.DayOfWeek == DayOfWeek.Monday
        && snapshot.PeriodSunday.DayNumber - snapshot.PeriodMonday.DayNumber == 20
        && snapshot.PeriodSunday.DayOfWeek == DayOfWeek.Sunday;

    private static void ValidateEmployees(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> typeIds = [];
        foreach (PlanningEmployeeTypeSnapshot type in snapshot.EmployeeTypes)
        {
            if (type.Id == Guid.Empty
                || string.IsNullOrWhiteSpace(type.Code)
                || string.IsNullOrWhiteSpace(type.Name)
                || type.WeeklyWorkTargetMinutes is <= 0 or > WeeklyWorkTarget.MaximumMinutes
                || !Enum.IsDefined(type.PlanningRole)
                || !Enum.IsDefined(type.ManualSuggestionPriority)
                || !HasConsistentPlanningPolicy(type)
                || !typeIds.Add(type.Id))
            {
                Add(issues, PlanningInputValidationCode.InvalidEmployeeType, type.Id);
            }

            if (type.AllowsVacationAndSickness != type.AbsenceDayValueMinutes.HasValue
                || type.AbsenceDayValueMinutes is <= 0 or > AbsenceDayValue.MaximumMinutes
                || type.Eligibilities.Any(eligibility =>
                    eligibility.TargetId == Guid.Empty
                    || !Enum.IsDefined(eligibility.TargetKind)
                    || !Enum.IsDefined(eligibility.Mode)
                    || !Enum.IsDefined(eligibility.Activation))
                || type.Eligibilities.Distinct().Count() != type.Eligibilities.Count)
            {
                Add(issues, PlanningInputValidationCode.InvalidEmployeeType, type.Id);
            }
        }

        HashSet<Guid> employeeIds = [];
        foreach (PlanningEmployeeSnapshot employee in snapshot.Employees)
        {
            if (employee.Id == Guid.Empty
                || employee.EmployeeTypeId == Guid.Empty
                || !typeIds.Contains(employee.EmployeeTypeId)
                || !employeeIds.Add(employee.Id))
            {
                Add(issues, PlanningInputValidationCode.InvalidEmployee, employee.Id);
            }
        }
    }

    private static bool HasConsistentPlanningPolicy(
        PlanningEmployeeTypeSnapshot type)
    {
        return type.PlanningRole switch
        {
            EmployeeTypePlanningRole.Normal or EmployeeTypePlanningRole.Auxiliary =>
                type.AllowsAutomaticAssignment
                && !type.RequiresWeeklyManualAssignment
                && !type.PreservesManualAssignmentsOnGeneration
                && type.ManualSuggestionPriority == ManualSuggestionPriority.Standard,
            EmployeeTypePlanningRole.ServiceManagement =>
                !type.AllowsAutomaticAssignment
                && type.RequiresWeeklyManualAssignment
                && type.PreservesManualAssignmentsOnGeneration
                && type.ManualSuggestionPriority == ManualSuggestionPriority.LastResort,
            _ => false,
        };
    }

    private static void ValidateServiceCatalog(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> locationIds = [];
        foreach (PlanningWorkLocationSnapshot location in snapshot.ServiceCatalog.WorkLocations)
        {
            if (location.Id == Guid.Empty
                || string.IsNullOrWhiteSpace(location.Name)
                || !locationIds.Add(location.Id))
            {
                Add(issues, PlanningInputValidationCode.InvalidServiceCatalog, location.Id);
            }
        }

        HashSet<Guid> shiftTypeIds = [];
        Dictionary<Guid, Guid> shiftTypeLocations = [];
        foreach (PlanningShiftTypeSnapshot shiftType in snapshot.ServiceCatalog.ShiftTypes)
        {
            if (shiftType.Id == Guid.Empty
                || shiftType.WorkLocationId == Guid.Empty
                || !locationIds.Contains(shiftType.WorkLocationId)
                || string.IsNullOrWhiteSpace(shiftType.Name)
                || shiftType.StandardEnd <= shiftType.StandardStart
                || !UsesThirtyMinuteIncrement(shiftType.StandardStart)
                || !UsesThirtyMinuteIncrement(shiftType.StandardEnd)
                || !Enum.IsDefined(shiftType.DisplayKind)
                || !shiftTypeIds.Add(shiftType.Id))
            {
                Add(issues, PlanningInputValidationCode.InvalidServiceCatalog, shiftType.Id);
            }

            shiftTypeLocations.TryAdd(shiftType.Id, shiftType.WorkLocationId);
        }

        PlanningSplitShiftPatternSnapshot split = snapshot.ServiceCatalog.SplitShiftPattern;
        if (split.Id == Guid.Empty
            || !locationIds.Contains(split.WorkLocationId)
            || !shiftTypeIds.Contains(split.FirstShiftTypeId)
            || !shiftTypeIds.Contains(split.SecondShiftTypeId)
            || !shiftTypeLocations.TryGetValue(split.FirstShiftTypeId, out Guid splitFirstLocation)
            || splitFirstLocation != split.WorkLocationId
            || !shiftTypeLocations.TryGetValue(split.SecondShiftTypeId, out Guid splitSecondLocation)
            || splitSecondLocation != split.WorkLocationId
            || split.StandardBreakMinutes <= 0
            || split.StandardWorkMinutes <= 0)
        {
            Add(issues, PlanningInputValidationCode.InvalidServiceCatalog, split.Id);
        }

        PlanningReliefShiftPatternSnapshot relief = snapshot.ServiceCatalog.ReliefShiftPattern;
        if (relief.Id == Guid.Empty
            || !locationIds.Contains(relief.FirstWorkLocationId)
            || !locationIds.Contains(relief.SecondWorkLocationId)
            || !shiftTypeIds.Contains(relief.FirstShiftTypeId)
            || !shiftTypeIds.Contains(relief.SecondShiftTypeId)
            || !shiftTypeLocations.TryGetValue(relief.FirstShiftTypeId, out Guid reliefFirstLocation)
            || reliefFirstLocation != relief.FirstWorkLocationId
            || !shiftTypeLocations.TryGetValue(relief.SecondShiftTypeId, out Guid reliefSecondLocation)
            || reliefSecondLocation != relief.SecondWorkLocationId
            || !Enum.IsDefined(relief.AllowedDay)
            || !Enum.IsDefined(relief.SwitchRule))
        {
            Add(issues, PlanningInputValidationCode.InvalidServiceCatalog, relief.Id);
        }

        foreach (PlanningEmployeeTypeSnapshot type in snapshot.EmployeeTypes)
        {
            foreach (PlanningEmployeeTypeEligibilitySnapshot eligibility in type.Eligibilities)
            {
                bool knownTarget = eligibility.TargetKind switch
                {
                    ShiftEligibilityTargetKind.ShiftType =>
                        shiftTypeIds.Contains(eligibility.TargetId),
                    ShiftEligibilityTargetKind.ShiftPattern =>
                        eligibility.TargetId == split.Id || eligibility.TargetId == relief.Id,
                    _ => false,
                };
                if (!knownTarget)
                {
                    Add(issues, PlanningInputValidationCode.InvalidEmployeeType, type.Id);
                }
            }
        }
    }

    private static void ValidateAvailability(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> employeeIds = snapshot.Employees.Select(employee => employee.Id).ToHashSet();
        HashSet<(Guid, DateOnly)> keys = [];
        foreach (PlanningAvailabilityEntrySnapshot entry in snapshot.AvailabilityEntries)
        {
            if (!employeeIds.Contains(entry.EmployeeId)
                || entry.Date < snapshot.PeriodMonday
                || entry.Date > snapshot.PeriodSunday
                || entry.ChangeVersion <= 0
                || !Enum.IsDefined(entry.Kind)
                || !keys.Add((entry.EmployeeId, entry.Date)))
            {
                Add(
                    issues,
                    PlanningInputValidationCode.InvalidAvailability,
                    entry.EmployeeId,
                    entry.Date);
            }
        }
    }

    private static void ValidateDemandSlots(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> locationIds = snapshot.ServiceCatalog.WorkLocations
            .Select(location => location.Id)
            .ToHashSet();
        HashSet<Guid> shiftTypeIds = snapshot.ServiceCatalog.ShiftTypes
            .Select(shiftType => shiftType.Id)
            .ToHashSet();
        Dictionary<Guid, Guid> shiftTypeLocations = snapshot.ServiceCatalog.ShiftTypes
            .GroupBy(shiftType => shiftType.Id)
            .ToDictionary(group => group.Key, group => group.First().WorkLocationId);
        HashSet<DemandKey> keys = [];

        foreach (ScheduleDemandSlotSnapshot slot in snapshot.DemandSlots)
        {
            DemandKey key = DemandKey.From(slot);
            int duration = MinutesBetween(slot.ActualStart, slot.ActualEnd);
            if (slot.SourceId == Guid.Empty
                || slot.Date < snapshot.PeriodMonday
                || slot.Date > snapshot.PeriodSunday
                || !locationIds.Contains(slot.WorkLocationId)
                || !shiftTypeIds.Contains(slot.ShiftTypeId)
                || !shiftTypeLocations.TryGetValue(slot.ShiftTypeId, out Guid shiftLocationId)
                || shiftLocationId != slot.WorkLocationId
                || slot.Ordinal < 1
                || slot.ActualEnd <= slot.ActualStart
                || !UsesThirtyMinuteIncrement(slot.ActualStart)
                || !UsesThirtyMinuteIncrement(slot.ActualEnd)
                || slot.DurationMinutes != duration
                || !Enum.IsDefined(slot.SourceKind)
                || !keys.Add(key))
            {
                Add(
                    issues,
                    PlanningInputValidationCode.InvalidDemandSlot,
                    slot.SourceId,
                    slot.Date,
                    slot.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }

    private static void ValidateHistory(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> employeeIds = snapshot.Employees.Select(employee => employee.Id).ToHashSet();
        bool invalid = snapshot.History.Days.Count != 7
            || snapshot.History.Days.Select(day => day.Date).Distinct().Count() != 7
            || snapshot.History.Days.Any(day =>
                snapshot.PeriodMonday.DayNumber - day.Date.DayNumber is < 1 or > 7
                || !Enum.IsDefined(day.Status)
                || (day.Status == PlanningHistoryDayStatus.Missing && day.Assignments.Count > 0)
                || day.Assignments.Any(assignment =>
                    !employeeIds.Contains(assignment.EmployeeId)
                    || assignment.WorkMinutes <= 0)
                || day.Assignments.GroupBy(assignment => assignment.EmployeeId)
                    .Any(group => group.Count() > 1));
        int availableDayCount = snapshot.History.Days.Count(day =>
            day.Status == PlanningHistoryDayStatus.Available);
        PlanningHistoryCompleteness expectedCompleteness = availableDayCount switch
        {
            0 => PlanningHistoryCompleteness.Missing,
            7 => PlanningHistoryCompleteness.Complete,
            _ => PlanningHistoryCompleteness.Partial,
        };

        if (invalid || snapshot.History.Completeness != expectedCompleteness)
        {
            Add(issues, PlanningInputValidationCode.InvalidHistory);
        }
    }

    private static void ValidateProtectedAssignments(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        Dictionary<Guid, PlanningEmployeeSnapshot> employees = snapshot.Employees
            .GroupBy(employee => employee.Id)
            .ToDictionary(group => group.Key, group => group.First());
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> types = snapshot.EmployeeTypes
            .GroupBy(type => type.Id)
            .ToDictionary(group => group.Key, group => group.First());
        HashSet<(Guid, DateOnly)> unavailable = snapshot.AvailabilityEntries
            .Select(entry => (entry.EmployeeId, entry.Date))
            .ToHashSet();

        foreach (ScheduleAssignmentSnapshot assignment in snapshot.ServiceManagementAssignments)
        {
            if (assignment.AssignmentId == Guid.Empty
                || assignment.EmployeeId == Guid.Empty
                || assignment.Date < snapshot.PeriodMonday
                || assignment.Date > snapshot.PeriodSunday
                || assignment.WorkMinutes <= 0
                || !Enum.IsDefined(assignment.Kind))
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialStructureRuleDefinitions.KnownReferences,
                    "InvalidIdentityOrValue");
            }

            if (assignment.Origin != ScheduleAssignmentOriginSnapshot.ServiceManagement
                || !assignment.IsProtectedFromAutomaticGeneration)
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly,
                    "AssignmentNotProtected");
            }

            employees.TryGetValue(
                assignment.EmployeeId,
                out PlanningEmployeeSnapshot? employee);
            types.TryGetValue(
                employee?.EmployeeTypeId ?? Guid.Empty,
                out PlanningEmployeeTypeSnapshot? type);
            if (employee is null || type is null)
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialStructureRuleDefinitions.KnownReferences,
                    "EmployeeOrTypeMissing");
            }
            else if (type.PlanningRole != EmployeeTypePlanningRole.ServiceManagement)
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly,
                    "WrongPlanningRole");
            }

            if (unavailable.Contains((assignment.EmployeeId, assignment.Date)))
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialStructureRuleDefinitions.BlockedDayMarker,
                    "AvailabilityConflict");
            }

            if (!HasValidShape(assignment)
                || assignment.Segments.Sum(segment => (long)segment.WorkMinutes)
                    != assignment.WorkMinutes
                || !HasCurrentDemandTimes(assignment, snapshot))
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialStructureRuleDefinitions.NormalSlotFullCoverage,
                    "DemandCoverageOrTimeChanged");
            }

            if (!HasValidPatternReference(assignment, snapshot.ServiceCatalog))
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialStructureRuleDefinitions.KnownReferences,
                    "PatternReferenceInvalid");
            }

            if (!HasEligibleTarget(assignment, type))
            {
                AddProtectedIssue(
                    issues,
                    assignment,
                    InitialAutomaticHardRuleDefinitions.ShiftEligibilityRequired,
                    "EligibilityMissing");
            }
        }

        foreach (IGrouping<(Guid EmployeeId, DateOnly Date), ScheduleAssignmentSnapshot> group in
                 snapshot.ServiceManagementAssignments.GroupBy(assignment =>
                     (assignment.EmployeeId, assignment.Date)))
        {
            if (group.Count() > 1)
            {
                foreach (ScheduleAssignmentSnapshot assignment in group)
                {
                    AddProtectedIssue(
                        issues,
                        assignment,
                        InitialStructureRuleDefinitions.SingleDailyAssignment,
                        "MultipleDailyAssignments");
                }
            }
        }

        ValidateProtectedCoverageOverlap(
            snapshot.ServiceManagementAssignments,
            issues);
    }

    private static void ValidateProtectedCoverageOverlap(
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        List<PlanningInputValidationIssue> issues)
    {
        IEnumerable<IGrouping<PlanningDemandKey, ProtectedCoverageReference>> coverages =
            assignments
            .SelectMany(assignment => assignment.Coverages.Select(coverage => new
                ProtectedCoverageReference(assignment, coverage)))
            .GroupBy(item => PlanningDemandKey.From(item.Coverage));

        foreach (IGrouping<PlanningDemandKey, ProtectedCoverageReference> demandGroup
                 in coverages)
        {
            ProtectedCoverageReference[] values = demandGroup
                .OrderBy(item => item.Coverage.CoveredStart)
                .ThenBy(item => item.Assignment.AssignmentId)
                .ToArray();
            for (int leftIndex = 0; leftIndex < values.Length; leftIndex++)
            {
                for (int rightIndex = leftIndex + 1; rightIndex < values.Length; rightIndex++)
                {
                    if (values[rightIndex].Coverage.CoveredStart
                        >= values[leftIndex].Coverage.CoveredEnd)
                    {
                        break;
                    }

                    AddProtectedIssue(
                        issues,
                        values[leftIndex].Assignment,
                        InitialAutomaticHardRuleDefinitions.AutomaticNoOverstaffing,
                        "ProtectedCoverageOverlap");
                    AddProtectedIssue(
                        issues,
                        values[rightIndex].Assignment,
                        InitialAutomaticHardRuleDefinitions.AutomaticNoOverstaffing,
                        "ProtectedCoverageOverlap");
                }
            }
        }
    }

    private static void AddProtectedIssue(
        List<PlanningInputValidationIssue> issues,
        ScheduleAssignmentSnapshot assignment,
        RuleDefinition rule,
        string parameter)
    {
        issues.Add(new PlanningInputValidationIssue(
            PlanningInputValidationCode.ProtectedAssignmentConflict,
            rule.Id.Value,
            assignment.AssignmentId,
            assignment.Date,
            Parameter: parameter));
    }

    private static bool HasValidShape(ScheduleAssignmentSnapshot assignment)
    {
        return assignment.Kind switch
        {
            ScheduleAssignmentKindSnapshot.NormalDemand =>
                assignment.Segments.Count == 1 && assignment.Coverages.Count == 1,
            ScheduleAssignmentKindSnapshot.OfficeTime =>
                assignment.Segments.Count == 1 && assignment.Coverages.Count == 0,
            ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                assignment.Segments.Count == 2 && assignment.Coverages.Count == 2,
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                assignment.Segments.Count == 2 && assignment.Coverages.Count == 2,
            _ => false,
        };
    }

    private static bool HasValidPatternReference(
        ScheduleAssignmentSnapshot assignment,
        PlanningServiceCatalogSnapshot catalog)
    {
        return assignment.Kind switch
        {
            ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                assignment.PatternId == catalog.SplitShiftPattern.Id,
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                assignment.PatternId == catalog.ReliefShiftPattern.Id,
            _ => assignment.PatternId is null,
        };
    }

    private static bool HasEligibleTarget(
        ScheduleAssignmentSnapshot assignment,
        PlanningEmployeeTypeSnapshot? type)
    {
        if (type is null)
        {
            return false;
        }

        if (assignment.PatternId is Guid patternId)
        {
            return type.Eligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
                && eligibility.TargetId == patternId
                && eligibility.Activation == ShiftEligibilityActivation.Always);
        }

        return assignment.Segments.All(segment => type.Eligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
            && eligibility.TargetId == segment.ShiftTypeId
            && eligibility.Activation == ShiftEligibilityActivation.Always));
    }

    private static bool HasCurrentDemandTimes(
        ScheduleAssignmentSnapshot assignment,
        PlanningInputSnapshot snapshot)
    {
        for (int index = 0; index < assignment.Segments.Count; index++)
        {
            if (!MatchesDemandSegment(
                    assignment,
                    assignment.Segments[index],
                    index,
                    snapshot))
            {
                return false;
            }
        }

        return assignment.Coverages.All(coverage =>
            MatchesDemandCoverage(assignment, coverage, snapshot));
    }

    private static bool MatchesDemandSegment(
        ScheduleAssignmentSnapshot assignment,
        ScheduleAssignmentSegmentSnapshot segment,
        int segmentIndex,
        PlanningInputSnapshot snapshot)
    {
        int duration = MinutesBetween(segment.ActualStart, segment.ActualEnd);
        ScheduleDemandSlotSnapshot? slot = snapshot.DemandSlots.FirstOrDefault(candidate =>
            candidate.SourceId == segment.DemandSourceId
            && candidate.Date == segment.Date
            && candidate.WorkLocationId == segment.WorkLocationId
            && candidate.ShiftTypeId == segment.ShiftTypeId
            && candidate.ActualEnd == segment.ActualEnd);
        if (slot is null)
        {
            return false;
        }

        bool startMatches = assignment.Kind == ScheduleAssignmentKindSnapshot.ReliefShiftPattern
            && segmentIndex == 1
                ? segment.ActualStart == assignment.Segments[0].ActualEnd
                : segment.ActualStart == slot.ActualStart;

        return segment.Date == assignment.Date
            && segment.DemandSourceId != Guid.Empty
            && segment.WorkLocationId != Guid.Empty
            && segment.ShiftTypeId != Guid.Empty
            && segment.ActualEnd > segment.ActualStart
            && segment.WorkMinutes == duration
            && startMatches;
    }

    private static bool MatchesDemandCoverage(
        ScheduleAssignmentSnapshot assignment,
        ScheduleDemandCoverageSnapshot coverage,
        PlanningInputSnapshot snapshot)
    {
        ScheduleDemandSlotSnapshot? slot = snapshot.DemandSlots.FirstOrDefault(candidate =>
            DemandKey.From(candidate) == DemandKey.From(coverage));
        if (slot is null
            || coverage.Date != assignment.Date
            || coverage.CoveredEnd <= coverage.CoveredStart
            || coverage.CoveredMinutes != MinutesBetween(coverage.CoveredStart, coverage.CoveredEnd)
            || !Enum.IsDefined(coverage.Kind))
        {
            return false;
        }

        return coverage.Kind switch
        {
            ScheduleDemandCoverageKindSnapshot.Full =>
                coverage.CoveredStart == slot.ActualStart
                && coverage.CoveredEnd == slot.ActualEnd,
            ScheduleDemandCoverageKindSnapshot.PartialReliefShift =>
                assignment.Kind == ScheduleAssignmentKindSnapshot.ReliefShiftPattern
                && coverage.CoveredStart == assignment.Segments[0].ActualEnd
                && coverage.CoveredStart > slot.ActualStart
                && coverage.CoveredEnd == slot.ActualEnd,
            _ => false,
        };
    }

    private static void ValidateServiceManagementReadiness(
        PlanningInputSnapshot snapshot,
        List<PlanningInputValidationIssue> issues)
    {
        HashSet<Guid> serviceManagementTypeIds = snapshot.EmployeeTypes
            .Where(type => type.PlanningRole == EmployeeTypePlanningRole.ServiceManagement)
            .Select(type => type.Id)
            .ToHashSet();
        PlanningEmployeeSnapshot[] serviceManagementEmployees = snapshot.Employees
            .Where(employee => serviceManagementTypeIds.Contains(employee.EmployeeTypeId))
            .ToArray();
        if (serviceManagementEmployees.Length != 1)
        {
            Guid? technicalEntityId = serviceManagementEmployees.Length == 0
                ? null
                : serviceManagementEmployees[0].Id;
            Add(
                issues,
                PlanningInputValidationCode.ServiceManagementNotReady,
                technicalEntityId,
                parameter: "RequiresExactlyOneActiveServiceManagementEmployee");
        }

        foreach (PlanningEmployeeSnapshot employee in serviceManagementEmployees)
        {
            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly weekMonday = snapshot.PeriodMonday.AddDays(weekIndex * 7);
                DateOnly weekSunday = weekMonday.AddDays(6);
                int unavailableDayCount = snapshot.AvailabilityEntries.Count(entry =>
                    entry.EmployeeId == employee.Id
                    && entry.Date >= weekMonday
                    && entry.Date <= weekSunday);
                bool hasAssignment = snapshot.ServiceManagementAssignments.Any(assignment =>
                    assignment.EmployeeId == employee.Id
                    && assignment.Date >= weekMonday
                    && assignment.Date <= weekSunday);

                if (unavailableDayCount != 7 && !hasAssignment)
                {
                    Add(
                        issues,
                        PlanningInputValidationCode.ServiceManagementNotReady,
                        employee.Id,
                        weekMonday);
                }
            }
        }
    }

    private static int MinutesBetween(TimeOnly start, TimeOnly end) =>
        (int)(end - start).TotalMinutes;

    private static bool UsesThirtyMinuteIncrement(TimeOnly value) =>
        value.Ticks % TimeSpan.TicksPerMinute == 0 && value.Minute % 30 == 0;

    private static void Add(
        List<PlanningInputValidationIssue> issues,
        PlanningInputValidationCode code,
        Guid? technicalEntityId = null,
        DateOnly? date = null,
        string? parameter = null)
    {
        issues.Add(new PlanningInputValidationIssue(
            code,
            TechnicalEntityId: technicalEntityId,
            Date: date,
            Parameter: parameter));
    }

    private readonly record struct DemandKey(
        Guid SourceId,
        DateOnly Date,
        Guid WorkLocationId,
        Guid ShiftTypeId,
        int Ordinal)
    {
        public static DemandKey From(ScheduleDemandSlotSnapshot slot) => new(
            slot.SourceId,
            slot.Date,
            slot.WorkLocationId,
            slot.ShiftTypeId,
            slot.Ordinal);

        public static DemandKey From(ScheduleDemandCoverageSnapshot coverage) => new(
            coverage.DemandSourceId,
            coverage.Date,
            coverage.WorkLocationId,
            coverage.ShiftTypeId,
            coverage.Ordinal);
    }

    private sealed record ProtectedCoverageReference(
        ScheduleAssignmentSnapshot Assignment,
        ScheduleDemandCoverageSnapshot Coverage);
}
