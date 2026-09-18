using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class AutomaticSchedulePlanningReport
{
    internal AutomaticSchedulePlanningReport(
        Guid snapshotId,
        Guid draftId,
        long draftVersion,
        DateOnly periodMonday,
        DateOnly periodSunday,
        IEnumerable<PlanningDemandCoverageSnapshot> demands,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns,
        IEnumerable<PlanningEmployeeWeekSnapshot> employeeWeeks)
    {
        if (snapshotId == Guid.Empty || draftId == Guid.Empty)
        {
            throw new ArgumentException("Report binding identifiers are required.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(draftVersion);
        ArgumentNullException.ThrowIfNull(demands);
        ArgumentNullException.ThrowIfNull(serviceColumns);
        ArgumentNullException.ThrowIfNull(employeeWeeks);
        if (periodMonday.DayOfWeek != DayOfWeek.Monday
            || periodSunday != periodMonday.AddDays(20))
        {
            throw new ArgumentException(
                "A planning report must cover exactly three Monday-to-Sunday weeks.");
        }

        PlanningDemandCoverageSnapshot[] demandValues = demands.ToArray();
        PlanningServiceColumnSnapshot[] serviceColumnValues = serviceColumns.ToArray();
        PlanningEmployeeWeekSnapshot[] employeeWeekValues = employeeWeeks.ToArray();
        if (demandValues.Any(value => value is null)
            || serviceColumnValues.Any(value => value is null)
            || employeeWeekValues.Any(value => value is null))
        {
            throw new ArgumentException("Report collections cannot contain null values.");
        }

        demandValues = demandValues
            .OrderBy(value => value.Date)
            .ThenBy(value => value.WorkLocationName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.DemandStart)
            .ThenBy(value => value.ShiftTypeName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.Ordinal)
            .ToArray();

        if (demandValues
            .Select(value => new
            {
                value.DemandSourceId,
                value.Date,
                value.WorkLocationId,
                value.ShiftTypeId,
                value.Ordinal,
            })
            .Distinct()
            .Count() != demandValues.Length)
        {
            throw new ArgumentException(
                "Planning report demands must be unique.",
                nameof(demands));
        }

        if (serviceColumnValues
            .Select(value => (value.Kind, value.ShiftTypeId, value.Code))
            .Distinct()
            .Count() != serviceColumnValues.Length
            || employeeWeekValues.Any(week =>
                week.ServiceCounts.Count != serviceColumnValues.Length
                || week.ServiceCounts.Where((count, index) =>
                    count.Kind != serviceColumnValues[index].Kind
                    || count.ShiftTypeId != serviceColumnValues[index].ShiftTypeId
                    || count.Code != serviceColumnValues[index].Code).Any()))
        {
            throw new ArgumentException(
                "Employee service counts must match the stable report columns.");
        }

        SnapshotId = snapshotId;
        DraftId = draftId;
        DraftVersion = draftVersion;
        PeriodMonday = periodMonday;
        PeriodSunday = periodSunday;
        Demands = Array.AsReadOnly(demandValues.ToArray());
        DemandWeeks = Array.AsReadOnly(Enumerable.Range(0, 3)
            .Select(index =>
            {
                DateOnly weekMonday = periodMonday.AddDays(index * 7);
                DateOnly weekSunday = weekMonday.AddDays(6);
                return new PlanningDemandSummarySnapshot(
                    weekMonday,
                    weekSunday,
                    demandValues.Where(value =>
                        value.Date >= weekMonday && value.Date <= weekSunday));
            })
            .ToArray());
        DemandTotal = new PlanningDemandSummarySnapshot(
            periodMonday,
            periodSunday,
            demandValues);
        ServiceColumns = Array.AsReadOnly(serviceColumnValues);
        EmployeeWeeks = Array.AsReadOnly(employeeWeekValues.ToArray());
        EmployeeWeekSummaries = Array.AsReadOnly(Enumerable.Range(0, 3)
            .Select(index =>
            {
                DateOnly weekMonday = periodMonday.AddDays(index * 7);
                return new PlanningEmployeeWeekSummarySnapshot(
                    weekMonday,
                    serviceColumnValues,
                    employeeWeekValues.Where(value =>
                        value.WeekMonday == weekMonday));
            })
            .ToArray());
    }

    public Guid SnapshotId { get; }

    public Guid DraftId { get; }

    public long DraftVersion { get; }

    public DateOnly PeriodMonday { get; }

    public DateOnly PeriodSunday { get; }

    public ReadOnlyCollection<PlanningDemandCoverageSnapshot> Demands { get; }

    public ReadOnlyCollection<PlanningDemandSummarySnapshot> DemandWeeks { get; }

    public PlanningDemandSummarySnapshot DemandTotal { get; }

    public ReadOnlyCollection<PlanningServiceColumnSnapshot> ServiceColumns { get; }

    public ReadOnlyCollection<PlanningEmployeeWeekSnapshot> EmployeeWeeks { get; }

    public ReadOnlyCollection<PlanningEmployeeWeekSummarySnapshot> EmployeeWeekSummaries
    {
        get;
    }
}

public static class AutomaticSchedulePlanningReportCalculator
{
    public static AutomaticSchedulePlanningReport Create(
        PlanningInputSnapshot input,
        AutomaticScheduleProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(proposal);
        ValidateBinding(input, proposal);
        return Create(
            input,
            proposal.Assignments,
            proposal.OpenDemands);
    }

    internal static AutomaticSchedulePlanningReport CreateAccepted(
        PlanningInputSnapshot input,
        IEnumerable<ScheduleAssignmentSnapshot> automaticAssignments)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(automaticAssignments);
        ScheduleAssignmentSnapshot[] values = automaticAssignments.ToArray();
        if (values.Any(value => value is null
            || value.Origin != ScheduleAssignmentOriginSnapshot.AutomaticGeneration))
        {
            throw new ArgumentException(
                "Accepted report assignments must be automatic assignments.",
                nameof(automaticAssignments));
        }

        return Create(input, values, null);
    }

    private static AutomaticSchedulePlanningReport Create(
        PlanningInputSnapshot input,
        IEnumerable<ScheduleAssignmentSnapshot> automaticAssignments,
        IEnumerable<AutomaticScheduleOpenDemand>? reportedOpenDemands)
    {
        ScheduleAssignmentSnapshot[] assignments = input.ServiceManagementAssignments
            .Concat(automaticAssignments)
            .ToArray();
        if (assignments.Select(value => value.AssignmentId).Distinct().Count()
            != assignments.Length)
        {
            throw new InvalidOperationException(
                "The report source contains duplicate assignment identifiers.");
        }

        PlanningDemandCoverageSnapshot[] demands = CreateDemandCoverage(
            input,
            assignments,
            reportedOpenDemands);
        PlanningServiceColumnSnapshot[] serviceColumns = CreateServiceColumns(
            input,
            assignments);
        PlanningEmployeeWeekSnapshot[] employees = CreateEmployeeWeeks(
            input,
            assignments,
            serviceColumns);
        return new AutomaticSchedulePlanningReport(
            input.Id,
            input.DraftId,
            input.DraftVersion,
            input.PeriodMonday,
            input.PeriodSunday,
            demands,
            serviceColumns,
            employees);
    }

    private static void ValidateBinding(
        PlanningInputSnapshot input,
        AutomaticScheduleProposal proposal)
    {
        if (proposal.SnapshotId != input.Id
            || proposal.DraftId != input.DraftId
            || proposal.ExpectedDraftVersion != input.DraftVersion)
        {
            throw new InvalidOperationException(
                "The report proposal does not match its planning input snapshot.");
        }
    }

    private static PlanningDemandCoverageSnapshot[] CreateDemandCoverage(
        PlanningInputSnapshot input,
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        IEnumerable<AutomaticScheduleOpenDemand>? reportedOpenDemands)
    {
        Dictionary<DemandIdentity, List<CoverageInterval>> coverageByDemand = [];
        foreach (ScheduleDemandCoverageSnapshot coverage in assignments
            .SelectMany(value => value.Coverages))
        {
            DemandIdentity key = DemandIdentity.From(coverage);
            if (!coverageByDemand.TryGetValue(key, out List<CoverageInterval>? intervals))
            {
                intervals = [];
                coverageByDemand.Add(key, intervals);
            }

            intervals.Add(new CoverageInterval(
                coverage.CoveredStart,
                coverage.CoveredEnd,
                coverage.CoveredMinutes,
                coverage.Kind));
        }

        Dictionary<DemandIdentity, ScheduleDemandSlotSnapshot> demandsByIdentity = input
            .DemandSlots
            .ToDictionary(DemandIdentity.From);
        if (coverageByDemand.Keys.Any(key => !demandsByIdentity.ContainsKey(key)))
        {
            throw new InvalidOperationException(
                "An assignment covers an unknown demand slot.");
        }

        PlanningDemandCoverageSnapshot[] result = input.DemandSlots
            .OrderBy(value => value.Date)
            .ThenBy(value => value.WorkLocationName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.ActualStart)
            .ThenBy(value => value.ShiftTypeName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.Ordinal)
            .Select(demand => CreateDemandCoverage(
                demand,
                coverageByDemand.GetValueOrDefault(DemandIdentity.From(demand), [])))
            .ToArray();
        if (reportedOpenDemands is not null)
        {
            ValidateOpenDemands(
                reportedOpenDemands,
                input.DemandSlots,
                coverageByDemand);
        }

        return result;
    }

    private static PlanningDemandCoverageSnapshot CreateDemandCoverage(
        ScheduleDemandSlotSnapshot demand,
        IEnumerable<CoverageInterval> coverage)
    {
        if (demand.ActualEnd <= demand.ActualStart
            || demand.DurationMinutes
                != (int)(demand.ActualEnd - demand.ActualStart).TotalMinutes)
        {
            throw new InvalidOperationException(
                "Demand duration does not match its actual time range.");
        }

        CoverageInterval[] intervals = coverage
            .OrderBy(value => value.Start)
            .ThenBy(value => value.End)
            .ToArray();
        foreach (CoverageInterval interval in intervals)
        {
            if (interval.Start < demand.ActualStart
                || interval.End > demand.ActualEnd
                || interval.End <= interval.Start
                || interval.Minutes != (int)(interval.End - interval.Start).TotalMinutes)
            {
                throw new InvalidOperationException(
                    "Demand coverage lies outside the referenced demand slot.");
            }
        }

        PlanningDemandIntervalSnapshot[] coveredIntervals = MergeCoveredIntervals(intervals);
        int coveredMinutes = coveredIntervals.Sum(value => value.Minutes);
        if (coveredMinutes > demand.DurationMinutes)
        {
            throw new InvalidOperationException(
                "Demand coverage exceeds the referenced demand slot.");
        }

        int openMinutes = demand.DurationMinutes - coveredMinutes;
        PlanningDemandCoverageStatus status = openMinutes == 0
            ? PlanningDemandCoverageStatus.FullyCovered
            : coveredMinutes == 0
                ? PlanningDemandCoverageStatus.Uncovered
                : intervals.Any(value => value.Kind
                    == ScheduleDemandCoverageKindSnapshot.PartialReliefShift)
                    ? PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift
                    : throw new InvalidOperationException(
                        "Partial demand coverage is only valid for a relief shift.");
        PlanningDemandIntervalSnapshot[] openIntervals = CreateOpenIntervals(
            demand.ActualStart,
            demand.ActualEnd,
            coveredIntervals);
        return new PlanningDemandCoverageSnapshot(
            demand.SourceId,
            demand.Date,
            demand.WorkLocationId,
            demand.WorkLocationName,
            demand.ShiftTypeId,
            demand.ShiftTypeName,
            demand.Ordinal,
            demand.ActualStart,
            demand.ActualEnd,
            coveredIntervals,
            openIntervals,
            status);
    }

    private static PlanningDemandIntervalSnapshot[] MergeCoveredIntervals(
        IReadOnlyList<CoverageInterval> intervals)
    {
        if (intervals.Count == 0)
        {
            return [];
        }

        List<PlanningDemandIntervalSnapshot> result = [];
        TimeOnly start = intervals[0].Start;
        TimeOnly end = intervals[0].End;
        for (int index = 1; index < intervals.Count; index++)
        {
            CoverageInterval current = intervals[index];
            if (current.Start < end)
            {
                throw new InvalidOperationException(
                    "Demand coverage intervals must not overlap.");
            }

            if (current.Start == end)
            {
                end = current.End;
                continue;
            }

            result.Add(new PlanningDemandIntervalSnapshot(start, end));
            start = current.Start;
            end = current.End;
        }

        result.Add(new PlanningDemandIntervalSnapshot(start, end));
        return result.ToArray();
    }

    private static PlanningDemandIntervalSnapshot[] CreateOpenIntervals(
        TimeOnly demandStart,
        TimeOnly demandEnd,
        IReadOnlyCollection<PlanningDemandIntervalSnapshot> coveredIntervals)
    {
        List<PlanningDemandIntervalSnapshot> result = [];
        TimeOnly cursor = demandStart;
        foreach (PlanningDemandIntervalSnapshot interval in coveredIntervals)
        {
            if (interval.Start > cursor)
            {
                result.Add(new PlanningDemandIntervalSnapshot(cursor, interval.Start));
            }

            cursor = interval.End;
        }

        if (cursor < demandEnd)
        {
            result.Add(new PlanningDemandIntervalSnapshot(cursor, demandEnd));
        }

        return result.ToArray();
    }

    private static void ValidateOpenDemands(
        IEnumerable<AutomaticScheduleOpenDemand> openDemands,
        IEnumerable<ScheduleDemandSlotSnapshot> demands,
        IReadOnlyDictionary<DemandIdentity, List<CoverageInterval>> coverageByDemand)
    {
        ReportedOpenInterval[] reported = openDemands
            .Select(value => new ReportedOpenInterval(
                DemandIdentity.From(value),
                value.UncoveredStart,
                value.UncoveredEnd,
                value.UncoveredMinutes,
                value.Kind))
            .OrderBy(value => value.Demand.Date)
            .ThenBy(value => value.Demand.WorkLocationId)
            .ThenBy(value => value.Demand.ShiftTypeId)
            .ThenBy(value => value.Demand.SourceId)
            .ThenBy(value => value.Demand.Ordinal)
            .ThenBy(value => value.Start)
            .ToArray();
        ReportedOpenInterval[] calculated = demands
            .SelectMany(demand => CreateOpenIntervals(
                demand,
                coverageByDemand.GetValueOrDefault(DemandIdentity.From(demand), [])))
            .OrderBy(value => value.Demand.Date)
            .ThenBy(value => value.Demand.WorkLocationId)
            .ThenBy(value => value.Demand.ShiftTypeId)
            .ThenBy(value => value.Demand.SourceId)
            .ThenBy(value => value.Demand.Ordinal)
            .ThenBy(value => value.Start)
            .ToArray();
        if (!reported.SequenceEqual(calculated))
        {
            throw new InvalidOperationException(
                "The reported open demand does not match the proposal assignments.");
        }
    }

    private static IEnumerable<ReportedOpenInterval> CreateOpenIntervals(
        ScheduleDemandSlotSnapshot demand,
        IEnumerable<CoverageInterval> coverage)
    {
        CoverageInterval[] ordered = coverage
            .OrderBy(value => value.Start)
            .ThenBy(value => value.End)
            .ToArray();
        TimeOnly cursor = demand.ActualStart;
        List<(TimeOnly Start, TimeOnly End)> open = [];
        foreach (CoverageInterval interval in ordered)
        {
            if (interval.Start > cursor)
            {
                open.Add((cursor, interval.Start));
            }

            if (interval.End > cursor)
            {
                cursor = interval.End;
            }
        }

        if (cursor < demand.ActualEnd)
        {
            open.Add((cursor, demand.ActualEnd));
        }

        AutomaticScheduleOpenDemandKind kind = open.Count == 1
            && open[0].Start == demand.ActualStart
            && open[0].End == demand.ActualEnd
                ? AutomaticScheduleOpenDemandKind.FullyUncovered
                : AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift;
        DemandIdentity identity = DemandIdentity.From(demand);
        return open.Select(value => new ReportedOpenInterval(
            identity,
            value.Start,
            value.End,
            (int)(value.End - value.Start).TotalMinutes,
            kind));
    }

    private static PlanningEmployeeWeekSnapshot[] CreateEmployeeWeeks(
        PlanningInputSnapshot input,
        IReadOnlyCollection<ScheduleAssignmentSnapshot> assignments,
        IReadOnlyCollection<PlanningServiceColumnSnapshot> serviceColumns)
    {
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> types = input.EmployeeTypes
            .ToDictionary(value => value.Id);
        Dictionary<Guid, PlanningEmployeeSnapshot> employees = input.Employees
            .ToDictionary(value => value.Id);
        if (assignments.Any(value => !employees.ContainsKey(value.EmployeeId)))
        {
            throw new InvalidOperationException(
                "The report source contains an assignment for an unknown employee.");
        }

        DateOnly[] weekMondays = Enumerable.Range(0, 3)
            .Select(index => input.PeriodMonday.AddDays(index * 7))
            .ToArray();
        return input.Employees
            .Select(employee =>
            {
                if (!types.TryGetValue(
                    employee.EmployeeTypeId,
                    out PlanningEmployeeTypeSnapshot? employeeType))
                {
                    throw new InvalidOperationException(
                        "The report source contains an unknown employee type.");
                }

                return (Employee: employee, EmployeeType: employeeType);
            })
            .OrderBy(value => RoleOrder(value.EmployeeType.PlanningRole))
            .ThenBy(value => value.Employee.LastName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.Employee.FirstName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.Employee.Id)
            .SelectMany(value => weekMondays.Select(weekMonday => CreateEmployeeWeek(
                input,
                value.Employee,
                value.EmployeeType,
                weekMonday,
                assignments,
                serviceColumns)))
            .ToArray();
    }

    private static PlanningEmployeeWeekSnapshot CreateEmployeeWeek(
        PlanningInputSnapshot input,
        PlanningEmployeeSnapshot employee,
        PlanningEmployeeTypeSnapshot employeeType,
        DateOnly weekMonday,
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        IReadOnlyCollection<PlanningServiceColumnSnapshot> serviceColumns)
    {
        DateOnly weekSunday = weekMonday.AddDays(6);
        PlanningAvailabilityEntrySnapshot[] absences = input.AvailabilityEntries
            .Where(value => value.EmployeeId == employee.Id
                && value.Date >= weekMonday
                && value.Date <= weekSunday)
            .ToArray();
        int vacationCount = absences.Count(value =>
            value.Kind == AvailabilityEntryKind.Vacation);
        int sicknessCount = absences.Count(value =>
            value.Kind == AvailabilityEntryKind.Sickness);
        int effectiveTarget = employeeType.AbsenceDayValueMinutes is int dayValue
            ? Math.Max(
                0,
                employeeType.WeeklyWorkTargetMinutes
                    - checked((vacationCount + sicknessCount) * dayValue))
            : employeeType.WeeklyWorkTargetMinutes;
        ScheduleAssignmentSnapshot[] weekAssignments = assignments
            .Where(value => value.EmployeeId == employee.Id
                && value.Date >= weekMonday
                && value.Date <= weekSunday)
            .ToArray();
        return new PlanningEmployeeWeekSnapshot(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employeeType.Code,
            employeeType.Name,
            EmployeeTypePlanningRoleMapper.ToSnapshotKind(employeeType.PlanningRole),
            weekMonday,
            employeeType.WeeklyWorkTargetMinutes,
            effectiveTarget,
            weekAssignments.Aggregate(
                0,
                (total, value) => value.WorkMinutes < 0
                    ? throw new InvalidOperationException(
                        "Planned work minutes cannot be negative.")
                    : checked(total + value.WorkMinutes)),
            vacationCount,
            sicknessCount,
            CreateServiceCounts(input, weekAssignments, serviceColumns),
            CreateComparableShiftTypeIds(input, employee, employeeType, weekMonday));
    }

    private static IEnumerable<PlanningServiceCountSnapshot> CreateServiceCounts(
        PlanningInputSnapshot input,
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        IEnumerable<PlanningServiceColumnSnapshot> serviceColumns)
    {
        Dictionary<Guid, PlanningShiftTypeSnapshot> shifts = input.ServiceCatalog
            .ShiftTypes
            .ToDictionary(value => value.Id);
        Dictionary<ServiceCountIdentity, int> counts = assignments
            .Select(assignment => ServiceCountIdentity.Create(assignment, shifts))
            .GroupBy(value => value)
            .ToDictionary(group => group.Key, group => group.Count());
        return serviceColumns.Select(column => new PlanningServiceCountSnapshot(
            column.Kind,
            column.ShiftTypeId,
            column.Code,
            counts.GetValueOrDefault(new ServiceCountIdentity(
                column.Kind,
                column.ShiftTypeId,
                column.Code))));
    }

    private static PlanningServiceColumnSnapshot[] CreateServiceColumns(
        PlanningInputSnapshot input,
        IEnumerable<ScheduleAssignmentSnapshot> assignments)
    {
        Dictionary<Guid, PlanningShiftTypeSnapshot> shifts = input.ServiceCatalog
            .ShiftTypes
            .ToDictionary(value => value.Id);
        PlanningServiceColumnSnapshot[] normalColumns = input.DemandSlots
            .Select(value => value.ShiftTypeId)
            .Distinct()
            .Select(shiftTypeId => CreateShiftColumn(
                PlanningServiceCountKind.NormalShift,
                shifts[shiftTypeId]))
            .OrderBy(value => value.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.ShiftTypeId)
            .ToArray();
        PlanningServiceColumnSnapshot[] manualColumns = assignments
            .Where(value => value.Kind == ScheduleAssignmentKindSnapshot.ManualAdditional)
            .SelectMany(value => value.Segments.Select(segment => segment.ShiftTypeId))
            .Distinct()
            .Select(shiftTypeId => CreateShiftColumn(
                PlanningServiceCountKind.ManualAdditional,
                shifts[shiftTypeId]))
            .OrderBy(value => value.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value.ShiftTypeId)
            .ToArray();
        return
        [
            .. normalColumns,
            new PlanningServiceColumnSnapshot(
                PlanningServiceCountKind.SplitShift,
                null,
                "D",
                "Doppeldienst"),
            new PlanningServiceColumnSnapshot(
                PlanningServiceCountKind.ReliefShift,
                null,
                "Spr",
                "Springer-Einsatz"),
            new PlanningServiceColumnSnapshot(
                PlanningServiceCountKind.OfficeTime,
                null,
                "B",
                "Bürozeit"),
            .. manualColumns,
        ];
    }

    private static PlanningServiceColumnSnapshot CreateShiftColumn(
        PlanningServiceCountKind kind,
        PlanningShiftTypeSnapshot shift)
    {
        string code = string.IsNullOrWhiteSpace(shift.Abbreviation)
            ? shift.Name
            : shift.Abbreviation;
        string name = kind == PlanningServiceCountKind.ManualAdditional
            ? $"{shift.Name} – manuell zusätzlich"
            : shift.Name;
        return new PlanningServiceColumnSnapshot(kind, shift.Id, code, name);
    }

    private static IEnumerable<Guid> CreateComparableShiftTypeIds(
        PlanningInputSnapshot input,
        PlanningEmployeeSnapshot employee,
        PlanningEmployeeTypeSnapshot employeeType,
        DateOnly weekMonday)
    {
        if (!employeeType.AllowsAutomaticAssignment)
        {
            return [];
        }

        DateOnly weekSunday = weekMonday.AddDays(6);
        HashSet<DateOnly> unavailableDates = input.AvailabilityEntries
            .Where(value => value.EmployeeId == employee.Id)
            .Where(value => value.Date >= weekMonday && value.Date <= weekSunday)
            .Select(value => value.Date)
            .ToHashSet();
        HashSet<Guid> eligibleShiftTypeIds = employeeType.Eligibilities
            .Where(value => value.TargetKind == ShiftEligibilityTargetKind.ShiftType)
            .Where(value => value.Mode == ShiftEligibilityMode.Regular)
            .Where(value => value.Activation == ShiftEligibilityActivation.Always)
            .Select(value => value.TargetId)
            .ToHashSet();
        return input.DemandSlots
            .Where(value => value.Date >= weekMonday && value.Date <= weekSunday)
            .Where(value => eligibleShiftTypeIds.Contains(value.ShiftTypeId))
            .Where(value => !unavailableDates.Contains(value.Date))
            .Select(value => value.ShiftTypeId)
            .Distinct()
            .Order();
    }

    private static int RoleOrder(EmployeeTypePlanningRole role) => role switch
    {
        EmployeeTypePlanningRole.ServiceManagement => 0,
        EmployeeTypePlanningRole.Normal => 1,
        EmployeeTypePlanningRole.Auxiliary => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    private readonly record struct DemandIdentity(
        Guid SourceId,
        DateOnly Date,
        Guid WorkLocationId,
        Guid ShiftTypeId,
        int Ordinal)
    {
        internal static DemandIdentity From(ScheduleDemandSlotSnapshot value) => new(
            value.SourceId,
            value.Date,
            value.WorkLocationId,
            value.ShiftTypeId,
            value.Ordinal);

        internal static DemandIdentity From(ScheduleDemandCoverageSnapshot value) => new(
            value.DemandSourceId,
            value.Date,
            value.WorkLocationId,
            value.ShiftTypeId,
            value.Ordinal);

        internal static DemandIdentity From(AutomaticScheduleOpenDemand value) => new(
            value.SourceId,
            value.Date,
            value.WorkLocationId,
            value.ShiftTypeId,
            value.Ordinal);

        internal static DemandIdentity From(PlanningDemandCoverageSnapshot value) => new(
            value.DemandSourceId,
            value.Date,
            value.WorkLocationId,
            value.ShiftTypeId,
            value.Ordinal);
    }

    private sealed record CoverageInterval(
        TimeOnly Start,
        TimeOnly End,
        int Minutes,
        ScheduleDemandCoverageKindSnapshot Kind);

    private sealed record ReportedOpenInterval(
        DemandIdentity Demand,
        TimeOnly Start,
        TimeOnly End,
        int Minutes,
        AutomaticScheduleOpenDemandKind Kind);

    private readonly record struct ServiceCountIdentity(
        PlanningServiceCountKind Kind,
        Guid? ShiftTypeId,
        string Code)
    {
        internal static ServiceCountIdentity Create(
            ScheduleAssignmentSnapshot assignment,
            IReadOnlyDictionary<Guid, PlanningShiftTypeSnapshot> shifts) =>
            assignment.Kind switch
            {
                ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                    new(PlanningServiceCountKind.SplitShift, null, "D"),
                ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                    new(PlanningServiceCountKind.ReliefShift, null, "Spr"),
                ScheduleAssignmentKindSnapshot.OfficeTime =>
                    new(PlanningServiceCountKind.OfficeTime, null, "B"),
                ScheduleAssignmentKindSnapshot.NormalDemand =>
                    CreateShift(PlanningServiceCountKind.NormalShift, assignment, shifts),
                ScheduleAssignmentKindSnapshot.ManualAdditional =>
                    CreateShift(PlanningServiceCountKind.ManualAdditional, assignment, shifts),
                _ => throw new ArgumentOutOfRangeException(nameof(assignment)),
            };

        private static ServiceCountIdentity CreateShift(
            PlanningServiceCountKind kind,
            ScheduleAssignmentSnapshot assignment,
            IReadOnlyDictionary<Guid, PlanningShiftTypeSnapshot> shifts)
        {
            Guid[] shiftIds = assignment.Segments
                .Select(value => value.ShiftTypeId)
                .Distinct()
                .ToArray();
            if (shiftIds.Length != 1
                || !shifts.TryGetValue(
                    shiftIds[0],
                    out PlanningShiftTypeSnapshot? shift))
            {
                throw new InvalidOperationException(
                    "A counted assignment must reference exactly one known shift type.");
            }

            string code = string.IsNullOrWhiteSpace(shift.Abbreviation)
                ? shift.Name
                : shift.Abbreviation;
            return new ServiceCountIdentity(kind, shift.Id, code);
        }
    }
}
