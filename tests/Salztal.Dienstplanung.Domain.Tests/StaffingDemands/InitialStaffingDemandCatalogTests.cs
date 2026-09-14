using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.StaffingDemands;

public sealed class InitialStaffingDemandCatalogTests
{
    [Fact]
    public void AllWhenReadContainsConfirmedTwentyThreeWeekdayStandards()
    {
        IReadOnlyList<StandardStaffingDemandRevision> revisions =
            InitialStaffingDemandCatalog.All;

        Assert.Equal(23, revisions.Count);

        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            AssertStandard(
                revisions,
                dayOfWeek,
                InitialShiftTypeCatalog.CafeteriaShiftA,
                1);
            AssertStandard(revisions, dayOfWeek, InitialShiftTypeCatalog.EarlyShift, 4);
            AssertStandard(revisions, dayOfWeek, InitialShiftTypeCatalog.LateShift, 4);
        }

        AssertStandard(
            revisions,
            DayOfWeek.Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            1);
        AssertStandard(
            revisions,
            DayOfWeek.Sunday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            1);

        Assert.DoesNotContain(
            revisions,
            revision => revision.Key.ShiftTypeId ==
                InitialShiftTypeCatalog.CafeteriaShiftB.Id
                && revision.Key.DayOfWeek is not DayOfWeek.Saturday
                and not DayOfWeek.Sunday);
        Assert.Equal(
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday,
            ],
            revisions.Select(revision => revision.Key.DayOfWeek).Distinct());
    }

    [Fact]
    public void AllWhenReadUsesUniqueKeysAndStableIdentifiers()
    {
        IReadOnlyList<StandardStaffingDemandRevision> revisions =
            InitialStaffingDemandCatalog.All;

        Assert.Equal(
            revisions.Count,
            revisions.Select(revision => revision.Id).Distinct().Count());
        Assert.Equal(
            revisions.Count,
            revisions.Select(revision => revision.Key).Distinct().Count());
        Assert.DoesNotContain(revisions, revision => revision.Id.Value == Guid.Empty);
        Assert.All(
            revisions,
            revision =>
            {
                Assert.Equal(StandardStaffingDemandRevisionKind.Add, revision.Kind);
                Assert.Equal(DateOnly.MinValue, revision.EffectiveFromMonday);
                Assert.Equal(DayOfWeek.Monday, revision.EffectiveFromMonday.DayOfWeek);
            });
    }

    [Fact]
    public void AllWhenReadContainsOnlyNormalShiftTypeIdentifiers()
    {
        HashSet<Guid> knownNormalShiftTypeIds = InitialShiftTypeCatalog.All
            .Select(shiftType => shiftType.Id.Value)
            .ToHashSet();
        HashSet<Guid> patternIds = InitialShiftPatternCatalog.All
            .Select(pattern => pattern.Id.Value)
            .ToHashSet();

        Assert.All(
            InitialStaffingDemandCatalog.All,
            revision => Assert.Contains(
                revision.Key.ShiftTypeId.Value,
                knownNormalShiftTypeIds));
        Assert.DoesNotContain(
            InitialStaffingDemandCatalog.All,
            revision => patternIds.Contains(revision.Key.ShiftTypeId.Value));
    }

    [Fact]
    public void CreateFromCurrentShiftTypesUsesCurrentStandardTimes()
    {
        ShiftStandardTime changedStandardTime = Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(
                new TimeOnly(14, 0),
                new TimeOnly(21, 0)).Value);
        ShiftType changedCafeteriaShiftA = Assert.IsType<ShiftType>(
            InitialShiftTypeCatalog.CafeteriaShiftA
                .WithStandardTime(changedStandardTime).Value);
        ShiftType[] currentShiftTypes = InitialShiftTypeCatalog.All
            .Select(
                shiftType => shiftType.Id == changedCafeteriaShiftA.Id
                    ? changedCafeteriaShiftA
                    : shiftType)
            .ToArray();

        StandardStaffingDemandRevisionSet revisionSet =
            InitialStaffingDemandCatalog.CreateFromCurrentShiftTypes(currentShiftTypes);

        StandardStaffingDemandRevision[] cafeteriaShiftARevisions = revisionSet.Revisions
            .Where(
                revision => revision.Key.ShiftTypeId ==
                    InitialShiftTypeCatalog.CafeteriaShiftA.Id)
            .ToArray();
        Assert.Equal(7, cafeteriaShiftARevisions.Length);
        Assert.All(
            cafeteriaShiftARevisions,
            revision =>
            {
                Assert.Equal(new TimeOnly(14, 0), revision.ActualTime!.Start);
                Assert.Equal(new TimeOnly(21, 0), revision.ActualTime.End);
            });
        Assert.Equal(
            new TimeOnly(6, 30),
            revisionSet.Revisions.Single(
                revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                    && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id)
                .ActualTime!.Start);
    }

    private static void AssertStandard(
        IReadOnlyList<StandardStaffingDemandRevision> revisions,
        DayOfWeek dayOfWeek,
        ShiftType shiftType,
        int expectedRequiredEmployeeCount)
    {
        StandardStaffingDemandRevision revision = Assert.Single(
            revisions,
            candidate => candidate.Key.DayOfWeek == dayOfWeek
                && candidate.Key.WorkLocationId == shiftType.WorkLocationId
                && candidate.Key.ShiftTypeId == shiftType.Id);

        Assert.Equal(expectedRequiredEmployeeCount, revision.RequiredEmployeeCount!.Value);
        Assert.Equal(shiftType.StandardTime.Start, revision.ActualTime!.Start);
        Assert.Equal(shiftType.StandardTime.End, revision.ActualTime.End);
    }
}
