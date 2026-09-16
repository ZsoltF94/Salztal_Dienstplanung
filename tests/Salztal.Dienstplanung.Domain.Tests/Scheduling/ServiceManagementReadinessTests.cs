using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

public sealed class ServiceManagementReadinessTests
{
    private static readonly Employee Employee = CreateEmployee();

    [Fact]
    public void EvaluateCountsNormalOfficeAndReliefPatternAsOneValidEntryPerWeek()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment normal = context.CreateNormalAssignment(
            Employee.Id,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        DemandSlot officeSlot = context.FindSlot(
            context.Period.StartMonday.AddDays(7),
            InitialShiftTypeCatalog.LateShift);
        ScheduleAssignment office = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateOfficeTime(
                Guid.NewGuid(),
                Employee.Id,
                officeSlot).Value);
        DateOnly thirdSaturday = context.Period.StartMonday.AddDays(19);
        DemandSlot cafeteria = context.FindSlot(
            thirdSaturday,
            InitialShiftTypeCatalog.CafeteriaShiftB);
        DemandSlot late = context.FindSlot(
            thirdSaturday,
            InitialShiftTypeCatalog.LateShift);
        ScheduleAssignment relief = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateReliefShift(
                Guid.NewGuid(),
                Employee.Id,
                InitialShiftPatternCatalog.ReliefShift,
                cafeteria,
                late,
                AssignmentOrigin.ServiceManagement).Value);
        ScheduleDraft draft = context.CreateDraft(
            assignments: [normal, office, relief]);

        ServiceManagementReadiness readiness = Evaluate(
            draft,
            CreateWeeklyAvailabilities(context.Period, []));

        Assert.True(readiness.CanPrepare);
        Assert.All(
            readiness.Weeks,
            week => Assert.Equal(
                ServiceManagementWeekReadinessStatus.Ready,
                week.Status));
        Assert.Equal(normal.WorkMinutes, readiness.Weeks[0].WorkMinutes);
        Assert.Equal(office.WorkMinutes, readiness.Weeks[1].WorkMinutes);
        Assert.Equal(relief.WorkMinutes, readiness.Weeks[2].WorkMinutes);
    }

    [Fact]
    public void EvaluateCountsSplitShiftAsOneReadyDayPattern()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DemandSlot early = context.FindSlot(
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift);
        DemandSlot late = context.FindSlot(
            context.Period.StartMonday,
            InitialShiftTypeCatalog.LateShift);
        ScheduleAssignment split = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                Employee.Id,
                InitialShiftPatternCatalog.SplitShift,
                early,
                late,
                AssignmentOrigin.ServiceManagement).Value);
        ScheduleDraft draft = context.CreateDraft(assignments: [split]);

        ServiceManagementReadiness readiness = Evaluate(
            draft,
            CreateWeeklyAvailabilities(context.Period, []));

        Assert.Equal(
            ServiceManagementWeekReadinessStatus.Ready,
            readiness.Weeks[0].Status);
        Assert.Equal(split.WorkMinutes, readiness.Weeks[0].WorkMinutes);
        Assert.False(readiness.CanPrepare);
    }

    [Fact]
    public void EvaluateExemptsExactlySevenUnavailableDaysButNotPartialWeek()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        AvailabilityEntry[] entries = Enumerable.Range(0, 7)
            .Select(offset => CreateAvailabilityEntry(
                Employee.Id,
                context.Period.StartMonday.AddDays(offset),
                (offset % 3) switch
                {
                    0 => AvailabilityEntryKind.Vacation,
                    1 => AvailabilityEntryKind.Sickness,
                    _ => AvailabilityEntryKind.FixedDayOff,
                }))
            .Append(CreateAvailabilityEntry(
                Employee.Id,
                context.Period.StartMonday.AddDays(7),
                AvailabilityEntryKind.Vacation))
            .ToArray();
        ScheduleDraft draft = context.CreateDraft(availabilityEntries: entries);

        ServiceManagementReadiness readiness = Evaluate(
            draft,
            CreateWeeklyAvailabilities(context.Period, entries));

        Assert.Equal(
            ServiceManagementWeekReadinessStatus.ExemptFullyUnavailable,
            readiness.Weeks[0].Status);
        Assert.True(readiness.Weeks[0].IsFullyUnavailable);
        Assert.Equal(
            ServiceManagementWeekReadinessStatus.MissingAssignment,
            readiness.Weeks[1].Status);
        Assert.Equal(
            ServiceManagementWeekReadinessStatus.MissingAssignment,
            readiness.Weeks[2].Status);
        Assert.False(readiness.CanPrepare);
    }

    [Fact]
    public void EvaluateIgnoresNonServiceManagementAssignmentForRequirement()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment automatic = context.CreateNormalAssignment(
            Employee.Id,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        ScheduleDraft draft = context.CreateDraft(assignments: [automatic]);

        ServiceManagementReadiness readiness = Evaluate(
            draft,
            CreateWeeklyAvailabilities(context.Period, []));

        Assert.Equal(
            ServiceManagementWeekReadinessStatus.MissingAssignment,
            readiness.Weeks[0].Status);
        Assert.Equal(0, readiness.Weeks[0].WorkMinutes);
    }

    [Fact]
    public void EvaluateWithMissingDuplicateForeignAndOutsideWeekReturnsErrors()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleDraft draft = context.CreateDraft();
        WeeklyAvailability firstWeek = CalculateWeeklyAvailability(
            context.Period.StartMonday,
            []);
        Employee foreignEmployee = Assert.IsType<Employee>(
            Employee.Create(
                new Guid("30000000-0000-4000-8000-000000000099"),
                "Mira",
                "Beispiel",
                InitialEmployeeTypeCatalog.Type1.Id.Value).Value);
        WeeklyAvailability foreignWeek = CalculateWeeklyAvailability(
            foreignEmployee,
            context.Period.StartMonday.AddDays(7),
            []);
        WeeklyAvailability outsideWeek = CalculateWeeklyAvailability(
            context.Period.StartMonday.AddDays(21),
            []);

        ServiceManagementReadinessValidationResult result =
            ServiceManagementReadiness.Evaluate(
                draft,
                Employee.Id,
                [firstWeek, firstWeek, foreignWeek, outsideWeek]);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ServiceManagementReadinessValidationCode.DuplicateWeek);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ServiceManagementReadinessValidationCode.MissingWeek);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ServiceManagementReadinessValidationCode.EmployeeMismatch);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ServiceManagementReadinessValidationCode.WeekOutsidePeriod);
    }

    private static ServiceManagementReadiness Evaluate(
        ScheduleDraft draft,
        IEnumerable<WeeklyAvailability> weeklyAvailabilities)
    {
        return Assert.IsType<ServiceManagementReadiness>(
            ServiceManagementReadiness.Evaluate(
                draft,
                Employee.Id,
                weeklyAvailabilities).Value);
    }

    private static WeeklyAvailability[] CreateWeeklyAvailabilities(
        SchedulePeriod period,
        IEnumerable<AvailabilityEntry> entries)
    {
        AvailabilityEntrySet entrySet = Assert.IsType<AvailabilityEntrySet>(
            AvailabilityEntrySet.Create(entries).Value);

        return Enumerable.Range(0, 3)
            .Select(index => Assert.IsType<WeeklyAvailability>(
                WeeklyAvailability.Calculate(
                    Employee,
                    InitialEmployeeTypeCatalog.Type1,
                    period.StartMonday.AddDays(index * 7),
                    entrySet).Value))
            .ToArray();
    }

    private static WeeklyAvailability CalculateWeeklyAvailability(
        DateOnly weekMonday,
        IEnumerable<AvailabilityEntry> entries)
    {
        return CalculateWeeklyAvailability(Employee, weekMonday, entries);
    }

    private static WeeklyAvailability CalculateWeeklyAvailability(
        Employee employee,
        DateOnly weekMonday,
        IEnumerable<AvailabilityEntry> entries)
    {
        AvailabilityEntrySet entrySet = Assert.IsType<AvailabilityEntrySet>(
            AvailabilityEntrySet.Create(entries).Value);

        return Assert.IsType<WeeklyAvailability>(
            WeeklyAvailability.Calculate(
                employee,
                InitialEmployeeTypeCatalog.Type1,
                weekMonday,
                entrySet).Value);
    }

    private static AvailabilityEntry CreateAvailabilityEntry(
        EmployeeId employeeId,
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(employeeId.Value, date, kind).Value);
    }

    private static Employee CreateEmployee()
    {
        return Assert.IsType<Employee>(
            Employee.Create(
                new Guid("30000000-0000-4000-8000-000000000001"),
                "Tina",
                "Planung",
                InitialEmployeeTypeCatalog.Type1.Id.Value).Value);
    }
}
