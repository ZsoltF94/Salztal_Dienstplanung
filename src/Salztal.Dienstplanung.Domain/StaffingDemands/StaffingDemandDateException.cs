using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "DateException is the established domain term for a staffing-demand override.")]
public sealed class StaffingDemandDateException
{
    private StaffingDemandDateException(
        StaffingDemandId id,
        StaffingDemandDateKey key,
        StaffingDemandDateExceptionKind kind,
        StaffingDemandTime? actualTime,
        RequiredEmployeeCount? requiredEmployeeCount,
        long? requiredWorkMinutes)
    {
        Id = id;
        Key = key;
        Kind = kind;
        ActualTime = actualTime;
        RequiredEmployeeCount = requiredEmployeeCount;
        RequiredWorkMinutes = requiredWorkMinutes;
    }

    public StaffingDemandId Id { get; }

    public StaffingDemandDateKey Key { get; }

    public StaffingDemandDateExceptionKind Kind { get; }

    public StaffingDemandTime? ActualTime { get; }

    public RequiredEmployeeCount? RequiredEmployeeCount { get; }

    public int? DurationMinutes => ActualTime?.DurationMinutes;

    public long? RequiredWorkMinutes { get; }

    public static StaffingDemandDateExceptionValidationResult CreateAddition(
        Guid id,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        return CreateDefinedException(
            id,
            date,
            workLocationId,
            shiftTypeId,
            StaffingDemandDateExceptionKind.Add,
            actualStart,
            actualEnd,
            requiredEmployeeCount);
    }

    public static StaffingDemandDateExceptionValidationResult CreateReplacement(
        Guid id,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        return CreateDefinedException(
            id,
            date,
            workLocationId,
            shiftTypeId,
            StaffingDemandDateExceptionKind.Replace,
            actualStart,
            actualEnd,
            requiredEmployeeCount);
    }

    public static StaffingDemandDateExceptionValidationResult CreateRemoval(
        Guid id,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId)
    {
        List<StaffingDemandDateExceptionValidationError> errors = [];

        if (!StaffingDemandId.TryCreate(id, out StaffingDemandId? validatedId))
        {
            errors.Add(new StaffingDemandDateExceptionValidationError(
                StaffingDemandDateExceptionValidationCode.IdentifierRequired));
        }

        IReadOnlyList<StaffingDemandDateExceptionValidationCode> keyErrors =
            StaffingDemandDateKey.TryCreate(
                date,
                workLocationId,
                shiftTypeId,
                out StaffingDemandDateKey? validatedKey);
        errors.AddRange(
            keyErrors.Select(
                code => new StaffingDemandDateExceptionValidationError(code)));

        if (errors.Count > 0)
        {
            return StaffingDemandDateExceptionValidationResult.Failure(errors);
        }

        return StaffingDemandDateExceptionValidationResult.Success(
            new StaffingDemandDateException(
                validatedId!,
                validatedKey!,
                StaffingDemandDateExceptionKind.Remove,
                null,
                null,
                null));
    }

    private static StaffingDemandDateExceptionValidationResult CreateDefinedException(
        Guid id,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        StaffingDemandDateExceptionKind kind,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        StaffingDemandValidationResult demandResult = StaffingDemand.Create(
            id,
            date,
            workLocationId,
            shiftTypeId,
            actualStart,
            actualEnd,
            requiredEmployeeCount);

        if (!demandResult.IsSuccess)
        {
            return StaffingDemandDateExceptionValidationResult.Failure(
                demandResult.Errors.Select(CreateValidationError));
        }

        StaffingDemand demand = demandResult.Value!;
        StaffingDemandDateKey key = StaffingDemandDateKey.CreateValidated(
            demand.Date,
            demand.WorkLocationId,
            demand.ShiftTypeId);

        return StaffingDemandDateExceptionValidationResult.Success(
            new StaffingDemandDateException(
                demand.Id,
                key,
                kind,
                demand.ActualTime,
                demand.RequiredEmployeeCount,
                demand.RequiredWorkMinutes));
    }

    private static StaffingDemandDateExceptionValidationError CreateValidationError(
        StaffingDemandValidationError error)
    {
        StaffingDemandDateExceptionValidationCode code = error.Code switch
        {
            StaffingDemandValidationCode.IdentifierRequired =>
                StaffingDemandDateExceptionValidationCode.IdentifierRequired,
            StaffingDemandValidationCode.WorkLocationRequired =>
                StaffingDemandDateExceptionValidationCode.WorkLocationRequired,
            StaffingDemandValidationCode.ShiftTypeRequired =>
                StaffingDemandDateExceptionValidationCode.ShiftTypeRequired,
            StaffingDemandValidationCode.RequiredEmployeeCountMustBePositive =>
                StaffingDemandDateExceptionValidationCode
                    .RequiredEmployeeCountMustBePositive,
            StaffingDemandValidationCode.ActualStartMustUseWholeMinute =>
                StaffingDemandDateExceptionValidationCode.ActualStartMustUseWholeMinute,
            StaffingDemandValidationCode.ActualEndMustUseWholeMinute =>
                StaffingDemandDateExceptionValidationCode.ActualEndMustUseWholeMinute,
            StaffingDemandValidationCode.ActualStartMustUseThirtyMinuteIncrement =>
                StaffingDemandDateExceptionValidationCode
                    .ActualStartMustUseThirtyMinuteIncrement,
            StaffingDemandValidationCode.ActualEndMustUseThirtyMinuteIncrement =>
                StaffingDemandDateExceptionValidationCode
                    .ActualEndMustUseThirtyMinuteIncrement,
            StaffingDemandValidationCode.ActualEndMustBeAfterStart =>
                StaffingDemandDateExceptionValidationCode.ActualEndMustBeAfterStart,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand validation code: {error.Code}"),
        };

        return new StaffingDemandDateExceptionValidationError(code);
    }
}
