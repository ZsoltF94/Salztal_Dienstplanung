using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal sealed class HardRulePlanningContext
{
    private readonly Dictionary<Guid, PlanningEmployeeTypeSnapshot> _typesByEmployee;

    public HardRulePlanningContext(
        PlanningInputSnapshot snapshot,
        StructuralPlanningModel structuralModel,
        ReliefShiftEmergencyGate reliefShiftEmergencyGate)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(structuralModel);
        ArgumentNullException.ThrowIfNull(reliefShiftEmergencyGate);
        Snapshot = snapshot;
        StructuralModel = structuralModel;
        ReliefShiftEmergencyGate = reliefShiftEmergencyGate;

        Dictionary<Guid, PlanningEmployeeTypeSnapshot> types = snapshot.EmployeeTypes
            .ToDictionary(type => type.Id);
        _typesByEmployee = snapshot.Employees.ToDictionary(
            employee => employee.Id,
            employee => types[employee.EmployeeTypeId]);
        CandidatesByEmployeeWeek = structuralModel.CandidateSet.Candidates
            .GroupBy(candidate => (candidate.EmployeeId, WeekMonday(candidate.Date)))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PlanningAssignmentCandidate>)Array.AsReadOnly(
                    group.OrderBy(candidate => candidate.TechnicalKey, StringComparer.Ordinal)
                        .ToArray()));
        CandidatesByEmployeeDate = structuralModel.CandidateSet.Candidates
            .GroupBy(candidate => (candidate.EmployeeId, candidate.Date))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PlanningAssignmentCandidate>)Array.AsReadOnly(
                    group.OrderBy(candidate => candidate.TechnicalKey, StringComparer.Ordinal)
                        .ToArray()));
    }

    public PlanningInputSnapshot Snapshot { get; }

    public StructuralPlanningModel StructuralModel { get; }

    public ReliefShiftEmergencyGate ReliefShiftEmergencyGate { get; }

    public IReadOnlyDictionary<
        (Guid EmployeeId, DateOnly WeekMonday),
        IReadOnlyList<PlanningAssignmentCandidate>> CandidatesByEmployeeWeek
    { get; }

    public IReadOnlyDictionary<
        (Guid EmployeeId, DateOnly Date),
        IReadOnlyList<PlanningAssignmentCandidate>> CandidatesByEmployeeDate
    { get; }

    public PlanningEmployeeTypeSnapshot GetType(Guid employeeId) =>
        _typesByEmployee[employeeId];

    public TParameters GetParameters<TParameters>(RuleDefinition definition)
        where TParameters : RuleParameters
    {
        ArgumentNullException.ThrowIfNull(definition);
        RuleDefinitionSnapshot snapshotDefinition = Snapshot.RuleCatalog.Definitions.Single(
            item => item.Id == definition.Id.Value);
        return (TParameters)snapshotDefinition.Parameters;
    }

    public int EffectiveWeeklyTargetMinutes(
        Guid employeeId,
        DateOnly weekMonday)
    {
        PlanningEmployeeTypeSnapshot type = GetType(employeeId);
        int absenceMinutes = Snapshot.AvailabilityEntries
            .Where(entry => entry.EmployeeId == employeeId)
            .Where(entry => entry.Date >= weekMonday
                && entry.Date <= weekMonday.AddDays(6))
            .Where(entry => entry.Kind is AvailabilityEntryKind.Vacation
                or AvailabilityEntryKind.Sickness)
            .Sum(_ => type.AbsenceDayValueMinutes ?? 0);
        return Math.Max(0, type.WeeklyWorkTargetMinutes - absenceMinutes);
    }

    public int ProtectedWorkMinutes(Guid employeeId, DateOnly weekMonday) =>
        Snapshot.ServiceManagementAssignments
            .Where(assignment => assignment.EmployeeId == employeeId)
            .Where(assignment => assignment.Date >= weekMonday
                && assignment.Date <= weekMonday.AddDays(6))
            .Sum(assignment => assignment.WorkMinutes);

    public bool HasProtectedWork(Guid employeeId, DateOnly date) =>
        Snapshot.ServiceManagementAssignments.Any(assignment =>
            assignment.EmployeeId == employeeId
            && assignment.Date == date
            && assignment.WorkMinutes > 0);

    public static DateOnly WeekMonday(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}
