using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record MediumPriorityRuleCaseModel(
    RuleDefinition Rule,
    string CaseKey,
    BoolVar IsViolated,
    IntVar Magnitude);

internal sealed class MediumPriorityRuleModel(
    IEnumerable<MediumPriorityRuleCaseModel> cases)
{
    public IReadOnlyList<MediumPriorityRuleCaseModel> Cases { get; } = cases.ToArray();

    public LinearExpr ViolationCount => LinearExpr.Sum(Cases.Select(item => item.IsViolated));

    public LinearExpr Magnitude(RuleDefinition rule) => LinearExpr.Sum(
        Cases.Where(item => item.Rule.Id == rule.Id).Select(item => item.Magnitude));
}

internal static class MediumPriorityRuleModelBuilder
{
    public static MediumPriorityRuleModel Apply(HardRulePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        List<MediumPriorityRuleCaseModel> cases = [];
        AddThreeWeekFreeWeekend(context, cases);
        return new MediumPriorityRuleModel(cases);
    }

    private static void AddThreeWeekFreeWeekend(
        HardRulePlanningContext context,
        List<MediumPriorityRuleCaseModel> cases)
    {
        PlanningPeriodFreeWeekendParameters parameters =
            context.GetParameters<PlanningPeriodFreeWeekendParameters>(
                CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend);
        foreach (PlanningEmployeeSnapshot employee in context.Snapshot.Employees)
        {
            BoolVar[] freeWeekends = Enumerable.Range(0, parameters.PeriodWeeks)
                .Select(index =>
                {
                    DateOnly monday = context.Snapshot.PeriodMonday.AddDays(index * 7);
                    BoolVar saturday = DayOffVariable(
                        context,
                        employee.Id,
                        monday.AddDays(5));
                    BoolVar sunday = DayOffVariable(
                        context,
                        employee.Id,
                        monday.AddDays(6));
                    BoolVar weekend = context.StructuralModel.Model.NewBoolVar(
                        $"free-weekend-{employee.Id:N}-{monday:yyyyMMdd}");
                    context.StructuralModel.Model.Add(weekend <= saturday);
                    context.StructuralModel.Model.Add(weekend <= sunday);
                    context.StructuralModel.Model.Add(weekend >= saturday + sunday - 1);
                    return weekend;
                })
                .ToArray();
            LinearExpr raw = parameters.MinimumFreeWeekends
                - LinearExpr.Sum(freeWeekends);
            AddPositiveMagnitudeCase(
                context,
                cases,
                CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend,
                employee.Id.ToString("N"),
                parameters.MinimumFreeWeekends,
                raw);
        }
    }

    private static void AddPositiveMagnitudeCase(
        HardRulePlanningContext context,
        List<MediumPriorityRuleCaseModel> cases,
        RuleDefinition rule,
        string caseKey,
        int maximum,
        LinearExpr rawMagnitude)
    {
        IntVar magnitude = context.StructuralModel.Model.NewIntVar(
            0,
            maximum,
            $"magnitude-{rule.Id.Value}-{caseKey}");
        context.StructuralModel.Model.AddMaxEquality(
            magnitude,
            [rawMagnitude, LinearExpr.Constant(0)]);
        BoolVar violation = context.StructuralModel.Model.NewBoolVar(
            $"violated-{rule.Id.Value}-{caseKey}");
        context.StructuralModel.Model.Add(magnitude >= 1).OnlyEnforceIf(violation);
        context.StructuralModel.Model.Add(magnitude == 0).OnlyEnforceIf(violation.Not());
        cases.Add(new MediumPriorityRuleCaseModel(rule, caseKey, violation, magnitude));
    }

    private static BoolVar DayOffVariable(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly date)
    {
        AvailabilityEntryKind? marker = context.Snapshot.AvailabilityEntries
            .Where(entry => entry.EmployeeId == employeeId && entry.Date == date)
            .Select(entry => (AvailabilityEntryKind?)entry.Kind)
            .SingleOrDefault();
        BoolVar result = context.StructuralModel.Model.NewBoolVar(
            $"medium-day-off-{employeeId:N}-{date:yyyyMMdd}");
        if (marker == AvailabilityEntryKind.FixedDayOff)
        {
            context.StructuralModel.Model.Add(result == 1);
        }
        else if (marker is AvailabilityEntryKind.Vacation or AvailabilityEntryKind.Sickness
                 || context.HasProtectedWork(employeeId, date))
        {
            context.StructuralModel.Model.Add(result == 0);
        }
        else
        {
            IReadOnlyList<PlanningAssignmentCandidate> candidates = context
                .CandidatesByEmployeeDate.GetValueOrDefault((employeeId, date)) ?? [];
            context.StructuralModel.Model.Add(
                result + LinearExpr.Sum(candidates.Select(candidate =>
                    context.StructuralModel.CandidateVariables[candidate.TechnicalKey])) == 1);
        }

        return result;
    }
}
