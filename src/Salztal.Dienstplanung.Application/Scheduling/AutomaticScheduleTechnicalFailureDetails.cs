using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticScheduleTechnicalStage
{
    ApplicationValidation,
    InputLoading,
    CurrentSnapshotBuilding,
    PlanningBoundary,
    ModelBuilding,
    Solving,
    Mapping,
    ResultMapping,
}

public sealed class AutomaticScheduleTechnicalFailureDetails
{
    private const int MaximumCallPathLength = 16;

    private AutomaticScheduleTechnicalFailureDetails(
        AutomaticScheduleTechnicalStage stage,
        string exceptionType,
        int hResult,
        ReadOnlyCollection<string> callPath,
        string? technicalContext)
    {
        Stage = stage;
        ExceptionType = exceptionType;
        HResult = hResult;
        CallPath = callPath;
        TechnicalContext = technicalContext;
    }

    public AutomaticScheduleTechnicalStage Stage { get; }

    public string ExceptionType { get; }

    public int HResult { get; }

    public ReadOnlyCollection<string> CallPath { get; }

    public string? TechnicalContext { get; }

    public static AutomaticScheduleTechnicalFailureDetails FromException(
        AutomaticScheduleTechnicalStage stage,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        string exceptionType = exception.GetType().FullName
            ?? exception.GetType().Name;
        string[] callPath = new StackTrace(exception, false)
            .GetFrames()
            .Select(frame => frame.GetMethod())
            .Where(method => method is not null)
            .Take(MaximumCallPathLength)
            .Select(method => method!.DeclaringType is null
                ? method.Name
                : $"{method.DeclaringType.FullName}.{method.Name}")
            .ToArray();
        string? technicalContext = exception is IAutomaticScheduleTechnicalContextProvider
            provider
                ? SanitizeTechnicalContext(provider.TechnicalContext)
                : null;
        return new AutomaticScheduleTechnicalFailureDetails(
            stage,
            exceptionType,
            exception.HResult,
            Array.AsReadOnly(callPath),
            technicalContext);
    }

    private static string? SanitizeTechnicalContext(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > 512
            || value.Any(character => !char.IsAsciiLetterOrDigit(character)
                && character is not '.' and not '_' and not ',' and not ':' and not '-'))
        {
            return null;
        }

        return value;
    }
}

public interface IAutomaticScheduleTechnicalContextProvider
{
    public string TechnicalContext { get; }
}
