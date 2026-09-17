using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.Stability;

internal enum FairDistributionSubject
{
    EarlyShift,
    LateShift,
    Weekend,
    SplitShift,
    ReliefShift,
}

internal sealed record FairDistributionCohort(
    FairDistributionSubject Subject,
    int OpportunityCount,
    IReadOnlyList<Guid> EmployeeIds,
    IntVar Spread);

internal sealed class FairDistributionRuleModel(
    IEnumerable<FairDistributionCohort> cohorts)
{
    public IReadOnlyList<FairDistributionCohort> Cohorts { get; } = cohorts.ToArray();

    public LinearExpr TotalSpread => LinearExpr.Sum(Cohorts.Select(item => item.Spread));
}

internal static class FairDistributionRuleModelBuilder
{
    public static FairDistributionRuleModel Apply(HardRulePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        List<FairDistributionCohort> cohorts = [];
        foreach (FairDistributionSubject subject in Enum.GetValues<FairDistributionSubject>())
        {
            Dictionary<Guid, PlanningAssignmentCandidate[]> opportunities = context.Snapshot
                .Employees
                .ToDictionary(
                    employee => employee.Id,
                    employee => context.StructuralModel.CandidateSet.Candidates
                        .Where(candidate => candidate.EmployeeId == employee.Id
                            && Matches(context.Snapshot, candidate, subject))
                        .ToArray());
            foreach (IGrouping<int, KeyValuePair<Guid, PlanningAssignmentCandidate[]>> group in
                     opportunities.Where(item => item.Value.Length > 0)
                         .GroupBy(item => OpportunityCount(item.Value, subject)))
            {
                Guid[] employeeIds = group.Select(item => item.Key).Order().ToArray();
                if (employeeIds.Length < 2)
                {
                    continue;
                }

                IntVar[] assignmentCounts = employeeIds.Select(employeeId =>
                {
                    PlanningAssignmentCandidate[] candidates = opportunities[employeeId];
                    IntVar count = context.StructuralModel.Model.NewIntVar(
                        0,
                        group.Key,
                        $"fair-count-{subject}-{group.Key}-{employeeId:N}");
                    context.StructuralModel.Model.Add(count == LinearExpr.Sum(
                        candidates.Select(candidate =>
                            context.StructuralModel.CandidateVariables[
                                candidate.TechnicalKey])));
                    return count;
                }).ToArray();
                IntVar maximum = context.StructuralModel.Model.NewIntVar(
                    0,
                    group.Key,
                    $"fair-max-{subject}-{group.Key}");
                IntVar minimum = context.StructuralModel.Model.NewIntVar(
                    0,
                    group.Key,
                    $"fair-min-{subject}-{group.Key}");
                IntVar spread = context.StructuralModel.Model.NewIntVar(
                    0,
                    group.Key,
                    $"fair-spread-{subject}-{group.Key}");
                context.StructuralModel.Model.AddMaxEquality(maximum, assignmentCounts);
                context.StructuralModel.Model.AddMinEquality(minimum, assignmentCounts);
                context.StructuralModel.Model.Add(spread == maximum - minimum);
                cohorts.Add(new FairDistributionCohort(
                    subject,
                    group.Key,
                    employeeIds,
                    spread));
            }
        }

        return new FairDistributionRuleModel(cohorts);
    }

    internal static bool Matches(
        PlanningInputSnapshot snapshot,
        PlanningAssignmentCandidate candidate,
        FairDistributionSubject subject) => subject switch
        {
            FairDistributionSubject.EarlyShift => candidate.Coverages.Any(coverage =>
                coverage.Demand.ShiftTypeId
                    == snapshot.ServiceCatalog.SplitShiftPattern.FirstShiftTypeId),
            FairDistributionSubject.LateShift => candidate.Coverages.Any(coverage =>
                coverage.Demand.ShiftTypeId
                    == snapshot.ServiceCatalog.SplitShiftPattern.SecondShiftTypeId),
            FairDistributionSubject.Weekend => candidate.Date.DayOfWeek is
                DayOfWeek.Saturday or DayOfWeek.Sunday,
            FairDistributionSubject.SplitShift =>
                candidate.Kind == PlanningCandidateKind.SplitShiftPattern,
            FairDistributionSubject.ReliefShift =>
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern,
            _ => false,
        };

    internal static int OpportunityCount(
        IEnumerable<PlanningAssignmentCandidate> candidates,
        FairDistributionSubject subject)
    {
        _ = subject;
        return candidates.Select(candidate => candidate.Date).Distinct().Count();
    }
}
