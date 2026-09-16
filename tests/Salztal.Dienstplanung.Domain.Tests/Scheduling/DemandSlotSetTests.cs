using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

public sealed class DemandSlotSetTests
{
    private static readonly IReadOnlyList<WorkLocationId> KnownWorkLocationIds =
        InitialWorkLocationCatalog.All.Select(location => location.Id).ToArray();
    private static readonly IReadOnlyList<ShiftTypeId> KnownShiftTypeIds =
        InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id).ToArray();

    [Fact]
    public void CreateForThreeInitialWeeksReturnsOneSlotPerRequiredEmployee()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand[] demands = ResolvePeriodDemands(period);

        DemandSlotSet slotSet = CreateSlotSet(period, demands);

        Assert.Equal(195, slotSet.Slots.Count);
        Assert.Equal(
            demands.Sum(demand => demand.RequiredEmployeeCount.Value),
            slotSet.Slots.Count);
        Assert.Equal(slotSet.Slots.Count, slotSet.Slots.Select(slot => slot.Id).Distinct().Count());
    }

    [Fact]
    public void CreateSplitsFourPersonDemandIntoDeterministicOrdinals()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand earlyDemand = ResolvePeriodDemands(period)
            .Single(demand =>
                demand.Date == period.StartMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id);

        DemandSlot[] slots = CreateSlotSet(period, [earlyDemand]).Slots.ToArray();

        Assert.Equal([1, 2, 3, 4], slots.Select(slot => slot.Id.Ordinal));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.SourceId, slot.Id.SourceId));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.SourceKind, slot.Id.SourceKind));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.Date, slot.Id.Date));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.WorkLocationId, slot.Id.WorkLocationId));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.ShiftTypeId, slot.Id.ShiftTypeId));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.ActualTime, slot.ActualTime));
        Assert.All(slots, slot => Assert.Equal(earlyDemand.DurationMinutes, slot.DurationMinutes));
    }

    [Fact]
    public void CreateUsesStableValueIdentityAcrossRepeatedConstruction()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand[] demands = ResolvePeriodDemands(period);

        DemandSlotSet first = CreateSlotSet(period, demands);
        DemandSlotSet second = CreateSlotSet(period, demands.Reverse());

        Assert.Equal(first.Slots, second.Slots);
    }

    [Fact]
    public void CreateOrdersSlotsByDateLocationShiftSourceAndOrdinal()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand[] demands = ResolvePeriodDemands(period);

        DemandSlotSet slotSet = CreateSlotSet(period, demands.Reverse());
        DemandSlotId[] independentlyOrdered = slotSet.Slots
            .Select(slot => slot.Id)
            .OrderBy(id => id.Date)
            .ThenBy(id => id.WorkLocationId.Value)
            .ThenBy(id => id.ShiftTypeId.Value)
            .ThenBy(id => id.SourceKind)
            .ThenBy(id => id.SourceId.Value)
            .ThenBy(id => id.Ordinal)
            .ToArray();

        Assert.Equal(independentlyOrdered, slotSet.Slots.Select(slot => slot.Id));
    }

    [Fact]
    public void CreateWithDemandOutsidePeriodReturnsStructuredError()
    {
        SchedulePeriod period = CreatePeriod();
        SchedulePeriod laterPeriod = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(period.StartMonday.AddDays(21)).Value);
        EffectiveStaffingDemand outsideDemand = ResolvePeriodDemands(laterPeriod)[0];

        DemandSlotSetValidationResult result = CreateSlotSetResult(
            period,
            [outsideDemand]);

        DemandSlotSetValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(DemandSlotSetValidationCode.DemandOutsidePeriod, error.Code);
        Assert.Equal(outsideDemand.SourceId, error.SourceId);
        Assert.Equal(outsideDemand.Date, error.Date);
    }

    [Fact]
    public void CreateWithUnknownWorkLocationReturnsStructuredError()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand restaurantDemand = ResolvePeriodDemands(period)
            .First(demand => demand.WorkLocationId == InitialWorkLocationCatalog.Restaurant.Id);

        DemandSlotSetValidationResult result = DemandSlotSet.Create(
            period,
            [restaurantDemand],
            [InitialWorkLocationCatalog.Cafeteria.Id],
            KnownShiftTypeIds);

        DemandSlotSetValidationError error = Assert.Single(result.Errors);
        Assert.Equal(DemandSlotSetValidationCode.UnknownWorkLocation, error.Code);
        Assert.Equal(restaurantDemand.WorkLocationId, error.WorkLocationId);
    }

    [Fact]
    public void CreateWithUnknownShiftTypeReturnsStructuredError()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand earlyDemand = ResolvePeriodDemands(period)
            .First(demand => demand.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id);
        ShiftTypeId[] otherShiftTypeIds = KnownShiftTypeIds
            .Where(id => id != earlyDemand.ShiftTypeId)
            .ToArray();

        DemandSlotSetValidationResult result = DemandSlotSet.Create(
            period,
            [earlyDemand],
            KnownWorkLocationIds,
            otherShiftTypeIds);

        DemandSlotSetValidationError error = Assert.Single(result.Errors);
        Assert.Equal(DemandSlotSetValidationCode.UnknownShiftType, error.Code);
        Assert.Equal(earlyDemand.ShiftTypeId, error.ShiftTypeId);
    }

    [Fact]
    public void CreateWithDuplicateDemandReturnsDuplicateSlotIdentityErrors()
    {
        SchedulePeriod period = CreatePeriod();
        EffectiveStaffingDemand demand = ResolvePeriodDemands(period)
            .First(candidate => candidate.RequiredEmployeeCount.Value == 4);

        DemandSlotSetValidationResult result = CreateSlotSetResult(
            period,
            [demand, demand]);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(4, result.Errors.Count);
        Assert.All(
            result.Errors,
            error => Assert.Equal(
                DemandSlotSetValidationCode.DuplicateSlotIdentity,
                error.Code));
        Assert.Equal([1, 2, 3, 4], result.Errors.Select(error => error.SlotId!.Ordinal));
    }

    private static SchedulePeriod CreatePeriod()
    {
        return Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(new DateOnly(2026, 9, 14)).Value);
    }

    private static DemandSlotSet CreateSlotSet(
        SchedulePeriod period,
        IEnumerable<EffectiveStaffingDemand> demands)
    {
        return Assert.IsType<DemandSlotSet>(CreateSlotSetResult(period, demands).Value);
    }

    private static DemandSlotSetValidationResult CreateSlotSetResult(
        SchedulePeriod period,
        IEnumerable<EffectiveStaffingDemand> demands)
    {
        return DemandSlotSet.Create(
            period,
            demands,
            KnownWorkLocationIds,
            KnownShiftTypeIds);
    }

    private static EffectiveStaffingDemand[] ResolvePeriodDemands(SchedulePeriod period)
    {
        StandardStaffingDemandRevisionSet revisions =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    InitialStaffingDemandCatalog.All).Value);
        StaffingDemandDateExceptionSet exceptions =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create(
                    Array.Empty<StaffingDemandDateException>()).Value);

        return Enumerable.Range(0, 3)
            .SelectMany(
                weekIndex => Assert.IsType<StaffingDemandWeek>(
                    StaffingDemandWeek.Resolve(
                        period.StartMonday.AddDays(weekIndex * 7),
                        revisions,
                        exceptions).Value).Demands)
            .ToArray();
    }
}
