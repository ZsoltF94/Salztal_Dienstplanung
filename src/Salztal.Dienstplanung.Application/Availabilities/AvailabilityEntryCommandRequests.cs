namespace Salztal.Dienstplanung.Application.Availabilities;

public enum AvailabilityEntryReplacementConfirmation
{
    NotRequired,
    NotConfirmed,
    Confirmed,
}

public enum AvailabilityEntryRemovalConfirmation
{
    NotConfirmed,
    Confirmed,
}

public sealed record SaveAvailabilityEntryRequest(
    Guid EmployeeId,
    DateOnly Date,
    AvailabilityDayEntryKind Kind,
    long? ExpectedChangeVersion,
    AvailabilityEntryReplacementConfirmation ReplacementConfirmation);

public sealed record RemoveAvailabilityEntryRequest(
    Guid EmployeeId,
    DateOnly Date,
    long ExpectedChangeVersion,
    AvailabilityEntryRemovalConfirmation Confirmation);
