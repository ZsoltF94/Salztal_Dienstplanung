using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed partial class AutomaticScheduleReportWindow : Window
{
    public AutomaticScheduleReportWindow()
    {
        InitializeComponent();
    }

    private void OnDataContextChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        while (EmployeeGrid.Columns.Count > 7)
        {
            EmployeeGrid.Columns.RemoveAt(EmployeeGrid.Columns.Count - 1);
        }

        if (e.NewValue is not AutomaticScheduleReportWindowViewModel
            {
                Employees: not null,
            } viewModel)
        {
            return;
        }

        for (int index = 0; index < viewModel.Employees.ServiceColumns.Count; index++)
        {
            PlanningEmployeeServiceColumnViewModel column =
                viewModel.Employees.ServiceColumns[index];
            EmployeeGrid.Columns.Add(new DataGridTextColumn
            {
                Header = column.Display,
                Binding = new Binding($"ServiceCounts[{index}].Count"),
                IsReadOnly = true,
                Width = new DataGridLength(62),
            });
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape
            || e.Key == Key.W && Keyboard.Modifiers == ModifierKeys.Control)
        {
            Close();
            e.Handled = true;
        }
    }
}
