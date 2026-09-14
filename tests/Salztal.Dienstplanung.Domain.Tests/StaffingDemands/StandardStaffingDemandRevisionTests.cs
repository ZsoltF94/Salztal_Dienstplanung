using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.StaffingDemands;

public sealed class StandardStaffingDemandRevisionTests
{
    private static readonly Guid CafeteriaId = InitialWorkLocationCatalog.Cafeteria.Id.Value;
    private static readonly Guid CafeteriaShiftAId =
        InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value;

    [Fact]
    public void CreateAdditionWhenValuesAreValidReturnsImmutableStandardRevision()
    {
        Guid id = new("d82c634f-0cba-47f3-a36f-5a3a2d7aa639");
        DateOnly effectiveMonday = new(2026, 9, 14);

        StandardStaffingDemandRevisionValidationResult result =
            StandardStaffingDemandRevision.CreateAddition(
                id,
                DayOfWeek.Saturday,
                CafeteriaId,
                CafeteriaShiftAId,
                effectiveMonday,
                new TimeOnly(13, 30),
                new TimeOnly(20, 30),
                1);

        StandardStaffingDemandRevision revision =
            Assert.IsType<StandardStaffingDemandRevision>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, revision.Id.Value);
        Assert.Equal(DayOfWeek.Saturday, revision.Key.DayOfWeek);
        Assert.Equal(CafeteriaId, revision.Key.WorkLocationId.Value);
        Assert.Equal(CafeteriaShiftAId, revision.Key.ShiftTypeId.Value);
        Assert.Equal(effectiveMonday, revision.EffectiveFromMonday);
        Assert.Equal(1, revision.CorrectionSequence);
        Assert.Equal(StandardStaffingDemandRevisionKind.Add, revision.Kind);
        Assert.Equal(new TimeOnly(13, 30), revision.ActualTime!.Start);
        Assert.Equal(new TimeOnly(20, 30), revision.ActualTime.End);
        Assert.Equal(1, revision.RequiredEmployeeCount!.Value);
        Assert.Equal(420, revision.DurationMinutes);
        Assert.Equal(420L, revision.RequiredWorkMinutes);
    }

    [Fact]
    public void CreateAdditionWhenEffectiveDateIsNotMondayReturnsError()
    {
        StandardStaffingDemandRevisionValidationResult result =
            StandardStaffingDemandRevision.CreateAddition(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                CafeteriaId,
                CafeteriaShiftAId,
                new DateOnly(2026, 9, 15),
                new TimeOnly(13, 30),
                new TimeOnly(20, 30),
                1);

        StandardStaffingDemandRevisionValidationError error =
            Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            StandardStaffingDemandRevisionValidationCode.EffectiveDateMustBeMonday,
            error.Code);
    }

    [Fact]
    public void CreateAdditionWhenKeyValuesAreInvalidReturnsAllKeyErrors()
    {
        StandardStaffingDemandRevisionValidationResult result =
            StandardStaffingDemandRevision.CreateAddition(
                Guid.NewGuid(),
                (DayOfWeek)99,
                Guid.Empty,
                Guid.Empty,
                new DateOnly(2026, 9, 14),
                new TimeOnly(13, 30),
                new TimeOnly(20, 30),
                1);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                StandardStaffingDemandRevisionValidationCode.UnsupportedDayOfWeek,
                error.Code),
            error => Assert.Equal(
                StandardStaffingDemandRevisionValidationCode.WorkLocationRequired,
                error.Code),
            error => Assert.Equal(
                StandardStaffingDemandRevisionValidationCode.ShiftTypeRequired,
                error.Code));
    }

    [Fact]
    public void AdditionReplacementAndRemovalWhenCreatedRemainDistinct()
    {
        DateOnly firstMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision addition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday,
            1);
        StandardStaffingDemandRevision replacement = CreateDefinition(
            StandardStaffingDemandRevisionKind.Replace,
            firstMonday.AddDays(7),
            2);
        StandardStaffingDemandRevision removal = CreateRemoval(firstMonday.AddDays(14));

        Assert.Equal(StandardStaffingDemandRevisionKind.Add, addition.Kind);
        Assert.Equal(StandardStaffingDemandRevisionKind.Replace, replacement.Kind);
        Assert.Equal(2, replacement.RequiredEmployeeCount!.Value);
        Assert.Equal(StandardStaffingDemandRevisionKind.Remove, removal.Kind);
        Assert.Null(removal.ActualTime);
        Assert.Null(removal.RequiredEmployeeCount);
        Assert.Null(removal.DurationMinutes);
        Assert.Null(removal.RequiredWorkMinutes);
    }

    [Fact]
    public void CreateSetWithEarlierReplacementAndLaterRemovalResolvesEffectiveHistory()
    {
        DateOnly firstMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision addition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday,
            1);
        StandardStaffingDemandRevision replacement = CreateDefinition(
            StandardStaffingDemandRevisionKind.Replace,
            firstMonday.AddDays(14),
            2);
        StandardStaffingDemandRevision removal = CreateRemoval(firstMonday.AddDays(28));

        StandardStaffingDemandRevisionSetValidationResult result =
            StandardStaffingDemandRevisionSet.Create(
                [removal, addition, replacement]);

        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Null(
            revisionSet.FindEffectiveRevision(addition.Key, firstMonday.AddDays(-1)));
        Assert.Same(
            addition,
            revisionSet.FindEffectiveRevision(addition.Key, firstMonday.AddDays(7)));
        Assert.Same(
            replacement,
            revisionSet.FindEffectiveRevision(addition.Key, firstMonday.AddDays(21)));
        Assert.Null(
            revisionSet.FindEffectiveRevision(addition.Key, firstMonday.AddDays(35)));
    }

    [Fact]
    public void CreateSetWithAdditionAfterRemovalAllowsStandardToBeRestored()
    {
        DateOnly firstMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision firstAddition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday,
            1);
        StandardStaffingDemandRevision removal = CreateRemoval(firstMonday.AddDays(7));
        StandardStaffingDemandRevision restoredAddition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday.AddDays(14),
            3);

        StandardStaffingDemandRevisionSetValidationResult result =
            StandardStaffingDemandRevisionSet.Create(
                [firstAddition, removal, restoredAddition]);

        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(result.Value);
        Assert.Same(
            restoredAddition,
            revisionSet.FindEffectiveRevision(
                firstAddition.Key,
                firstMonday.AddDays(21)));
    }

    [Theory]
    [InlineData(StandardStaffingDemandRevisionKind.Replace)]
    [InlineData(StandardStaffingDemandRevisionKind.Remove)]
    public void CreateSetWhenFirstRevisionRequiresExistingStandardReturnsError(
        StandardStaffingDemandRevisionKind kind)
    {
        StandardStaffingDemandRevision revision = kind ==
            StandardStaffingDemandRevisionKind.Replace
            ? CreateDefinition(kind, new DateOnly(2026, 9, 14), 2)
            : CreateRemoval(new DateOnly(2026, 9, 14));

        StandardStaffingDemandRevisionSetValidationResult result =
            StandardStaffingDemandRevisionSet.Create([revision]);

        StandardStaffingDemandRevisionSetValidationError error =
            Assert.Single(result.Errors);
        Assert.Equal(
            kind == StandardStaffingDemandRevisionKind.Replace
                ? StandardStaffingDemandRevisionSetValidationCode
                    .ReplacementRequiresExistingStandard
                : StandardStaffingDemandRevisionSetValidationCode
                    .RemovalRequiresExistingStandard,
            error.Code);
    }

    [Fact]
    public void CreateSetWhenAdditionFollowsEffectiveStandardReturnsError()
    {
        DateOnly firstMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision firstAddition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday,
            1);
        StandardStaffingDemandRevision secondAddition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            firstMonday.AddDays(7),
            2);

        StandardStaffingDemandRevisionSetValidationResult result =
            StandardStaffingDemandRevisionSet.Create(
                [firstAddition, secondAddition]);

        StandardStaffingDemandRevisionSetValidationError error =
            Assert.Single(result.Errors);
        Assert.Equal(
            StandardStaffingDemandRevisionSetValidationCode
                .AdditionRequiresMissingStandard,
            error.Code);
    }

    [Fact]
    public void CreateSetWithRepeatedCorrectionsForSameMondayUsesLatestCorrection()
    {
        DateOnly effectiveMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision firstRevision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            effectiveMonday,
            1,
            correctionSequence: 1);
        StandardStaffingDemandRevision correctedRevision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Replace,
            effectiveMonday,
            2,
            correctionSequence: 2);

        StandardStaffingDemandRevisionSetValidationResult result =
            StandardStaffingDemandRevisionSet.Create(
                [correctedRevision, firstRevision]);

        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(result.Value);
        Assert.Empty(result.Errors);
        Assert.Same(
            correctedRevision,
            revisionSet.FindEffectiveRevision(firstRevision.Key, effectiveMonday));
    }

    [Fact]
    public void CreateAdditionWhenCorrectionSequenceIsNotPositiveReturnsError()
    {
        StandardStaffingDemandRevisionValidationResult result =
            StandardStaffingDemandRevision.CreateAddition(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                CafeteriaId,
                CafeteriaShiftAId,
                new DateOnly(2026, 9, 14),
                new TimeOnly(13, 30),
                new TimeOnly(20, 30),
                1,
                correctionSequence: 0);

        Assert.Equal(
            StandardStaffingDemandRevisionValidationCode
                .CorrectionSequenceMustBePositive,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public void CreateSetWithAddRemoveAndRestoreForSameMondayUsesRestoredCorrection()
    {
        DateOnly effectiveMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision addition = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            effectiveMonday,
            1,
            correctionSequence: 1);
        StandardStaffingDemandRevision removal = CreateRemoval(
            effectiveMonday,
            correctionSequence: 2);
        StandardStaffingDemandRevision restored = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            effectiveMonday,
            3,
            correctionSequence: 3);

        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    [addition, removal, restored]).Value);

        Assert.Same(
            restored,
            revisionSet.FindEffectiveRevision(addition.Key, effectiveMonday));
    }

    [Fact]
    public void CreateSetWhenCorrectionSequenceHasGapReturnsError()
    {
        DateOnly effectiveMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision firstRevision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            effectiveMonday,
            1,
            correctionSequence: 1);
        StandardStaffingDemandRevision thirdRevision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Replace,
            effectiveMonday,
            2,
            correctionSequence: 3);

        StandardStaffingDemandRevisionSetValidationError error = Assert.Single(
            StandardStaffingDemandRevisionSet.Create(
                [firstRevision, thirdRevision]).Errors);

        Assert.Equal(
            StandardStaffingDemandRevisionSetValidationCode
                .NonContiguousCorrectionSequence,
            error.Code);
    }

    [Fact]
    public void CreateSetWhenCorrectionSequenceIsDuplicatedReturnsError()
    {
        DateOnly effectiveMonday = new(2026, 9, 14);
        StandardStaffingDemandRevision firstRevision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            effectiveMonday,
            1,
            correctionSequence: 1);
        StandardStaffingDemandRevision duplicateSequence = CreateDefinition(
            StandardStaffingDemandRevisionKind.Replace,
            effectiveMonday,
            2,
            correctionSequence: 1);

        StandardStaffingDemandRevisionSetValidationError error = Assert.Single(
            StandardStaffingDemandRevisionSet.Create(
                [firstRevision, duplicateSequence]).Errors);

        Assert.Equal(
            StandardStaffingDemandRevisionSetValidationCode
                .DuplicateKeyEffectiveMondayAndCorrectionSequence,
            error.Code);
    }

    [Fact]
    public void CreateSetWhenSourceCollectionChangesKeepsImmutableSnapshot()
    {
        StandardStaffingDemandRevision revision = CreateDefinition(
            StandardStaffingDemandRevisionKind.Add,
            new DateOnly(2026, 9, 14),
            1);
        List<StandardStaffingDemandRevision> source = [revision];
        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(source).Value);

        source.Clear();

        Assert.Equal([revision], revisionSet.Revisions);
    }

    private static StandardStaffingDemandRevision CreateDefinition(
        StandardStaffingDemandRevisionKind kind,
        DateOnly effectiveMonday,
        int requiredEmployeeCount,
        int correctionSequence = 1)
    {
        StandardStaffingDemandRevisionValidationResult result = kind switch
        {
            StandardStaffingDemandRevisionKind.Add =>
                StandardStaffingDemandRevision.CreateAddition(
                    Guid.NewGuid(),
                    DayOfWeek.Monday,
                    CafeteriaId,
                    CafeteriaShiftAId,
                    effectiveMonday,
                    new TimeOnly(13, 30),
                    new TimeOnly(20, 30),
                    requiredEmployeeCount,
                    correctionSequence),
            StandardStaffingDemandRevisionKind.Replace =>
                StandardStaffingDemandRevision.CreateReplacement(
                    Guid.NewGuid(),
                    DayOfWeek.Monday,
                    CafeteriaId,
                    CafeteriaShiftAId,
                    effectiveMonday,
                    new TimeOnly(13, 30),
                    new TimeOnly(20, 30),
                    requiredEmployeeCount,
                    correctionSequence),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        return Assert.IsType<StandardStaffingDemandRevision>(result.Value);
    }

    private static StandardStaffingDemandRevision CreateRemoval(
        DateOnly effectiveMonday,
        int correctionSequence = 1)
    {
        StandardStaffingDemandRevisionValidationResult result =
            StandardStaffingDemandRevision.CreateRemoval(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                CafeteriaId,
                CafeteriaShiftAId,
                effectiveMonday,
                correctionSequence);

        return Assert.IsType<StandardStaffingDemandRevision>(result.Value);
    }
}
