using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemand
{
    private StaffingDemand(
        StaffingDemandId id,
        DateOnly date,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId,
        StaffingDemandTime actualTime,
        RequiredEmployeeCount requiredEmployeeCount)
    {
        Id = id;
        Date = date;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
        ActualTime = actualTime;
        RequiredEmployeeCount = requiredEmployeeCount;
        RequiredWorkMinutes = checked(
            (long)requiredEmployeeCount.Value * actualTime.DurationMinutes);
    }

    public StaffingDemandId Id { get; }

    public DateOnly Date { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    public StaffingDemandTime ActualTime { get; }

    public RequiredEmployeeCount RequiredEmployeeCount { get; }

    public int DurationMinutes => ActualTime.DurationMinutes;

    public long RequiredWorkMinutes { get; }

    public static StaffingDemandValidationResult Create(
        Guid id,
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        List<StaffingDemandValidationError> errors = [];

        if (!StaffingDemandId.TryCreate(id, out StaffingDemandId? validatedId))
        {
            errors.Add(new StaffingDemandValidationError(
                StaffingDemandValidationCode.IdentifierRequired));
        }

        if (!WorkLocationId.TryCreate(
                workLocationId,
                out WorkLocationId? validatedWorkLocationId))
        {
            errors.Add(new StaffingDemandValidationError(
                StaffingDemandValidationCode.WorkLocationRequired));
        }

        if (!ShiftTypeId.TryCreate(shiftTypeId, out ShiftTypeId? validatedShiftTypeId))
        {
            errors.Add(new StaffingDemandValidationError(
                StaffingDemandValidationCode.ShiftTypeRequired));
        }

        RequiredEmployeeCountValidationCode? employeeCountError =
            RequiredEmployeeCount.TryCreate(
                requiredEmployeeCount,
                out RequiredEmployeeCount? validatedRequiredEmployeeCount);
        if (employeeCountError is not null)
        {
            errors.Add(new StaffingDemandValidationError(
                StaffingDemandValidationCode.RequiredEmployeeCountMustBePositive));
        }

        IReadOnlyList<StaffingDemandTimeValidationCode> timeErrors =
            StaffingDemandTime.TryCreate(
                actualStart,
                actualEnd,
                out StaffingDemandTime? validatedActualTime);
        errors.AddRange(timeErrors.Select(CreateTimeValidationError));

        if (errors.Count > 0)
        {
            return StaffingDemandValidationResult.Failure(errors);
        }

        StaffingDemand staffingDemand = new(
            validatedId!,
            date,
            validatedWorkLocationId!,
            validatedShiftTypeId!,
            validatedActualTime!,
            validatedRequiredEmployeeCount!);

        return StaffingDemandValidationResult.Success(staffingDemand);
    }

    private static StaffingDemandValidationError CreateTimeValidationError(
        StaffingDemandTimeValidationCode code)
    {
        StaffingDemandValidationCode mappedCode = code switch
        {
            StaffingDemandTimeValidationCode.StartMustUseWholeMinute =>
                StaffingDemandValidationCode.ActualStartMustUseWholeMinute,
            StaffingDemandTimeValidationCode.EndMustUseWholeMinute =>
                StaffingDemandValidationCode.ActualEndMustUseWholeMinute,
            StaffingDemandTimeValidationCode.StartMustUseThirtyMinuteIncrement =>
                StaffingDemandValidationCode.ActualStartMustUseThirtyMinuteIncrement,
            StaffingDemandTimeValidationCode.EndMustUseThirtyMinuteIncrement =>
                StaffingDemandValidationCode.ActualEndMustUseThirtyMinuteIncrement,
            StaffingDemandTimeValidationCode.EndMustBeAfterStart =>
                StaffingDemandValidationCode.ActualEndMustBeAfterStart,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand time validation code: {code}"),
        };

        return new StaffingDemandValidationError(mappedCode);
    }
}
