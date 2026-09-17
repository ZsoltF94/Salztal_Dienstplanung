using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class DeterministicAutomaticScheduleAssignmentIdFactoryTests
{
    private static readonly Guid SnapshotId =
        Guid.Parse("10000000-0000-4000-8000-000000000001");
    private static readonly Guid EmployeeId =
        Guid.Parse("20000000-0000-4000-8000-000000000001");
    private static readonly DateOnly Date = new(2026, 9, 19);

    [Fact]
    public void SameTechnicalCombinationCreatesSameNonEmptyIdentifier()
    {
        DeterministicAutomaticScheduleAssignmentIdFactory factory = new();
        AutomaticScheduleAssignmentIdentity identity = CreateIdentity();

        Guid first = factory.Create(identity);
        Guid second = factory.Create(identity);

        Assert.NotEqual(Guid.Empty, first);
        Assert.Equal(first, second);
        Assert.Equal(
            Guid.Parse("7a3e64cc-dd4d-83b9-9834-b7155f358400"),
            first);
    }

    [Fact]
    public void DemandSlotInputOrderDoesNotChangeIdentifier()
    {
        DeterministicAutomaticScheduleAssignmentIdFactory factory = new();
        AutomaticScheduleDemandSlotIdentity first = CreateSlot(1);
        AutomaticScheduleDemandSlotIdentity second = CreateSlot(2);

        Guid patternId = Guid.Parse("60000000-0000-4000-8000-000000000001");
        Guid ordered = factory.Create(CreateIdentity(
            [first, second],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            patternId: patternId));
        Guid reversed = factory.Create(CreateIdentity(
            [second, first],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            patternId: patternId));

        Assert.Equal(ordered, reversed);
    }

    [Fact]
    public void EveryIdentityComponentChangesIdentifier()
    {
        DeterministicAutomaticScheduleAssignmentIdFactory factory = new();
        Guid baseline = factory.Create(CreateIdentity());

        AutomaticScheduleAssignmentIdentity[] changedIdentities =
        [
            CreateIdentity(snapshotId: Guid.NewGuid()),
            CreateIdentity(employeeId: Guid.NewGuid()),
            CreateIdentity(
                [CreateSlot(1, Date.AddDays(1))],
                date: Date.AddDays(1)),
            CreateIdentity([CreateSlot(2)]),
        ];

        Assert.All(
            changedIdentities,
            identity => Assert.NotEqual(baseline, factory.Create(identity)));

        Guid firstPattern = factory.Create(CreateIdentity(
            [CreateSlot(1), CreateSlot(2)],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            patternId: Guid.Parse("60000000-0000-4000-8000-000000000001")));
        Guid secondPattern = factory.Create(CreateIdentity(
            [CreateSlot(1), CreateSlot(2)],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            patternId: Guid.Parse("60000000-0000-4000-8000-000000000002")));
        Assert.NotEqual(firstPattern, secondPattern);
    }

    [Fact]
    public void AssignmentKindControlsPatternAndSlotShape()
    {
        Assert.Throws<ArgumentException>(() => CreateIdentity(
            [CreateSlot(1), CreateSlot(2)]));
        Assert.Throws<ArgumentException>(() => CreateIdentity(
            [CreateSlot(1), CreateSlot(2)],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern));
        Assert.Throws<ArgumentException>(() => CreateIdentity(
            [CreateSlot(1)],
            kind: ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            patternId: Guid.NewGuid()));
    }

    private static AutomaticScheduleAssignmentIdentity CreateIdentity(
        IEnumerable<AutomaticScheduleDemandSlotIdentity>? slots = null,
        Guid? snapshotId = null,
        Guid? employeeId = null,
        DateOnly? date = null,
        ScheduleAssignmentKindSnapshot kind = ScheduleAssignmentKindSnapshot.NormalDemand,
        Guid? patternId = null)
    {
        return new AutomaticScheduleAssignmentIdentity(
            snapshotId ?? SnapshotId,
            employeeId ?? EmployeeId,
            date ?? Date,
            kind,
            patternId,
            slots ?? [CreateSlot(1)]);
    }

    private static AutomaticScheduleDemandSlotIdentity CreateSlot(
        int ordinal,
        DateOnly? date = null)
    {
        return new AutomaticScheduleDemandSlotIdentity(
            Guid.Parse("30000000-0000-4000-8000-000000000001"),
            date ?? Date,
            Guid.Parse("40000000-0000-4000-8000-000000000001"),
            Guid.Parse("50000000-0000-4000-8000-000000000001"),
            ordinal);
    }
}
