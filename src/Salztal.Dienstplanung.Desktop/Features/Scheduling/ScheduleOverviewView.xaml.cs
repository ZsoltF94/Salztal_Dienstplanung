using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed partial class ScheduleOverviewView : UserControl
{
    private IInputElement? _confirmationRestoreTarget;

    public ScheduleOverviewView()
    {
        InitializeComponent();
    }

    private void FocusType1PopupOption(object sender, EventArgs eventArgs)
    {
        if (sender is not Popup { Child: DependencyObject content } popup)
        {
            return;
        }

        popup.Dispatcher.BeginInvoke(() =>
        {
            Button[] optionButtons = GetVisualDescendants(content)
                .OfType<Button>()
                .ToArray();
            Button? target = optionButtons.FirstOrDefault(button =>
                    button.DataContext
                        is ServiceManagementAssignmentOptionViewModel { IsCurrent: true })
                ?? optionButtons.FirstOrDefault();
            if (target is not null)
            {
                FocusManager.SetFocusedElement(content, target);
            }

            target?.Focus();
        }, DispatcherPriority.Input);
    }

    private void CloseType1PopupOnEscape(
        object sender,
        KeyEventArgs eventArgs)
    {
        if (eventArgs.Key != Key.Escape
            || sender is not FrameworkElement
            {
                DataContext: ScheduleCellViewModel cell,
            })
        {
            return;
        }

        cell.IsAssignmentEditorOpen = false;
        eventArgs.Handled = true;
    }

    private void ForwardPlanningMouseWheelToWorkspace(
        object sender,
        MouseWheelEventArgs eventArgs)
    {
        if (eventArgs.Delta == 0)
        {
            return;
        }

        int wheelScrollLines = SystemParameters.WheelScrollLines;
        if (wheelScrollLines == -1)
        {
            if (eventArgs.Delta < 0)
            {
                WorkspaceScrollViewer.PageDown();
            }
            else
            {
                WorkspaceScrollViewer.PageUp();
            }
        }
        else
        {
            for (int line = 0; line < wheelScrollLines; line++)
            {
                if (eventArgs.Delta < 0)
                {
                    WorkspaceScrollViewer.LineDown();
                }
                else
                {
                    WorkspaceScrollViewer.LineUp();
                }
            }
        }

        eventArgs.Handled = true;
    }

    private void HandleConfirmationVisibilityChanged(
        object sender,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is true)
        {
            _confirmationRestoreTarget = Keyboard.FocusedElement
                ?? FocusManager.GetFocusedElement(this);
            Dispatcher.BeginInvoke(
                () =>
                {
                    FocusManager.SetFocusedElement(
                        ConfirmationOverlay,
                        ConfirmationCancelButton);
                    ConfirmationCancelButton.Focus();
                },
                DispatcherPriority.Input);
            return;
        }

        IInputElement? restoreTarget = _confirmationRestoreTarget;
        _confirmationRestoreTarget = null;
        Dispatcher.BeginInvoke(() =>
        {
            if (restoreTarget is UIElement { IsVisible: true, IsEnabled: true } element)
            {
                FocusManager.SetFocusedElement(this, element);
                element.Focus();
                return;
            }

            Focus();
        }, DispatcherPriority.ContextIdle);
    }

    private void HandleConfirmationPreviewKeyDown(
        object sender,
        KeyEventArgs eventArgs)
    {
        if (eventArgs.Key != Key.Escape
            || DataContext is not ScheduleOverviewViewModel viewModel
            || !viewModel.CancelActiveConfirmationCommand.CanExecute(null))
        {
            return;
        }

        viewModel.CancelActiveConfirmationCommand.Execute(null);
        eventArgs.Handled = true;
    }

    private static IEnumerable<DependencyObject> GetVisualDescendants(
        DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in GetVisualDescendants(child))
            {
                yield return descendant;
            }
        }
    }
}
