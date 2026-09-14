using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

internal sealed class StaffingDemandWeekSnapshotProjector
{
    private readonly StaffingDemandCatalogValidationResult _catalog;

    public StaffingDemandWeekSnapshotProjector(
        StaffingDemandCatalogValidationResult catalog)
    {
        _catalog = catalog;
    }

    public StaffingDemandWeekSnapshot Create(
        StaffingDemandWeek week,
        StandardStaffingDemandRevisionSet standardRevisions,
        StaffingDemandDateExceptionSet dateExceptions)
    {
        return new StaffingDemandWeekSnapshot(
            week.WeekMonday,
            week.Demands.Select(CreateDemandSnapshot),
            CreateStandardEditItems(week.WeekMonday, standardRevisions),
            CreateDateEditItems(week, standardRevisions, dateExceptions),
            week.DaySummaries.Select(summary =>
            {
                WorkLocation workLocation =
                    _catalog.WorkLocationsById[summary.WorkLocationId];
                return new StaffingDemandDayWorkLocationSummarySnapshot(
                    summary.Date,
                    workLocation.Id.Value,
                    workLocation.Name.Value,
                    workLocation.Color.Code,
                    summary.RequiredWorkMinutes);
            }),
            week.WorkLocationSummaries.Select(summary =>
            {
                WorkLocation workLocation =
                    _catalog.WorkLocationsById[summary.WorkLocationId];
                return new StaffingDemandWorkLocationWeekSummarySnapshot(
                    workLocation.Id.Value,
                    workLocation.Name.Value,
                    workLocation.Color.Code,
                    summary.RequiredWorkMinutes);
            }),
            week.TotalRequiredWorkMinutes);
    }

    private IEnumerable<DateStaffingDemandEditItemSnapshot> CreateDateEditItems(
        StaffingDemandWeek week,
        StandardStaffingDemandRevisionSet standardRevisions,
        StaffingDemandDateExceptionSet dateExceptions)
    {
        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            DateOnly date = week.WeekMonday.AddDays(dayOffset);

            foreach (WorkLocation workLocation in _catalog.WorkLocationsById.Values
                         .OrderBy(location => location.Name.Value, StringComparer.CurrentCulture))
            {
                foreach (ShiftType shiftType in _catalog.ShiftTypesById.Values
                             .Where(candidate => candidate.WorkLocationId == workLocation.Id)
                             .OrderBy(candidate => candidate.Name.Value, StringComparer.CurrentCulture))
                {
                    StandardStaffingDemandRevision? regularStandard = standardRevisions.Revisions
                        .Where(revision => revision.Key.DayOfWeek == date.DayOfWeek)
                        .Where(revision => revision.Key.WorkLocationId == workLocation.Id)
                        .Where(revision => revision.Key.ShiftTypeId == shiftType.Id)
                        .Where(revision => revision.EffectiveFromMonday <= date)
                        .OrderBy(revision => revision.EffectiveFromMonday)
                        .ThenBy(revision => revision.CorrectionSequence)
                        .LastOrDefault();
                    if (regularStandard?.Kind == StandardStaffingDemandRevisionKind.Remove)
                    {
                        regularStandard = null;
                    }

                    StaffingDemandDateException? dateException = dateExceptions.Exceptions
                        .SingleOrDefault(candidate =>
                            candidate.Key.Date == date
                            && candidate.Key.WorkLocationId == workLocation.Id
                            && candidate.Key.ShiftTypeId == shiftType.Id);
                    EffectiveStaffingDemand? effectiveDemand = week.Demands
                        .SingleOrDefault(candidate =>
                            candidate.Date == date
                            && candidate.WorkLocationId == workLocation.Id
                            && candidate.ShiftTypeId == shiftType.Id);

                    yield return new DateStaffingDemandEditItemSnapshot(
                        date,
                        workLocation.Id.Value,
                        workLocation.Name.Value,
                        workLocation.Color.Code,
                        shiftType.Id.Value,
                        shiftType.Name.Value,
                        shiftType.StandardTime.Start,
                        shiftType.StandardTime.End,
                        regularStandard is not null,
                        regularStandard?.ActualTime?.Start,
                        regularStandard?.ActualTime?.End,
                        regularStandard?.RequiredEmployeeCount?.Value,
                        effectiveDemand is not null,
                        dateException is not null,
                        dateException?.Kind == StaffingDemandDateExceptionKind.Remove,
                        dateException?.Id.Value,
                        effectiveDemand?.ActualTime.Start,
                        effectiveDemand?.ActualTime.End,
                        effectiveDemand?.RequiredEmployeeCount.Value);
                }
            }
        }
    }

    private IEnumerable<StandardStaffingDemandEditItemSnapshot> CreateStandardEditItems(
        DateOnly effectiveFromMonday,
        StandardStaffingDemandRevisionSet standardRevisions)
    {
        foreach (WorkLocation workLocation in _catalog.WorkLocationsById.Values
                     .OrderBy(location => location.Name.Value, StringComparer.CurrentCulture))
        {
            foreach (ShiftType shiftType in _catalog.ShiftTypesById.Values
                         .Where(candidate => candidate.WorkLocationId == workLocation.Id)
                         .OrderBy(candidate => candidate.Name.Value, StringComparer.CurrentCulture))
            {
                for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                {
                    DayOfWeek dayOfWeek = effectiveFromMonday.AddDays(dayOffset).DayOfWeek;
                    StandardStaffingDemandRevision[] history = standardRevisions.Revisions
                        .Where(revision => revision.Key.DayOfWeek == dayOfWeek)
                        .Where(revision => revision.Key.WorkLocationId == workLocation.Id)
                        .Where(revision => revision.Key.ShiftTypeId == shiftType.Id)
                        .OrderBy(revision => revision.EffectiveFromMonday)
                        .ThenBy(revision => revision.CorrectionSequence)
                        .ToArray();
                    StandardStaffingDemandRevision? current = history
                        .Where(revision =>
                            revision.EffectiveFromMonday <= effectiveFromMonday)
                        .LastOrDefault();
                    bool hasEffectiveStandard = current is not null
                        && current.Kind != StandardStaffingDemandRevisionKind.Remove;

                    yield return new StandardStaffingDemandEditItemSnapshot(
                        effectiveFromMonday,
                        dayOfWeek,
                        workLocation.Id.Value,
                        workLocation.Name.Value,
                        shiftType.Id.Value,
                        shiftType.Name.Value,
                        shiftType.StandardTime.Start,
                        shiftType.StandardTime.End,
                        hasEffectiveStandard,
                        current?.EffectiveFromMonday == effectiveFromMonday,
                        current?.Id.Value,
                        hasEffectiveStandard ? current!.ActualTime!.Start : null,
                        hasEffectiveStandard ? current!.ActualTime!.End : null,
                        hasEffectiveStandard ? current!.RequiredEmployeeCount!.Value : null);
                }
            }
        }
    }

    private StaffingDemandItemSnapshot CreateDemandSnapshot(
        EffectiveStaffingDemand demand)
    {
        WorkLocation workLocation =
            _catalog.WorkLocationsById[demand.WorkLocationId];
        ShiftType shiftType = _catalog.ShiftTypesById[demand.ShiftTypeId];
        StaffingDemandSourceSnapshotKind sourceKind = demand.SourceKind switch
        {
            StaffingDemandSourceKind.Standard => StaffingDemandSourceSnapshotKind.Standard,
            StaffingDemandSourceKind.DateException =>
                StaffingDemandSourceSnapshotKind.DateException,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand source kind: {demand.SourceKind}"),
        };

        return new StaffingDemandItemSnapshot(
            demand.SourceId.Value,
            sourceKind,
            sourceKind == StaffingDemandSourceSnapshotKind.Standard
                ? "Regelmäßiger Standard"
                : "Einmalige Änderung",
            demand.Date,
            workLocation.Id.Value,
            workLocation.Name.Value,
            workLocation.Color.Code,
            shiftType.Id.Value,
            shiftType.Name.Value,
            shiftType.Display.Abbreviation,
            shiftType.Display.Kind == ShiftTypeDisplayKind.ActualTime,
            demand.ActualTime.Start,
            demand.ActualTime.End,
            demand.RequiredEmployeeCount.Value,
            demand.DurationMinutes,
            demand.RequiredWorkMinutes);
    }
}
