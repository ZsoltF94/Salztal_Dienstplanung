using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record AuxiliaryMinimumCaseModel(
    Guid EmployeeId,
    DateOnly WeekMonday,
    bool HasEligibleDemand,
    IntVar WeeklyMinutes,
    BoolVar IsViolated,
    IntVar MissingMinutes);

internal sealed record RelativeWeeklyTargetCaseModel(
    Guid EmployeeId,
    DateOnly WeekMonday,
    IntVar WeeklyMinutes,
    int TargetMinutes,
    IntVar AbsoluteDeviation,
    int MaximumDeviation);

internal sealed class JointWeeklyObjectiveModel(
    IEnumerable<AuxiliaryMinimumCaseModel> auxiliaryMinimumCases,
    IEnumerable<RelativeWeeklyTargetCaseModel> relativeTargetCases)
{
    public IReadOnlyList<AuxiliaryMinimumCaseModel> AuxiliaryMinimumCases { get; } =
        auxiliaryMinimumCases.ToArray();

    public IReadOnlyList<RelativeWeeklyTargetCaseModel> RelativeTargetCases { get; } =
        relativeTargetCases.ToArray();

    public LinearExpr AuxiliaryViolationCount => LinearExpr.Sum(
        AuxiliaryMinimumCases.Select(item => item.IsViolated));

    public LinearExpr AuxiliaryMissingMinutes => LinearExpr.Sum(
        AuxiliaryMinimumCases.Select(item => item.MissingMinutes));
}

internal static class JointWeeklyObjectiveModelBuilder
{
    public static JointWeeklyObjectiveModel Apply(HardRulePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        WeeklyMinutesMinimumParameters minimumParameters =
            context.GetParameters<WeeklyMinutesMinimumParameters>(
                CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum);
        RelativeWeeklyTargetParameters targetParameters =
            context.GetParameters<RelativeWeeklyTargetParameters>(
                CurrentSoftRuleDefinitions.RelativeWeeklyTarget);
        List<AuxiliaryMinimumCaseModel> minimumCases = [];
        List<RelativeWeeklyTargetCaseModel> targetCases = [];

        foreach (PlanningEmployeeSnapshot employee in context.Snapshot.Employees)
        {
            EmployeeTypePlanningRole role = context.GetType(employee.Id).PlanningRole;
            if (role is not EmployeeTypePlanningRole.Normal
                and not EmployeeTypePlanningRole.Auxiliary)
            {
                continue;
            }

            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly monday = context.Snapshot.PeriodMonday.AddDays(weekIndex * 7);
                IReadOnlyList<PlanningAssignmentCandidate> candidates = context
                    .CandidatesByEmployeeWeek.GetValueOrDefault((employee.Id, monday)) ?? [];
                int protectedMinutes = context.ProtectedWorkMinutes(employee.Id, monday);
                int maximumMinutes = checked(
                    protectedMinutes + candidates.Sum(candidate => candidate.WorkMinutes));
                IntVar weeklyMinutes = context.StructuralModel.Model.NewIntVar(
                    protectedMinutes,
                    maximumMinutes,
                    $"joint-weekly-minutes-{employee.Id:N}-{monday:yyyyMMdd}");
                context.StructuralModel.Model.Add(
                    weeklyMinutes == protectedMinutes + LinearExpr.WeightedSum(
                        candidates.Select(candidate => context.StructuralModel
                            .CandidateVariables[candidate.TechnicalKey]),
                        candidates.Select(candidate => (long)candidate.WorkMinutes)));

                if (role == minimumParameters.Role)
                {
                    bool hasEligibleDemand = candidates.Count > 0;
                    IntVar missing = context.StructuralModel.Model.NewIntVar(
                        0,
                        minimumParameters.MinimumMinutes,
                        $"aux-minimum-missing-{employee.Id:N}-{monday:yyyyMMdd}");
                    BoolVar violated = context.StructuralModel.Model.NewBoolVar(
                        $"aux-minimum-violated-{employee.Id:N}-{monday:yyyyMMdd}");
                    if (hasEligibleDemand || !minimumParameters.RequiresEligibleDemand)
                    {
                        context.StructuralModel.Model.AddMaxEquality(
                            missing,
                            [
                                minimumParameters.MinimumMinutes - weeklyMinutes,
                                LinearExpr.Constant(0),
                            ]);
                        context.StructuralModel.Model.Add(missing >= 1)
                            .OnlyEnforceIf(violated);
                        context.StructuralModel.Model.Add(missing == 0)
                            .OnlyEnforceIf(violated.Not());
                    }
                    else
                    {
                        context.StructuralModel.Model.Add(missing == 0);
                        context.StructuralModel.Model.Add(violated == 0);
                    }

                    minimumCases.Add(new AuxiliaryMinimumCaseModel(
                        employee.Id,
                        monday,
                        hasEligibleDemand,
                        weeklyMinutes,
                        violated,
                        missing));
                }

                int targetMinutes = role == EmployeeTypePlanningRole.Auxiliary
                    ? targetParameters.AuxiliaryTargetMinutes
                    : context.EffectiveWeeklyTargetMinutes(employee.Id, monday);
                int maximumDeviation = Math.Max(
                    targetMinutes,
                    Math.Abs(maximumMinutes - targetMinutes));
                IntVar deviation = context.StructuralModel.Model.NewIntVar(
                    0,
                    maximumDeviation,
                    $"relative-target-deviation-{employee.Id:N}-{monday:yyyyMMdd}");
                context.StructuralModel.Model.AddAbsEquality(
                    deviation,
                    weeklyMinutes - targetMinutes);
                targetCases.Add(new RelativeWeeklyTargetCaseModel(
                    employee.Id,
                    monday,
                    weeklyMinutes,
                    targetMinutes,
                    deviation,
                    maximumDeviation));
            }
        }

        return new JointWeeklyObjectiveModel(minimumCases, targetCases);
    }
}
