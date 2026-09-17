using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ServiceManagementAssignmentOptionViewModel
{
    public ServiceManagementAssignmentOptionViewModel(
        ServiceManagementAssignmentOptionSnapshot snapshot,
        bool isCurrent,
        IAsyncRelayCommand<ServiceManagementAssignmentOptionViewModel> selectCommand)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(selectCommand);
        Snapshot = snapshot;
        IsCurrent = isCurrent;
        SelectCommand = selectCommand;
    }

    public ServiceManagementAssignmentOptionSnapshot Snapshot { get; }

    public bool IsCurrent { get; }

    public IAsyncRelayCommand<ServiceManagementAssignmentOptionViewModel> SelectCommand
    { get; }

    public string Display => Snapshot.Kind switch
    {
        ServiceManagementAssignmentSelectionKind.NormalDemand =>
            CreateSlotDisplay(Snapshot.FirstSlot),
        ServiceManagementAssignmentSelectionKind.OfficeTime =>
            $"B – {CreateSlotDisplay(Snapshot.FirstSlot)} (Bedarf bleibt offen)",
        ServiceManagementAssignmentSelectionKind.SplitShiftPattern =>
            $"D – {CreateCompositeDisplay(Snapshot)}",
        ServiceManagementAssignmentSelectionKind.ReliefShiftPattern =>
            $"Spr – {CreateCompositeDisplay(Snapshot)}",
        _ => throw new InvalidOperationException(
            $"Unsupported service-management option: {Snapshot.Kind}"),
    };

    private static string CreateCompositeDisplay(
        ServiceManagementAssignmentOptionSnapshot snapshot)
    {
        ScheduleDemandSlotSnapshot second = snapshot.SecondSlot
            ?? throw new InvalidOperationException("Composite option has no second slot.");
        return $"{CreateSlotDisplay(snapshot.FirstSlot)} + {CreateSlotDisplay(second)}";
    }

    private static string CreateSlotDisplay(ScheduleDemandSlotSnapshot slot)
    {
        return string.Create(
            CultureInfo.GetCultureInfo("de-DE"),
            $"{slot.WorkLocationName}: {slot.ShiftTypeName} "
            + $"{slot.ActualStart:HH\\:mm}–{slot.ActualEnd:HH\\:mm} "
            + $"(Platz {slot.Ordinal})");
    }
}
