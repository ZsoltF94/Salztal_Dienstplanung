using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class AvailabilityPeriodEmployeeSnapshot
{
    internal AvailabilityPeriodEmployeeSnapshot(
        Guid employeeId,
        string displayName,
        Guid employeeTypeId,
        string employeeTypeCode,
        string employeeTypeName,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        IEnumerable<AvailabilityPeriodEntrySnapshot> entries,
        IEnumerable<AvailabilityPeriodWeekSnapshot> weeks)
    {
        EmployeeId = employeeId;
        DisplayName = displayName;
        EmployeeTypeId = employeeTypeId;
        EmployeeTypeCode = employeeTypeCode;
        EmployeeTypeName = employeeTypeName;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        AbsenceDayValueMinutes = absenceDayValueMinutes;
        Entries = Array.AsReadOnly(entries.ToArray());
        Weeks = Array.AsReadOnly(weeks.ToArray());
    }

    public Guid EmployeeId { get; }

    public string DisplayName { get; }

    public Guid EmployeeTypeId { get; }

    public string EmployeeTypeCode { get; }

    public string EmployeeTypeName { get; }

    public bool AllowsVacationAndSickness { get; }

    public int? AbsenceDayValueMinutes { get; }

    public ReadOnlyCollection<AvailabilityPeriodEntrySnapshot> Entries { get; }

    public ReadOnlyCollection<AvailabilityPeriodWeekSnapshot> Weeks { get; }
}
