using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Planning.Candidates;

internal static class PlanningCandidateBuilder
{
    public static PlanningCandidateSet Build(PlanningInputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        Dictionary<Guid, PlanningEmployeeTypeSnapshot> employeeTypes =
            snapshot.EmployeeTypes.ToDictionary(type => type.Id);
        PlanningEmployeeSnapshot[] employees = snapshot.Employees
            .Where(employee => employeeTypes[employee.EmployeeTypeId]
                .AllowsAutomaticAssignment)
            .OrderBy(employee => employee.Id)
            .ToArray();
        ScheduleDemandSlotSnapshot[] slots = snapshot.DemandSlots
            .OrderBy(slot => PlanningDemandKey.From(slot))
            .ToArray();
        HashSet<(Guid EmployeeId, DateOnly Date)> unavailable =
            snapshot.AvailabilityEntries
                .Select(entry => (entry.EmployeeId, entry.Date))
                .ToHashSet();
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages =
            CreateProtectedCoverages(snapshot.ServiceManagementAssignments);

        List<PlanningAssignmentCandidate> candidates = [];
        AddNormalCandidates(
            candidates,
            employees,
            employeeTypes,
            slots,
            unavailable,
            protectedCoverages);
        AddSplitShiftCandidates(
            candidates,
            employees,
            employeeTypes,
            slots,
            unavailable,
            protectedCoverages,
            snapshot.ServiceCatalog.SplitShiftPattern);
        AddReliefShiftCandidates(
            candidates,
            employees,
            employeeTypes,
            slots,
            unavailable,
            protectedCoverages,
            snapshot.ServiceCatalog.ReliefShiftPattern,
            snapshot.RunOptions);

        return new PlanningCandidateSet(
            candidates,
            CreateRemainingDemands(slots, protectedCoverages));
    }

    private static void AddNormalCandidates(
        List<PlanningAssignmentCandidate> candidates,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> employeeTypes,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        HashSet<(Guid EmployeeId, DateOnly Date)> unavailable,
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages)
    {
        foreach (PlanningEmployeeSnapshot employee in employees)
        {
            PlanningEmployeeTypeSnapshot type = employeeTypes[employee.EmployeeTypeId];
            foreach (ScheduleDemandSlotSnapshot slot in slots)
            {
                PlanningDemandKey demand = PlanningDemandKey.From(slot);
                if (unavailable.Contains((employee.Id, slot.Date))
                    || !HasRegularAlwaysEligibility(
                        type,
                        ShiftEligibilityTargetKind.ShiftType,
                        slot.ShiftTypeId)
                    || !IsCoverageAvailable(
                        demand,
                        slot.ActualStart,
                        slot.ActualEnd,
                        protectedCoverages))
                {
                    continue;
                }

                candidates.Add(new PlanningAssignmentCandidate(
                    CreateTechnicalKey("normal", employee.Id, null, [demand]),
                    employee.Id,
                    slot.Date,
                    PlanningCandidateKind.NormalDemand,
                    null,
                    [CreateFullSegment(slot)],
                    [CreateFullCoverage(slot)]));
            }
        }
    }

    private static void AddSplitShiftCandidates(
        List<PlanningAssignmentCandidate> candidates,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> employeeTypes,
        ScheduleDemandSlotSnapshot[] slots,
        HashSet<(Guid EmployeeId, DateOnly Date)> unavailable,
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages,
        PlanningSplitShiftPatternSnapshot pattern)
    {
        ScheduleDemandSlotSnapshot[] firstSlots = slots
            .Where(slot => slot.WorkLocationId == pattern.WorkLocationId)
            .Where(slot => slot.ShiftTypeId == pattern.FirstShiftTypeId)
            .ToArray();
        ScheduleDemandSlotSnapshot[] secondSlots = slots
            .Where(slot => slot.WorkLocationId == pattern.WorkLocationId)
            .Where(slot => slot.ShiftTypeId == pattern.SecondShiftTypeId)
            .ToArray();

        foreach (PlanningEmployeeSnapshot employee in employees)
        {
            PlanningEmployeeTypeSnapshot type = employeeTypes[employee.EmployeeTypeId];
            if (!HasRegularAlwaysEligibility(
                    type,
                    ShiftEligibilityTargetKind.ShiftPattern,
                    pattern.Id))
            {
                continue;
            }

            foreach (ScheduleDemandSlotSnapshot first in firstSlots)
            {
                foreach (ScheduleDemandSlotSnapshot second in secondSlots.Where(second =>
                             second.Date == first.Date
                             && first.ActualEnd < second.ActualStart))
                {
                    PlanningDemandKey firstDemand = PlanningDemandKey.From(first);
                    PlanningDemandKey secondDemand = PlanningDemandKey.From(second);
                    if (unavailable.Contains((employee.Id, first.Date))
                        || !IsCoverageAvailable(
                            firstDemand,
                            first.ActualStart,
                            first.ActualEnd,
                            protectedCoverages)
                        || !IsCoverageAvailable(
                            secondDemand,
                            second.ActualStart,
                            second.ActualEnd,
                            protectedCoverages))
                    {
                        continue;
                    }

                    candidates.Add(new PlanningAssignmentCandidate(
                        CreateTechnicalKey(
                            "split",
                            employee.Id,
                            pattern.Id,
                            [firstDemand, secondDemand]),
                        employee.Id,
                        first.Date,
                        PlanningCandidateKind.SplitShiftPattern,
                        pattern.Id,
                        [CreateFullSegment(first), CreateFullSegment(second)],
                        [CreateFullCoverage(first), CreateFullCoverage(second)]));
                }
            }
        }
    }

    private static void AddReliefShiftCandidates(
        List<PlanningAssignmentCandidate> candidates,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> employeeTypes,
        ScheduleDemandSlotSnapshot[] slots,
        HashSet<(Guid EmployeeId, DateOnly Date)> unavailable,
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages,
        PlanningReliefShiftPatternSnapshot pattern,
        PlanningRunOptions runOptions)
    {
        ScheduleDemandSlotSnapshot[] firstSlots = slots
            .Where(slot => slot.WorkLocationId == pattern.FirstWorkLocationId)
            .Where(slot => slot.ShiftTypeId == pattern.FirstShiftTypeId)
            .Where(slot => slot.Date.DayOfWeek == pattern.AllowedDay)
            .ToArray();
        ScheduleDemandSlotSnapshot[] secondSlots = slots
            .Where(slot => slot.WorkLocationId == pattern.SecondWorkLocationId)
            .Where(slot => slot.ShiftTypeId == pattern.SecondShiftTypeId)
            .ToArray();

        foreach (PlanningEmployeeSnapshot employee in employees)
        {
            PlanningEmployeeTypeSnapshot type = employeeTypes[employee.EmployeeTypeId];
            if (!HasRegularAlwaysEligibility(
                    type,
                    ShiftEligibilityTargetKind.ShiftType,
                    pattern.FirstShiftTypeId)
                || !HasRegularAlwaysEligibility(
                    type,
                    ShiftEligibilityTargetKind.ShiftType,
                    pattern.SecondShiftTypeId)
                || !HasEnabledPatternEligibility(type, pattern.Id, runOptions))
            {
                continue;
            }

            foreach (ScheduleDemandSlotSnapshot first in firstSlots)
            {
                foreach (ScheduleDemandSlotSnapshot second in secondSlots.Where(second =>
                             second.Date == first.Date
                             && first.ActualEnd > second.ActualStart
                             && first.ActualEnd < second.ActualEnd))
                {
                    PlanningDemandKey firstDemand = PlanningDemandKey.From(first);
                    PlanningDemandKey secondDemand = PlanningDemandKey.From(second);
                    if (unavailable.Contains((employee.Id, first.Date))
                        || !IsCoverageAvailable(
                            firstDemand,
                            first.ActualStart,
                            first.ActualEnd,
                            protectedCoverages)
                        || !IsCoverageAvailable(
                            secondDemand,
                            first.ActualEnd,
                            second.ActualEnd,
                            protectedCoverages))
                    {
                        continue;
                    }

                    int secondMinutes = MinutesBetween(first.ActualEnd, second.ActualEnd);
                    candidates.Add(new PlanningAssignmentCandidate(
                        CreateTechnicalKey(
                            "relief",
                            employee.Id,
                            pattern.Id,
                            [firstDemand, secondDemand]),
                        employee.Id,
                        first.Date,
                        PlanningCandidateKind.ReliefShiftPattern,
                        pattern.Id,
                        [
                            CreateFullSegment(first),
                            new PlanningCandidateSegment(
                                secondDemand,
                                first.ActualEnd,
                                second.ActualEnd,
                                secondMinutes),
                        ],
                        [
                            CreateFullCoverage(first),
                            new PlanningCandidateCoverage(
                                secondDemand,
                                first.ActualEnd,
                                second.ActualEnd,
                                secondMinutes),
                        ]));
                }
            }
        }
    }

    private static bool HasRegularAlwaysEligibility(
        PlanningEmployeeTypeSnapshot type,
        ShiftEligibilityTargetKind targetKind,
        Guid targetId)
    {
        return type.Eligibilities.Any(eligibility =>
            eligibility.TargetKind == targetKind
            && eligibility.TargetId == targetId
            && eligibility.Mode == ShiftEligibilityMode.Regular
            && eligibility.Activation == ShiftEligibilityActivation.Always);
    }

    private static bool HasEnabledPatternEligibility(
        PlanningEmployeeTypeSnapshot type,
        Guid patternId,
        PlanningRunOptions runOptions)
    {
        return type.Eligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
            && eligibility.TargetId == patternId
            && eligibility.Mode == ShiftEligibilityMode.Regular
            && (eligibility.Activation == ShiftEligibilityActivation.Always
                || (eligibility.Activation
                        == ShiftEligibilityActivation.ExplicitPlanningRunOption
                    && runOptions.EnableAuxiliaryReliefShift)));
    }

    private static Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]>
        CreateProtectedCoverages(
            IEnumerable<ScheduleAssignmentSnapshot> assignments)
    {
        return assignments
            .SelectMany(assignment => assignment.Coverages)
            .Select(coverage => new PlanningCandidateCoverage(
                PlanningDemandKey.From(coverage),
                coverage.CoveredStart,
                coverage.CoveredEnd,
                coverage.CoveredMinutes))
            .GroupBy(coverage => coverage.Demand)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(coverage => coverage.CoveredStart).ToArray());
    }

    private static IEnumerable<PlanningRemainingDemand> CreateRemainingDemands(
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages)
    {
        foreach (ScheduleDemandSlotSnapshot slot in slots)
        {
            PlanningDemandKey demand = PlanningDemandKey.From(slot);
            if (!protectedCoverages.TryGetValue(
                    demand,
                    out PlanningCandidateCoverage[]? coverages))
            {
                yield return new PlanningRemainingDemand(
                    demand,
                    slot.ActualStart,
                    slot.ActualEnd,
                    slot.DurationMinutes,
                    PlanningRemainingDemandKind.FullyUncovered);
                continue;
            }

            TimeOnly firstCoveredStart = coverages.Min(coverage =>
                coverage.CoveredStart);
            if (firstCoveredStart > slot.ActualStart)
            {
                yield return new PlanningRemainingDemand(
                    demand,
                    slot.ActualStart,
                    firstCoveredStart,
                    MinutesBetween(slot.ActualStart, firstCoveredStart),
                    PlanningRemainingDemandKind.PartiallyUncoveredByProtectedReliefShift);
            }
        }
    }

    private static bool IsCoverageAvailable(
        PlanningDemandKey demand,
        TimeOnly start,
        TimeOnly end,
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage[]> protectedCoverages)
    {
        return !protectedCoverages.TryGetValue(
                demand,
                out PlanningCandidateCoverage[]? coverages)
            || coverages.All(coverage =>
                end <= coverage.CoveredStart || start >= coverage.CoveredEnd);
    }

    private static PlanningCandidateSegment CreateFullSegment(
        ScheduleDemandSlotSnapshot slot) => new(
            PlanningDemandKey.From(slot),
            slot.ActualStart,
            slot.ActualEnd,
            slot.DurationMinutes);

    private static PlanningCandidateCoverage CreateFullCoverage(
        ScheduleDemandSlotSnapshot slot) => new(
            PlanningDemandKey.From(slot),
            slot.ActualStart,
            slot.ActualEnd,
            slot.DurationMinutes);

    private static string CreateTechnicalKey(
        string kind,
        Guid employeeId,
        Guid? patternId,
        IEnumerable<PlanningDemandKey> demands)
    {
        string[] parts =
        [
            "candidate",
            kind,
            employeeId.ToString("N", CultureInfo.InvariantCulture),
            patternId?.ToString("N", CultureInfo.InvariantCulture) ?? "none",
            .. demands.Order().Select(demand => demand.TechnicalKey),
        ];
        return string.Join("_", parts);
    }

    private static int MinutesBetween(TimeOnly start, TimeOnly end) =>
        (int)(end - start).TotalMinutes;
}
