using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Employees;

namespace Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;

internal sealed class EmployeeTypeEligibilityEditorItemViewModel : ObservableObject
{
    private bool _isRegularAllowed;
    private bool _isManualSuggestionAllowed;
    private bool _requiresPlanningOption;

    public EmployeeTypeEligibilityEditorItemViewModel(
        Guid targetId,
        string targetName,
        EmployeeTypeEligibilityTargetKind targetKind)
    {
        TargetId = targetId;
        TargetName = targetName;
        TargetKind = targetKind;
    }

    public Guid TargetId { get; }

    public string TargetName { get; }

    public EmployeeTypeEligibilityTargetKind TargetKind { get; }

    public string TargetKindDisplay => TargetKind == EmployeeTypeEligibilityTargetKind.ShiftType
        ? "Normaler Dienst"
        : "Einsatzmuster";

    public bool SupportsPlanningOption =>
        TargetKind == EmployeeTypeEligibilityTargetKind.ShiftPattern;

    public bool IsRegularAllowed
    {
        get => _isRegularAllowed;
        set => SetProperty(ref _isRegularAllowed, value);
    }

    public bool IsManualSuggestionAllowed
    {
        get => _isManualSuggestionAllowed;
        set => SetProperty(ref _isManualSuggestionAllowed, value);
    }

    public bool RequiresPlanningOption
    {
        get => _requiresPlanningOption;
        set => SetProperty(ref _requiresPlanningOption, value);
    }

    public void Apply(IEnumerable<EmployeeTypeEligibilitySnapshot> eligibilities)
    {
        IsRegularAllowed = false;
        IsManualSuggestionAllowed = false;
        RequiresPlanningOption = false;

        foreach (EmployeeTypeEligibilitySnapshot eligibility in eligibilities.Where(
                     value => value.TargetId == TargetId && value.TargetKind == TargetKind))
        {
            if (eligibility.Mode == EmployeeTypeEligibilityMode.ManualSuggestion
                && eligibility.Activation == EmployeeTypeEligibilityActivation.Always)
            {
                IsManualSuggestionAllowed = true;
            }
            else if (eligibility.Mode == EmployeeTypeEligibilityMode.Regular
                     && eligibility.Activation == EmployeeTypeEligibilityActivation.Always)
            {
                IsRegularAllowed = true;
            }
            else if (eligibility.Mode == EmployeeTypeEligibilityMode.Regular
                     && eligibility.Activation ==
                     EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption
                     && SupportsPlanningOption)
            {
                RequiresPlanningOption = true;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Eligibility for target '{TargetId}' cannot be edited without data loss.");
            }
        }
    }

    public IEnumerable<EmployeeTypeEligibilityRequest> CreateRequests()
    {
        if (IsRegularAllowed)
        {
            yield return CreateRequest(
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.Always);
        }

        if (IsManualSuggestionAllowed)
        {
            yield return CreateRequest(
                EmployeeTypeEligibilityMode.ManualSuggestion,
                EmployeeTypeEligibilityActivation.Always);
        }

        if (RequiresPlanningOption && SupportsPlanningOption)
        {
            yield return CreateRequest(
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption);
        }
    }

    private EmployeeTypeEligibilityRequest CreateRequest(
        EmployeeTypeEligibilityMode mode,
        EmployeeTypeEligibilityActivation activation)
    {
        return new EmployeeTypeEligibilityRequest(TargetId, TargetKind, mode, activation);
    }
}
