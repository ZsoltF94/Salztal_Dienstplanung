using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class ScheduleCellViewModelTests
{
    [Fact]
    public void AutomaticNormalAssignmentUsesConfiguredShiftDisplay()
    {
        Guid sourceId = new("e441fbd1-39ef-4533-a50c-3e81828891b6");
        Guid locationId = new("e33718bb-9a35-45f6-bc5a-2409ff446c41");
        Guid shiftTypeId = new("a61bd7be-d9c3-44d6-a0a2-fc7c59d59bf7");
        DateOnly date = new(2026, 9, 21);
        ScheduleDemandSlotSnapshot slot = new(
            sourceId,
            ScheduleDemandSourceKindSnapshot.Standard,
            date,
            locationId,
            "Restaurant",
            shiftTypeId,
            "Frühdienst",
            1,
            new TimeOnly(6, 30),
            new TimeOnly(13, 30),
            420,
            ScheduleShiftDisplayKindSnapshot.Abbreviation,
            "F");
        ScheduleAssignmentSnapshot assignment = new(
            new Guid("43b22a46-1f6d-4361-85ef-28b55bbd68ea"),
            new Guid("052cf071-b442-47f0-ae20-af2f8dd553f0"),
            date,
            ScheduleAssignmentKindSnapshot.NormalDemand,
            ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
            null,
            420,
            false,
            [new ScheduleAssignmentSegmentSnapshot(
                sourceId,
                date,
                locationId,
                shiftTypeId,
                new TimeOnly(6, 30),
                new TimeOnly(13, 30),
                420)],
            [new ScheduleDemandCoverageSnapshot(
                sourceId,
                date,
                locationId,
                shiftTypeId,
                1,
                new TimeOnly(6, 30),
                new TimeOnly(13, 30),
                420,
                ScheduleDemandCoverageKindSnapshot.Full)]);

        ScheduleAssignmentDisplay display =
            ScheduleAssignmentDisplayFormatter.Create(assignment, [slot]);

        Assert.Equal("F", display.Text);
        Assert.Contains("Frühdienst", display.Meaning);
        Assert.Contains("automatisch geplant", display.Meaning);

        AsyncRelayCommand<ServiceManagementAssignmentOptionViewModel> command =
            new(_ => Task.CompletedTask);
        ServiceManagementAssignmentOptionViewModel option = new(
            new ServiceManagementAssignmentOptionSnapshot(
                assignment.EmployeeId,
                ServiceManagementAssignmentSelectionKind.NormalDemand,
                slot,
                null),
            false,
            command);
        ScheduleCellViewModel cell = new(
            assignment.EmployeeId,
            "Sarah Leitung",
            date,
            true,
            true,
            null,
            null,
            false,
            assignment,
            display,
            [option]);

        Assert.False(cell.CanOpenAssignmentEditor);
    }

    [Fact]
    public void GeneratedDayOffUsesBlackXWithTextualMeaning()
    {
        ScheduleCellViewModel cell = new(
            new Guid("5c213af6-5550-46a2-99af-d6503e1058fc"),
            "Erika Muster",
            new DateOnly(2026, 9, 22),
            true,
            false,
            null,
            null,
            true,
            null,
            null,
            []);

        Assert.Equal("X", cell.EntryDisplay);
        Assert.True(cell.IsGeneratedDayOff);
        Assert.Contains("schwarzes X", cell.EntryMeaning);
        Assert.Contains("schwarzes X", cell.AutomationName);
    }
}
