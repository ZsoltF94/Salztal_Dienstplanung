using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Tests.Availabilities;

public sealed class AvailabilityEntryTests
{
    private static readonly Guid EmployeeIdentifier =
        new("8f09e479-c0e3-4097-af9f-49d813b121d4");

    [Theory]
    [InlineData(AvailabilityEntryKind.Vacation)]
    [InlineData(AvailabilityEntryKind.Sickness)]
    [InlineData(AvailabilityEntryKind.FixedDayOff)]
    public void CreateWhenKindIsSupportedPreservesStructuredValues(
        AvailabilityEntryKind kind)
    {
        DateOnly date = new(2026, 9, 21);

        AvailabilityEntryValidationResult result = AvailabilityEntry.Create(
            EmployeeIdentifier,
            date,
            kind);

        AvailabilityEntry entry = Assert.IsType<AvailabilityEntry>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(EmployeeIdentifier, entry.EmployeeId.Value);
        Assert.Equal(date, entry.Date);
        Assert.Equal(kind, entry.Kind);
    }

    [Fact]
    public void CreateWhenEmployeeIdentifierIsMissingReturnsStructuredError()
    {
        AvailabilityEntryValidationResult result = AvailabilityEntry.Create(
            Guid.Empty,
            new DateOnly(2026, 9, 21),
            AvailabilityEntryKind.Vacation);

        AvailabilityEntryValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            AvailabilityEntryValidationCode.EmployeeIdentifierRequired,
            error.Code);
    }

    [Fact]
    public void CreateWhenKindIsUnknownReturnsStructuredError()
    {
        AvailabilityEntryValidationResult result = AvailabilityEntry.Create(
            EmployeeIdentifier,
            new DateOnly(2026, 9, 21),
            (AvailabilityEntryKind)999);

        AvailabilityEntryValidationError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityEntryValidationCode.UnsupportedKind, error.Code);
    }

    [Fact]
    public void SetWhenEmployeeAndDateAreDuplicatedRejectsAmbiguousCurrentState()
    {
        DateOnly date = new(2026, 9, 21);
        AvailabilityEntry vacation = CreateEntry(date, AvailabilityEntryKind.Vacation);
        AvailabilityEntry sickness = CreateEntry(date, AvailabilityEntryKind.Sickness);

        AvailabilityEntrySetValidationResult result = AvailabilityEntrySet.Create(
            [vacation, sickness]);

        AvailabilityEntrySetValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(AvailabilityEntrySetValidationCode.DuplicateEmployeeAndDate, error.Code);
        Assert.Equal(EmployeeIdentifier, error.EmployeeId.Value);
        Assert.Equal(date, error.Date);
    }

    [Fact]
    public void WithEntryWhenEmployeeAndDateAlreadyExistReplacesPreviousKind()
    {
        DateOnly date = new(2026, 9, 21);
        AvailabilityEntry vacation = CreateEntry(date, AvailabilityEntryKind.Vacation);
        AvailabilityEntry sickness = CreateEntry(date, AvailabilityEntryKind.Sickness);
        AvailabilityEntrySet original = CreateSet(vacation);

        AvailabilityEntrySet changed = original.WithEntry(sickness);

        AvailabilityEntry current = Assert.IsType<AvailabilityEntry>(
            changed.Find(vacation.EmployeeId, date));
        Assert.Equal(AvailabilityEntryKind.Sickness, current.Kind);
        Assert.Single(changed.Entries);
        Assert.Equal(AvailabilityEntryKind.Vacation, original.Entries[0].Kind);
    }

    [Fact]
    public void WithEntryWhenKeyIsDifferentKeepsBothCurrentEntries()
    {
        AvailabilityEntry monday = CreateEntry(
            new DateOnly(2026, 9, 21),
            AvailabilityEntryKind.Vacation);
        AvailabilityEntry tuesday = CreateEntry(
            new DateOnly(2026, 9, 22),
            AvailabilityEntryKind.FixedDayOff);

        AvailabilityEntrySet changed = CreateSet(monday).WithEntry(tuesday);

        Assert.Equal([monday, tuesday], changed.Entries);
    }

    private static AvailabilityEntry CreateEntry(
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(EmployeeIdentifier, date, kind).Value);
    }

    private static AvailabilityEntrySet CreateSet(params AvailabilityEntry[] entries)
    {
        return Assert.IsType<AvailabilityEntrySet>(AvailabilityEntrySet.Create(entries).Value);
    }
}
