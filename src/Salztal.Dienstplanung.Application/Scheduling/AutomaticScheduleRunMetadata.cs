using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticSchedulePhaseTraceCompleteness
{
    Complete,
    InputValidationNotRecorded,
    NotRecorded,
}

public sealed record AutomaticScheduleSetting(string Key, string Value)
{
    public string Key { get; } = string.IsNullOrWhiteSpace(Key)
        ? throw new ArgumentException("A solver setting key is required.", nameof(Key))
        : Key.Trim();

    public string Value { get; } = string.IsNullOrWhiteSpace(Value)
        ? throw new ArgumentException("A solver setting value is required.", nameof(Value))
        : Value.Trim();
}

public sealed class AutomaticScheduleRunMetadata
{
    public AutomaticScheduleRunMetadata(
        string solverName,
        string solverVersion,
        AutomaticSchedulePlanningStatus resultStatus,
        TimeSpan timeLimit,
        TimeSpan modelBuildDuration,
        TimeSpan optimizationDuration,
        TimeSpan resultMappingDuration,
        TimeSpan totalDuration,
        IEnumerable<AutomaticScheduleSetting> settings,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? phases = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solverName);
        ArgumentException.ThrowIfNullOrWhiteSpace(solverVersion);
        if (resultStatus is not AutomaticSchedulePlanningStatus.Optimal
            and not AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal)
        {
            throw new ArgumentOutOfRangeException(nameof(resultStatus));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeLimit, TimeSpan.Zero);
        ThrowIfNegative(modelBuildDuration, nameof(modelBuildDuration));
        ThrowIfNegative(optimizationDuration, nameof(optimizationDuration));
        ThrowIfNegative(resultMappingDuration, nameof(resultMappingDuration));
        ThrowIfNegative(totalDuration, nameof(totalDuration));
        ArgumentNullException.ThrowIfNull(settings);

        AutomaticScheduleSetting[] settingValues = settings.ToArray();
        if (settingValues.Any(setting => setting is null))
        {
            throw new ArgumentException(
                "Solver settings cannot contain null values.",
                nameof(settings));
        }

        if (settingValues.Select(setting => setting.Key).Distinct(StringComparer.Ordinal).Count()
            != settingValues.Length)
        {
            throw new ArgumentException(
                "Solver setting keys must be unique.",
                nameof(settings));
        }

        TimeSpan measuredDuration = modelBuildDuration
            + optimizationDuration
            + resultMappingDuration;
        if (totalDuration < measuredDuration)
        {
            throw new ArgumentException(
                "Total duration cannot be shorter than its measured parts.",
                nameof(totalDuration));
        }

        SolverName = solverName.Trim();
        SolverVersion = solverVersion.Trim();
        ResultStatus = resultStatus;
        TimeLimit = timeLimit;
        ModelBuildDuration = modelBuildDuration;
        OptimizationDuration = optimizationDuration;
        ResultMappingDuration = resultMappingDuration;
        TotalDuration = totalDuration;
        Settings = Array.AsReadOnly(
            settingValues.OrderBy(setting => setting.Key, StringComparer.Ordinal).ToArray());
        Phases = AutomaticSchedulePhaseSequence.Create(phases ?? []);
        PhaseTraceCompleteness = Phases.Count == 0
            ? AutomaticSchedulePhaseTraceCompleteness.NotRecorded
            : Phases[0].Kind == AutomaticSchedulePhaseKind.InputValidation
                ? AutomaticSchedulePhaseTraceCompleteness.Complete
                : AutomaticSchedulePhaseTraceCompleteness.InputValidationNotRecorded;
    }

    public string SolverName { get; }

    public string SolverVersion { get; }

    public AutomaticSchedulePlanningStatus ResultStatus { get; }

    public TimeSpan TimeLimit { get; }

    public TimeSpan ModelBuildDuration { get; }

    public TimeSpan OptimizationDuration { get; }

    public TimeSpan ResultMappingDuration { get; }

    public TimeSpan TotalDuration { get; }

    public ReadOnlyCollection<AutomaticScheduleSetting> Settings { get; }

    public ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> Phases { get; }

    public AutomaticSchedulePhaseTraceCompleteness PhaseTraceCompleteness { get; }

    internal AutomaticScheduleRunMetadata PrependPhase(
        AutomaticSchedulePhaseSnapshot phase)
    {
        ArgumentNullException.ThrowIfNull(phase);
        return new AutomaticScheduleRunMetadata(
            SolverName,
            SolverVersion,
            ResultStatus,
            TimeLimit,
            ModelBuildDuration,
            OptimizationDuration,
            ResultMappingDuration,
            TotalDuration,
            Settings,
            [phase, .. Phases]);
    }

    private static void ThrowIfNegative(TimeSpan value, string parameterName)
    {
        if (value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
