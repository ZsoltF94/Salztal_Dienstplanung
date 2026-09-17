using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal static class ScheduleAssignmentDisplayFormatter
{
    public static ScheduleAssignmentDisplay Create(
        ScheduleAssignmentSnapshot assignment,
        IReadOnlyCollection<ScheduleDemandSlotSnapshot> demandSlots)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(demandSlots);
        return assignment.Kind switch
        {
            ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                new ScheduleAssignmentDisplay("D", CreateMeaning("Doppeldienst", assignment)),
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                new ScheduleAssignmentDisplay("Spr", CreateMeaning("Springerdienst", assignment)),
            ScheduleAssignmentKindSnapshot.OfficeTime =>
                new ScheduleAssignmentDisplay(
                    "B",
                    CreateMeaning("Bürozeit; der zugrunde liegende Bedarf bleibt offen", assignment)),
            ScheduleAssignmentKindSnapshot.NormalDemand =>
                CreateNormalDisplay(assignment, demandSlots),
            ScheduleAssignmentKindSnapshot.ManualAdditional =>
                new ScheduleAssignmentDisplay("+", CreateMeaning("zusätzliche Einteilung", assignment)),
            _ => new ScheduleAssignmentDisplay("?", "unbekannte Einteilung"),
        };
    }

    private static ScheduleAssignmentDisplay CreateNormalDisplay(
        ScheduleAssignmentSnapshot assignment,
        IReadOnlyCollection<ScheduleDemandSlotSnapshot> demandSlots)
    {
        ScheduleDemandCoverageSnapshot? coverage = assignment.Coverages.FirstOrDefault();
        ScheduleDemandSlotSnapshot? slot = coverage is null
            ? null
            : demandSlots.SingleOrDefault(candidate =>
                candidate.SourceId == coverage.DemandSourceId
                && candidate.Date == coverage.Date
                && candidate.WorkLocationId == coverage.WorkLocationId
                && candidate.ShiftTypeId == coverage.ShiftTypeId
                && candidate.Ordinal == coverage.Ordinal);
        ScheduleAssignmentSegmentSnapshot? segment = assignment.Segments.FirstOrDefault();
        string display = slot?.ShiftTypeDisplayKind
            == ScheduleShiftDisplayKindSnapshot.Abbreviation
            && !string.IsNullOrWhiteSpace(slot.ShiftTypeAbbreviation)
                ? slot.ShiftTypeAbbreviation
                : segment?.ActualStart.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "Dienst";
        string description = slot is null
            ? "Dienst"
            : $"{slot.ShiftTypeName} am Einsatzort {slot.WorkLocationName}";
        return new ScheduleAssignmentDisplay(
            display,
            CreateMeaning(description, assignment));
    }

    private static string CreateMeaning(
        string description,
        ScheduleAssignmentSnapshot assignment)
    {
        string origin = assignment.Origin switch
        {
            ScheduleAssignmentOriginSnapshot.AutomaticGeneration => "automatisch geplant",
            ScheduleAssignmentOriginSnapshot.ServiceManagement => "von Typ1 vorgetragen",
            ScheduleAssignmentOriginSnapshot.ManualEdit => "manuell eingetragen",
            _ => "mit unbekannter Herkunft",
        };
        return $"{description}, {origin}";
    }
}

internal sealed record ScheduleAssignmentDisplay(string Text, string Meaning);
