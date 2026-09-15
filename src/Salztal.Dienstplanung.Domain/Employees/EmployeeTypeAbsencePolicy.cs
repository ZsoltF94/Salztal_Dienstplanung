using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record EmployeeTypeAbsencePolicy
{
    private EmployeeTypeAbsencePolicy(
        bool allowsVacationAndSickness,
        AbsenceDayValue? dayValue)
    {
        AllowsVacationAndSickness = allowsVacationAndSickness;
        DayValue = dayValue;
    }

    public static EmployeeTypeAbsencePolicy NotAllowed { get; } = new(false, null);

    public bool AllowsVacationAndSickness { get; }

    public AbsenceDayValue? DayValue { get; }

    internal static EmployeeTypeValidationCode? TryCreate(
        bool allowsVacationAndSickness,
        int? dayValueMinutes,
        [NotNullWhen(true)] out EmployeeTypeAbsencePolicy? absencePolicy)
    {
        if (!allowsVacationAndSickness)
        {
            if (dayValueMinutes is not null)
            {
                absencePolicy = null;
                return EmployeeTypeValidationCode.AbsenceDayValueMustNotBeSet;
            }

            absencePolicy = NotAllowed;
            return null;
        }

        if (dayValueMinutes is null)
        {
            absencePolicy = null;
            return EmployeeTypeValidationCode.AbsenceDayValueRequired;
        }

        AbsenceDayValueValidationCode? dayValueError = AbsenceDayValue.TryCreate(
            dayValueMinutes.Value,
            out AbsenceDayValue? dayValue);

        if (dayValueError is not null)
        {
            absencePolicy = null;
            return dayValueError == AbsenceDayValueValidationCode.MustBePositive
                ? EmployeeTypeValidationCode.AbsenceDayValueMustBePositive
                : EmployeeTypeValidationCode.AbsenceDayValueExceedsDay;
        }

        absencePolicy = new EmployeeTypeAbsencePolicy(true, dayValue!);
        return null;
    }
}
