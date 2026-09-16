using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class SetServiceManagementAssignmentCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IChangeScheduleDayStore _store;

    public SetServiceManagementAssignmentCommand(
        IScheduleWorkspaceReader reader,
        IChangeScheduleDayStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<ScheduleDayChangeResult> ExecuteAsync(
        SetServiceManagementAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(request.Kind))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.UnsupportedAssignmentKind,
                "Die ausgewählte Typ1-Einteilung wird nicht unterstützt.");
        }

        if (!Enum.IsDefined(request.DayEntryRemovalConfirmation))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentInvalid,
                "Der Bestätigungswert ist ungültig.");
        }

        SchedulePeriodValidationResult periodResult =
            SchedulePeriod.Create(request.PeriodMonday);
        if (!periodResult.IsSuccess)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.PeriodInvalid,
                "Der Planungszeitraum ist ungültig.");
        }

        ScheduleWorkspaceReadData data = await _reader.LoadAsync(
            periodResult.Value!,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        ScheduleDayCommandContextValidationResult contextResult =
            ScheduleDayCommandContext.Create(
                request.DraftId,
                request.ExpectedDraftVersion,
                request.PeriodMonday,
                data);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        ScheduleDayCommandContext context = contextResult.Value!;
        ScheduleDayChangeResult? employeeFailure = context.ValidateActiveEmployee(
            request.EmployeeId,
            out Employee? employee,
            out EmployeeType? employeeType);
        if (employeeFailure is not null)
        {
            return employeeFailure;
        }

        if (employeeType!.PlanningPolicy.Role
            != EmployeeTypePlanningRole.ServiceManagement)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.WrongPlanningRole,
                ScheduleDayChangeErrorCode.ServiceManagementRoleRequired,
                "Typ1-Dienste dürfen nur für die aktive Serviceleitungs-Person eingetragen werden.");
        }

        DemandSlotResolutionResult firstResult = context.ResolveSlot(request.FirstSlot);
        if (firstResult.Failure is not null)
        {
            return firstResult.Failure;
        }

        DemandSlot? secondSlot = null;
        if (RequiresSecondSlot(request.Kind))
        {
            DemandSlotResolutionResult secondResult = context.ResolveSlot(
                request.SecondSlot);
            if (secondResult.Failure is not null)
            {
                return secondResult.Failure;
            }

            secondSlot = secondResult.Value;
        }
        else if (request.SecondSlot is not null)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentInvalid,
                "Für diesen Dienst darf nur ein Bedarfsplatz ausgewählt werden.");
        }

        DemandSlot firstSlot = firstResult.Value!;
        ScheduleDayChangeResult? eligibilityFailure = ValidateEligibility(
            employeeType,
            request.Kind,
            firstSlot,
            context.ReadData.StaffingDemands.ServiceCatalog.SplitShiftPattern,
            context.ReadData.StaffingDemands.ServiceCatalog.ReliefShiftPattern);
        if (eligibilityFailure is not null)
        {
            return eligibilityFailure;
        }

        AvailabilityEntryReadItem? currentEntry = context.FindCurrentEntry(
            employee!.Id,
            firstSlot.Id.Date);
        if (!ScheduleDayCommandContext.MatchesExpectedAvailabilityVersion(
                currentEntry,
                request.ExpectedDayEntryChangeVersion))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.Conflict,
                ScheduleDayChangeErrorCode.AvailabilityVersionConflict,
                "Das Tageskennzeichen wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        if (currentEntry is not null
            && request.DayEntryRemovalConfirmation
                != ScheduleReplacementConfirmation.Confirmed)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ConfirmationRequired,
                ScheduleDayChangeErrorCode.ReplacementConfirmationRequired,
                "Bitte bestätigen Sie, dass das vorhandene Tageskennzeichen entfernt und durch den Typ1-Dienst ersetzt wird.");
        }

        ScheduleAssignment? currentAssignment = context.Draft.Assignments
            .SingleOrDefault(assignment =>
                assignment.EmployeeId == employee.Id
                && assignment.Date == firstSlot.Id.Date);
        if (currentAssignment is not null
            && currentAssignment.Origin != AssignmentOrigin.ServiceManagement)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.StoredDataInvalid,
                ScheduleDayChangeErrorCode.ServiceManagementAssignmentRequired,
                "Der vorhandene Dienst ist keine geschützte Typ1-Einteilung und kann hier nicht ersetzt werden.");
        }

        ScheduleAssignmentValidationResult assignmentResult = CreateAssignment(
            request.Kind,
            employee.Id,
            firstSlot,
            secondSlot,
            context.ReadData.StaffingDemands.ServiceCatalog.SplitShiftPattern,
            context.ReadData.StaffingDemands.ServiceCatalog.ReliefShiftPattern);
        if (!assignmentResult.IsSuccess)
        {
            return MapAssignmentFailure(assignmentResult.Errors);
        }

        AvailabilityEntrySet availabilityEntries = currentEntry is null
            ? context.Draft.AvailabilityEntries
            : AssertAvailabilitySet(
                context.Draft.AvailabilityEntries.Entries.Where(entry =>
                    entry.EmployeeId != employee.Id
                    || entry.Date != firstSlot.Id.Date));
        ScheduleAssignment assignment = assignmentResult.Value!;
        ScheduleDraftValidationResult draftResult;
        try
        {
            draftResult = ScheduleDayCommandContext.CreateUpdatedDraft(
                context.Draft,
                availabilityEntries,
                context.Draft.Assignments
                    .Where(candidate => candidate != currentAssignment)
                    .Append(assignment));
        }
        catch (OverflowException)
        {
            return ScheduleDayChangeCommandSupport.StoredDataInvalid();
        }

        if (!draftResult.IsSuccess)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentInvalid,
                "Die Typ1-Einteilung widerspricht dem aktuellen Entwurf, zum Beispiel durch eine bereits belegte Bedarfsdeckung.");
        }

        ScheduleAvailabilityMutation mutation = currentEntry is null
            ? ScheduleAvailabilityMutation.None()
            : ScheduleAvailabilityMutation.Remove(currentEntry);
        return await ScheduleDayChangeCommandSupport.StoreAsync(
            _store,
            context.Draft,
            draftResult.Value!,
            mutation,
            assignment.Id.Value,
            employee.Id.Value,
            assignment.Date,
            cancellationToken);
    }

    private static bool RequiresSecondSlot(
        ServiceManagementAssignmentSelectionKind kind)
    {
        return kind is ServiceManagementAssignmentSelectionKind.SplitShiftPattern
            or ServiceManagementAssignmentSelectionKind.ReliefShiftPattern;
    }

    private static ScheduleAssignmentValidationResult CreateAssignment(
        ServiceManagementAssignmentSelectionKind kind,
        EmployeeId employeeId,
        DemandSlot firstSlot,
        DemandSlot? secondSlot,
        SplitShiftPattern splitShiftPattern,
        ReliefShiftPattern reliefShiftPattern)
    {
        Guid assignmentId = Guid.NewGuid();
        return kind switch
        {
            ServiceManagementAssignmentSelectionKind.NormalDemand =>
                ScheduleAssignment.CreateNormal(
                    assignmentId,
                    employeeId,
                    firstSlot,
                    AssignmentOrigin.ServiceManagement),
            ServiceManagementAssignmentSelectionKind.OfficeTime =>
                ScheduleAssignment.CreateOfficeTime(
                    assignmentId,
                    employeeId,
                    firstSlot),
            ServiceManagementAssignmentSelectionKind.SplitShiftPattern =>
                ScheduleAssignment.CreateSplitShift(
                    assignmentId,
                    employeeId,
                    splitShiftPattern,
                    firstSlot,
                    secondSlot,
                    AssignmentOrigin.ServiceManagement),
            ServiceManagementAssignmentSelectionKind.ReliefShiftPattern =>
                ScheduleAssignment.CreateReliefShift(
                    assignmentId,
                    employeeId,
                    reliefShiftPattern,
                    firstSlot,
                    secondSlot,
                    AssignmentOrigin.ServiceManagement),
            _ => throw new InvalidOperationException(
                $"Unsupported service-management assignment kind: {kind}"),
        };
    }

    private static ScheduleDayChangeResult? ValidateEligibility(
        EmployeeType employeeType,
        ServiceManagementAssignmentSelectionKind kind,
        DemandSlot firstSlot,
        SplitShiftPattern splitShiftPattern,
        ReliefShiftPattern reliefShiftPattern)
    {
        if (kind is ServiceManagementAssignmentSelectionKind.NormalDemand
            or ServiceManagementAssignmentSelectionKind.OfficeTime)
        {
            bool eligible = employeeType.ShiftEligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
                && eligibility.ShiftTypeId == firstSlot.Id.ShiftTypeId
                && eligibility.Activation == ShiftEligibilityActivation.Always);
            return eligible
                ? null
                : ScheduleDayChangeResult.Failure(
                    ScheduleDayChangeStatus.ValidationFailed,
                    ScheduleDayChangeErrorCode.ShiftNotEligible,
                    "Der ausgewählte Dienst ist für Typ1 nicht freigegeben.");
        }

        ShiftPatternId patternId = kind
            == ServiceManagementAssignmentSelectionKind.SplitShiftPattern
            ? splitShiftPattern.Id
            : reliefShiftPattern.Id;
        bool patternEligible = employeeType.ShiftEligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
            && eligibility.ShiftPatternId == patternId
            && eligibility.Activation == ShiftEligibilityActivation.Always);
        return patternEligible
            ? null
            : ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.PatternNotEligible,
                "Das ausgewählte Dienstmuster ist für Typ1 nicht freigegeben.");
    }

    private static ScheduleDayChangeResult MapAssignmentFailure(
        IReadOnlyList<ScheduleAssignmentValidationError> errors)
    {
        ScheduleAssignmentValidationCode code = errors[0].Code;
        return code switch
        {
            ScheduleAssignmentValidationCode.OfficeTimeRequiresRestaurantEarlyOrLate =>
                ScheduleDayChangeResult.Failure(
                    ScheduleDayChangeStatus.ValidationFailed,
                    ScheduleDayChangeErrorCode.OfficeTimeNotAllowedForSlot,
                    "B ist nur für einen vorhandenen Restaurant-Früh- oder Restaurant-Spätdienst zulässig."),
            ScheduleAssignmentValidationCode.SlotsMustShareDate =>
                ScheduleDayChangeResult.Failure(
                    ScheduleDayChangeStatus.ValidationFailed,
                    ScheduleDayChangeErrorCode.SlotsMustShareDate,
                    "Beide Plätze eines Dienstmusters müssen am selben Tag liegen."),
            _ => ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentInvalid,
                "Die ausgewählten Bedarfsplätze bilden keinen gültigen Typ1-Dienst."),
        };
    }

    private static AvailabilityEntrySet AssertAvailabilitySet(
        IEnumerable<AvailabilityEntry> entries)
    {
        return AvailabilityEntrySet.Create(entries).Value
            ?? throw new InvalidOperationException(
                "Validated availability entries could not be reconstructed.");
    }
}
