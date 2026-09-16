using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal sealed record ScheduleDayCommandContext(
    ScheduleWorkspaceReadData ReadData,
    ScheduleWorkspaceContext Workspace,
    ScheduleDraft Draft)
{
    public static ScheduleDayCommandContextValidationResult Create(
        Guid draftId,
        long expectedDraftVersion,
        DateOnly periodMonday,
        ScheduleWorkspaceReadData data)
    {
        if (!ScheduleDraftId.TryCreate(draftId, out _))
        {
            return Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.DraftIdentifierRequired,
                "Der ausgewählte Entwurf besitzt keine gültige Kennung.");
        }

        if (expectedDraftVersion <= 0 || expectedDraftVersion > int.MaxValue)
        {
            return Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.ExpectedDraftVersionInvalid,
                "Der erwartete Änderungsstand des Entwurfs ist ungültig.");
        }

        SchedulePeriodValidationResult periodResult = SchedulePeriod.Create(periodMonday);
        if (!periodResult.IsSuccess)
        {
            return Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.PeriodInvalid,
                "Der Planungszeitraum muss an einem Montag beginnen und genau 21 Tage umfassen können.");
        }

        SchedulePeriod period = periodResult.Value!;
        ScheduleDraftHeader[] headers = data.DraftHeaders
            .Where(header => header.Period == period)
            .ToArray();
        if (headers.Length == 0 || data.ExactDraft is null)
        {
            return Failure(
                ScheduleDayChangeStatus.NotFound,
                ScheduleDayChangeErrorCode.DraftNotFound,
                "Für den ausgewählten Zeitraum ist kein Entwurf vorhanden.");
        }

        if (headers.Length != 1
            || data.ExactDraft.Id != headers[0].DraftId
            || data.ExactDraft.Version != headers[0].Version
            || data.ExactDraft.Period != period)
        {
            return Failure(
                ScheduleDayChangeStatus.StoredDataInvalid,
                ScheduleDayChangeErrorCode.StoredDataInvalid,
                "Der gespeicherte Entwurf passt nicht eindeutig zum ausgewählten Zeitraum.");
        }

        if (data.ExactDraft.Id.Value != draftId)
        {
            return Failure(
                ScheduleDayChangeStatus.NotFound,
                ScheduleDayChangeErrorCode.DraftNotFound,
                "Der ausgewählte Entwurf ist nicht mehr vorhanden.");
        }

        ScheduleWorkspaceContextResult workspaceResult =
            ScheduleWorkspaceContextBuilder.Build(period, data);
        if (!workspaceResult.IsSuccess)
        {
            bool catalogInvalid =
                workspaceResult.Status == OpenOrCreateScheduleDraftStatus.CatalogInvalid;
            return Failure(
                catalogInvalid
                    ? ScheduleDayChangeStatus.CatalogInvalid
                    : ScheduleDayChangeStatus.StoredDataInvalid,
                catalogInvalid
                    ? ScheduleDayChangeErrorCode.CatalogInvalid
                    : ScheduleDayChangeErrorCode.StoredDataInvalid,
                workspaceResult.Errors[0].Message);
        }

        if (data.ExactDraft.Version.Value != expectedDraftVersion)
        {
            return Failure(
                ScheduleDayChangeStatus.Conflict,
                ScheduleDayChangeErrorCode.Conflict,
                "Der Planungsstand wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        return new ScheduleDayCommandContextValidationResult(
            new ScheduleDayCommandContext(
                data,
                workspaceResult.Value!,
                data.ExactDraft),
            null);
    }

    public ScheduleDayChangeResult? ValidateActiveEmployee(
        Guid employeeId,
        out Employee? employee,
        out EmployeeType? employeeType)
    {
        employee = null;
        employeeType = null;

        if (!EmployeeId.TryCreate(employeeId, out EmployeeId? validatedEmployeeId))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.EmployeeIdentifierRequired,
                "Die ausgewählte Person besitzt keine gültige Kennung.");
        }

        employee = ReadData.Availability.Employees.SingleOrDefault(candidate =>
            candidate.Id == validatedEmployeeId);
        if (employee is null)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.NotFound,
                ScheduleDayChangeErrorCode.EmployeeNotFound,
                "Die ausgewählte Person ist nicht vorhanden.");
        }

        if (!employee.IsActive)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.InactiveEmployee,
                ScheduleDayChangeErrorCode.EmployeeInactive,
                "Für eine inaktive Person kann der Dienstplan nicht geändert werden.");
        }

        employeeType = Workspace.EmployeeTypes[employee.EmployeeTypeId];
        return null;
    }

    public AvailabilityEntryReadItem? FindCurrentEntry(
        EmployeeId employeeId,
        DateOnly date)
    {
        return ReadData.Availability.Entries.SingleOrDefault(item =>
            item.Entry.EmployeeId == employeeId && item.Entry.Date == date);
    }

    public static bool MatchesExpectedAvailabilityVersion(
        AvailabilityEntryReadItem? current,
        long? expectedVersion)
    {
        return current?.ChangeVersion == expectedVersion;
    }

    public static ScheduleDraftValidationResult CreateUpdatedDraft(
        ScheduleDraft current,
        AvailabilityEntrySet availabilityEntries,
        IEnumerable<ScheduleAssignment> assignments)
    {
        return ScheduleDraft.Create(
            current.Id.Value,
            checked(current.Version.Value + 1),
            current.Period,
            current.DemandSlots,
            availabilityEntries,
            assignments,
            current.GeneratedDayOffMarkers,
            current.AssignmentLocks);
    }

    public DemandSlotResolutionResult ResolveSlot(
        ScheduleDemandSlotSelection? selection)
    {
        if (selection is null)
        {
            return DemandSlotResolutionResult.Reject(
                ScheduleDayChangeErrorCode.DemandSlotRequired,
                "Für die Einteilung muss ein vorhandener Bedarfsplatz ausgewählt werden.");
        }

        DemandSlot? slot = Draft.DemandSlots.Slots.SingleOrDefault(candidate =>
            candidate.Id.SourceId.Value == selection.SourceId
            && MapSourceKind(candidate.Id.SourceKind) == selection.SourceKind
            && candidate.Id.Date == selection.Date
            && candidate.Id.WorkLocationId.Value == selection.WorkLocationId
            && candidate.Id.ShiftTypeId.Value == selection.ShiftTypeId
            && candidate.Id.Ordinal == selection.Ordinal);
        if (slot is null)
        {
            return DemandSlotResolutionResult.Reject(
                ScheduleDayChangeErrorCode.DemandSlotNotFound,
                "Der ausgewählte Bedarfsplatz ist nicht mehr vorhanden.");
        }

        if (slot.ActualTime.Start != selection.ActualStart
            || slot.ActualTime.End != selection.ActualEnd)
        {
            return DemandSlotResolutionResult.Reject(
                ScheduleDayChangeErrorCode.DemandSlotTimeChanged,
                "Die tatsächliche Zeit des ausgewählten Bedarfsplatzes hat sich geändert.");
        }

        return new DemandSlotResolutionResult(slot, null);
    }

    private static ScheduleDemandSourceKindSnapshot MapSourceKind(
        StaffingDemandSourceKind sourceKind)
    {
        return sourceKind switch
        {
            StaffingDemandSourceKind.Standard =>
                ScheduleDemandSourceKindSnapshot.Standard,
            StaffingDemandSourceKind.DateException =>
                ScheduleDemandSourceKindSnapshot.DateException,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing demand source kind: {sourceKind}"),
        };
    }

    private static ScheduleDayCommandContextValidationResult Failure(
        ScheduleDayChangeStatus status,
        ScheduleDayChangeErrorCode code,
        string message)
    {
        return new ScheduleDayCommandContextValidationResult(
            null,
            ScheduleDayChangeResult.Failure(status, code, message));
    }
}

internal sealed record ScheduleDayCommandContextValidationResult(
    ScheduleDayCommandContext? Value,
    ScheduleDayChangeResult? Failure);

internal sealed record DemandSlotResolutionResult(
    DemandSlot? Value,
    ScheduleDayChangeResult? Failure)
{
    public static DemandSlotResolutionResult Reject(
        ScheduleDayChangeErrorCode code,
        string message)
    {
        return new DemandSlotResolutionResult(
            null,
            ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                code,
                message));
    }
}
