namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StandardStaffingDemandRevision
{
    private StandardStaffingDemandRevision(
        StaffingDemandId id,
        StandardStaffingDemandKey key,
        DateOnly effectiveFromMonday,
        int correctionSequence,
        StandardStaffingDemandRevisionKind kind,
        StaffingDemandTime? actualTime,
        RequiredEmployeeCount? requiredEmployeeCount)
    {
        Id = id;
        Key = key;
        EffectiveFromMonday = effectiveFromMonday;
        CorrectionSequence = correctionSequence;
        Kind = kind;
        ActualTime = actualTime;
        RequiredEmployeeCount = requiredEmployeeCount;
        RequiredWorkMinutes = actualTime is null || requiredEmployeeCount is null
            ? null
            : checked((long)requiredEmployeeCount.Value * actualTime.DurationMinutes);
    }

    public StaffingDemandId Id { get; }

    public StandardStaffingDemandKey Key { get; }

    public DateOnly EffectiveFromMonday { get; }

    public int CorrectionSequence { get; }

    public StandardStaffingDemandRevisionKind Kind { get; }

    public StaffingDemandTime? ActualTime { get; }

    public RequiredEmployeeCount? RequiredEmployeeCount { get; }

    public int? DurationMinutes => ActualTime?.DurationMinutes;

    public long? RequiredWorkMinutes { get; }

    public static StandardStaffingDemandRevisionValidationResult CreateAddition(
        Guid id,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount,
        int correctionSequence = 1)
    {
        return CreateDefinedRevision(
            id,
            dayOfWeek,
            workLocationId,
            shiftTypeId,
            effectiveFromMonday,
            correctionSequence,
            StandardStaffingDemandRevisionKind.Add,
            actualStart,
            actualEnd,
            requiredEmployeeCount);
    }

    public static StandardStaffingDemandRevisionValidationResult CreateReplacement(
        Guid id,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount,
        int correctionSequence = 1)
    {
        return CreateDefinedRevision(
            id,
            dayOfWeek,
            workLocationId,
            shiftTypeId,
            effectiveFromMonday,
            correctionSequence,
            StandardStaffingDemandRevisionKind.Replace,
            actualStart,
            actualEnd,
            requiredEmployeeCount);
    }

    public static StandardStaffingDemandRevisionValidationResult CreateRemoval(
        Guid id,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        int correctionSequence = 1)
    {
        List<StandardStaffingDemandRevisionValidationError> errors = [];
        (StaffingDemandId? validatedId, StandardStaffingDemandKey? validatedKey) =
            ValidateIdentityAndKey(
                id,
                dayOfWeek,
                workLocationId,
                shiftTypeId,
                errors);
        ValidateEffectiveMonday(effectiveFromMonday, errors);
        ValidateCorrectionSequence(correctionSequence, errors);

        if (errors.Count > 0)
        {
            return StandardStaffingDemandRevisionValidationResult.Failure(errors);
        }

        return StandardStaffingDemandRevisionValidationResult.Success(
            new StandardStaffingDemandRevision(
                validatedId!,
                validatedKey!,
                effectiveFromMonday,
                correctionSequence,
                StandardStaffingDemandRevisionKind.Remove,
                null,
                null));
    }

    private static StandardStaffingDemandRevisionValidationResult CreateDefinedRevision(
        Guid id,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        DateOnly effectiveFromMonday,
        int correctionSequence,
        StandardStaffingDemandRevisionKind kind,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        List<StandardStaffingDemandRevisionValidationError> errors = [];
        (StaffingDemandId? validatedId, StandardStaffingDemandKey? validatedKey) =
            ValidateIdentityAndKey(
                id,
                dayOfWeek,
                workLocationId,
                shiftTypeId,
                errors);
        ValidateEffectiveMonday(effectiveFromMonday, errors);
        ValidateCorrectionSequence(correctionSequence, errors);

        RequiredEmployeeCountValidationCode? employeeCountError =
            RequiredEmployeeCount.TryCreate(
                requiredEmployeeCount,
                out RequiredEmployeeCount? validatedRequiredEmployeeCount);
        if (employeeCountError is not null)
        {
            errors.Add(new StandardStaffingDemandRevisionValidationError(
                StandardStaffingDemandRevisionValidationCode
                    .RequiredEmployeeCountMustBePositive));
        }

        IReadOnlyList<StaffingDemandTimeValidationCode> timeErrors =
            StaffingDemandTime.TryCreate(
                actualStart,
                actualEnd,
                out StaffingDemandTime? validatedActualTime);
        errors.AddRange(timeErrors.Select(CreateTimeValidationError));

        if (errors.Count > 0)
        {
            return StandardStaffingDemandRevisionValidationResult.Failure(errors);
        }

        return StandardStaffingDemandRevisionValidationResult.Success(
            new StandardStaffingDemandRevision(
                validatedId!,
                validatedKey!,
                effectiveFromMonday,
                correctionSequence,
                kind,
                validatedActualTime!,
                validatedRequiredEmployeeCount!));
    }

    private static (
        StaffingDemandId? Id,
        StandardStaffingDemandKey? Key) ValidateIdentityAndKey(
        Guid id,
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        List<StandardStaffingDemandRevisionValidationError> errors)
    {
        if (!StaffingDemandId.TryCreate(id, out StaffingDemandId? validatedId))
        {
            errors.Add(new StandardStaffingDemandRevisionValidationError(
                StandardStaffingDemandRevisionValidationCode.IdentifierRequired));
        }

        IReadOnlyList<StandardStaffingDemandRevisionValidationCode> keyErrors =
            StandardStaffingDemandKey.TryCreate(
                dayOfWeek,
                workLocationId,
                shiftTypeId,
                out StandardStaffingDemandKey? validatedKey);
        errors.AddRange(
            keyErrors.Select(
                code => new StandardStaffingDemandRevisionValidationError(code)));

        return (validatedId, validatedKey);
    }

    private static void ValidateEffectiveMonday(
        DateOnly effectiveFromMonday,
        List<StandardStaffingDemandRevisionValidationError> errors)
    {
        if (effectiveFromMonday.DayOfWeek != DayOfWeek.Monday)
        {
            errors.Add(new StandardStaffingDemandRevisionValidationError(
                StandardStaffingDemandRevisionValidationCode.EffectiveDateMustBeMonday));
        }
    }

    private static void ValidateCorrectionSequence(
        int correctionSequence,
        List<StandardStaffingDemandRevisionValidationError> errors)
    {
        if (correctionSequence <= 0)
        {
            errors.Add(new StandardStaffingDemandRevisionValidationError(
                StandardStaffingDemandRevisionValidationCode
                    .CorrectionSequenceMustBePositive));
        }
    }

    private static StandardStaffingDemandRevisionValidationError CreateTimeValidationError(
        StaffingDemandTimeValidationCode code)
    {
        StandardStaffingDemandRevisionValidationCode mappedCode = code switch
        {
            StaffingDemandTimeValidationCode.StartMustUseWholeMinute =>
                StandardStaffingDemandRevisionValidationCode.ActualStartMustUseWholeMinute,
            StaffingDemandTimeValidationCode.EndMustUseWholeMinute =>
                StandardStaffingDemandRevisionValidationCode.ActualEndMustUseWholeMinute,
            StaffingDemandTimeValidationCode.StartMustUseThirtyMinuteIncrement =>
                StandardStaffingDemandRevisionValidationCode
                    .ActualStartMustUseThirtyMinuteIncrement,
            StaffingDemandTimeValidationCode.EndMustUseThirtyMinuteIncrement =>
                StandardStaffingDemandRevisionValidationCode
                    .ActualEndMustUseThirtyMinuteIncrement,
            StaffingDemandTimeValidationCode.EndMustBeAfterStart =>
                StandardStaffingDemandRevisionValidationCode.ActualEndMustBeAfterStart,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand time validation code: {code}"),
        };

        return new StandardStaffingDemandRevisionValidationError(mappedCode);
    }
}
