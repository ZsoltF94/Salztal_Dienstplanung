using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Rules;
using Salztal.Dienstplanung.Planning.Validation;

namespace Salztal.Dienstplanung.Planning.Tests.Validation;

public sealed class PlanningInputValidatorTests
{
    private readonly InitialRuleTranslationRegistry registry = new();

    [Fact]
    public void CompleteCurrentSnapshotIsValid()
    {
        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create());

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void CatalogErrorsUseThreeDistinctGroupsAndStableOrder()
    {
        RuleCatalogSnapshot initial = PlanningInputTestFactory.CreateRuleCatalog();
        RuleDefinitionSnapshot knownButMissingTranslation = initial.Definitions[0];
        RuleDefinitionSnapshot unknown = Clone(
            initial.Definitions[1],
            id: "ZZ_UNKNOWN_RULE");
        RuleCatalogSnapshot supplied = new(
            999,
            initial.Definitions.Skip(2).Append(unknown));
        InitialRuleTranslationRegistry incompleteRegistry = new(
            registry.Descriptors.Where(descriptor =>
                descriptor.RuleId != knownButMissingTranslation.Id));

        PlanningInputValidationResult result = new PlanningInputValidator(
            incompleteRegistry).Validate(
                PlanningInputTestFactory.Create(ruleCatalog: supplied));

        Assert.False(result.IsValid);
        Assert.Equal(
            PlanningInputValidationCode.UnknownCatalogVersion,
            result.Issues[0].Code);
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.UnknownRule
            && issue.RuleId == "ZZ_UNKNOWN_RULE"
            && issue.CatalogVersion == 999);
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.RuleNotTranslated
            && issue.RuleId == knownButMissingTranslation.Id
            && issue.CatalogVersion == 999);
        Assert.All(
            result.Issues.Where(issue => issue.RuleId is not null),
            issue => Assert.Equal(999, issue.CatalogVersion));

        PlanningInputValidationIssue[] sortedAgain = result.Issues
            .OrderBy(issue => issue.Code)
            .ThenBy(issue => issue.RuleId, StringComparer.Ordinal)
            .ThenBy(issue => issue.TechnicalEntityId)
            .ThenBy(issue => issue.Date)
            .ThenBy(issue => issue.CatalogVersion)
            .ThenBy(issue => issue.Parameter, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(sortedAgain, result.Issues);
    }

    [Fact]
    public void DuplicateAndChangedRuleDefinitionsAreRejected()
    {
        RuleCatalogSnapshot initial = PlanningInputTestFactory.CreateRuleCatalog();
        RuleDefinitionSnapshot first = initial.Definitions[0];
        RuleDefinitionSnapshot changed = Clone(
            first,
            descriptionKey: "rules.changed");
        RuleCatalogSnapshot supplied = new(
            initial.Version,
            initial.Definitions.Append(changed));

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(ruleCatalog: supplied));

        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.DuplicateRule
            && issue.RuleId == first.Id);
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.RuleDefinitionMismatch
            && issue.RuleId == first.Id);
    }

    [Fact]
    public void MissingTypeReferenceAndInvalidActualTimeAreCollected()
    {
        PlanningEmployeeSnapshot employee = PlanningInputTestFactory.CreateEmployee() with
        {
            EmployeeTypeId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
        };
        ScheduleDemandSlotSnapshot invalidSlot = PlanningInputTestFactory.CreateDemandSlot(
            PlanningInputTestFactory.PeriodMonday,
            actualEnd: new TimeOnly(7, 30),
            durationMinutes: 240);

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                employees: [employee],
                demandSlots: [invalidSlot],
                assignments: []));

        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.InvalidEmployee
            && issue.TechnicalEntityId == employee.Id);
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.InvalidDemandSlot
            && issue.TechnicalEntityId == invalidSlot.SourceId);
    }

    [Fact]
    public void MissingServiceManagementAssignmentIsRejectedPerWeek()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot[] assignments = PlanningInputTestFactory
            .CreateAssignments(slots)
            .Take(2)
            .ToArray();

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                demandSlots: slots,
                assignments: assignments));

        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ServiceManagementNotReady
            && issue.TechnicalEntityId == PlanningInputTestFactory.EmployeeId
            && issue.Date == PlanningInputTestFactory.PeriodMonday.AddDays(14));
    }

    [Fact]
    public void ExactlyOneActiveServiceManagementEmployeeIsRequired()
    {
        PlanningInputValidationResult missing = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                employees: [],
                employeeTypes: [],
                demandSlots: [],
                assignments: []));
        PlanningEmployeeSnapshot first = PlanningInputTestFactory.CreateEmployee();
        PlanningEmployeeSnapshot second = first with
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        };
        PlanningInputValidationResult duplicate = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                employees: [first, second],
                assignments: PlanningInputTestFactory.CreateAssignments()
                    .Concat(PlanningInputTestFactory.CreateAssignments().Select(
                        (assignment, index) => new ScheduleAssignmentSnapshot(
                            Guid.Parse($"b0000000-0000-0000-0000-{index + 1:D12}"),
                            second.Id,
                            assignment.Date,
                            assignment.Kind,
                            assignment.Origin,
                            assignment.PatternId,
                            assignment.WorkMinutes,
                            assignment.IsProtectedFromAutomaticGeneration,
                            assignment.Segments,
                            assignment.Coverages)))));

        Assert.Contains(missing.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ServiceManagementNotReady
            && issue.Parameter == "RequiresExactlyOneActiveServiceManagementEmployee");
        Assert.Contains(duplicate.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ServiceManagementNotReady
            && issue.Parameter == "RequiresExactlyOneActiveServiceManagementEmployee");
    }

    [Fact]
    public void FullyUnavailableServiceManagementWeekNeedsNoAssignment()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot[] assignments = PlanningInputTestFactory
            .CreateAssignments(slots)
            .Take(2)
            .ToArray();
        PlanningAvailabilityEntrySnapshot[] entries = Enumerable.Range(14, 7)
            .Select(day => new PlanningAvailabilityEntrySnapshot(
                PlanningInputTestFactory.EmployeeId,
                PlanningInputTestFactory.PeriodMonday.AddDays(day),
                AvailabilityEntryKind.FixedDayOff,
                day + 1))
            .ToArray();

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                availabilityEntries: entries,
                demandSlots: slots,
                assignments: assignments));

        Assert.DoesNotContain(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ServiceManagementNotReady);
    }

    [Fact]
    public void UnprotectedOrTimeChangedServiceManagementAssignmentIsRejected()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot changed = PlanningInputTestFactory.CreateAssignment(
            slots[0],
            isProtected: false);
        ScheduleAssignmentSnapshot[] assignments =
        [
            changed,
            .. PlanningInputTestFactory.CreateAssignments(slots).Skip(1),
        ];

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                demandSlots: slots,
                assignments: assignments));

        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ProtectedAssignmentConflict
            && issue.TechnicalEntityId == changed.AssignmentId
            && issue.RuleId == InitialAutomaticHardRuleDefinitions
                .ServiceManagementManualOnly.Id.Value
            && issue.Parameter == "AssignmentNotProtected");
    }

    [Fact]
    public void StoredAssignmentWithChangedActualDemandTimeIsRejected()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot original = PlanningInputTestFactory.CreateAssignment(slots[0]);
        ScheduleAssignmentSegmentSnapshot segment = original.Segments[0];
        ScheduleAssignmentSnapshot changed = new(
            original.AssignmentId,
            original.EmployeeId,
            original.Date,
            original.Kind,
            original.Origin,
            original.PatternId,
            original.WorkMinutes - 30,
            true,
            [
                segment with
                {
                    ActualEnd = segment.ActualEnd.AddMinutes(-30),
                    WorkMinutes = segment.WorkMinutes - 30,
                },
            ],
            original.Coverages);
        ScheduleAssignmentSnapshot[] assignments =
        [
            changed,
            .. PlanningInputTestFactory.CreateAssignments(slots).Skip(1),
        ];

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                demandSlots: slots,
                assignments: assignments));

        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ProtectedAssignmentConflict
            && issue.TechnicalEntityId == changed.AssignmentId
            && issue.RuleId == InitialStructureRuleDefinitions
                .NormalSlotFullCoverage.Id.Value
            && issue.Parameter == "DemandCoverageOrTimeChanged");
    }

    [Fact]
    public void OverlappingProtectedCoverageIdentifiesBothAssignmentsAndRule()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot first = PlanningInputTestFactory.CreateAssignment(
            slots[0],
            assignmentNumber: 1);
        ScheduleAssignmentSnapshot second = PlanningInputTestFactory.CreateAssignment(
            slots[0],
            assignmentNumber: 2);

        PlanningInputValidationResult result = CreateValidator().Validate(
            PlanningInputTestFactory.Create(
                demandSlots: slots,
                assignments:
                [
                    first,
                    second,
                    .. PlanningInputTestFactory.CreateAssignments(slots).Skip(1),
                ]));

        string ruleId = InitialAutomaticHardRuleDefinitions
            .AutomaticNoOverstaffing.Id.Value;
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ProtectedAssignmentConflict
            && issue.TechnicalEntityId == first.AssignmentId
            && issue.RuleId == ruleId
            && issue.Parameter == "ProtectedCoverageOverlap");
        Assert.Contains(result.Issues, issue =>
            issue.Code == PlanningInputValidationCode.ProtectedAssignmentConflict
            && issue.TechnicalEntityId == second.AssignmentId
            && issue.RuleId == ruleId
            && issue.Parameter == "ProtectedCoverageOverlap");
    }

    private PlanningInputValidator CreateValidator() => new(registry);

    private static RuleDefinitionSnapshot Clone(
        RuleDefinitionSnapshot source,
        string? id = null,
        string? descriptionKey = null) => new(
            id ?? source.Id,
            source.Family,
            source.Scope,
            source.AutomaticEffect,
            source.ManualEffect,
            source.Priority,
            source.Parameters,
            descriptionKey ?? source.DescriptionKey);
}
