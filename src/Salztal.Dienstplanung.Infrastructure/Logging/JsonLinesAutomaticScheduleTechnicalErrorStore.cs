using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Infrastructure.Logging;

public sealed class JsonLinesAutomaticScheduleTechnicalErrorStore
    : IAutomaticScheduleTechnicalErrorStore
{
    internal const string CurrentFileName = "automatic-schedule-errors.jsonl";
    internal const string PreviousFileName = "automatic-schedule-errors.previous.jsonl";
    private const long DefaultMaximumFileBytes = 512 * 1024;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly object writeLock = new();
    private readonly string directoryPath;
    private readonly long maximumFileBytes;
    private readonly TimeProvider timeProvider;

    public JsonLinesAutomaticScheduleTechnicalErrorStore(string directoryPath)
        : this(directoryPath, DefaultMaximumFileBytes, TimeProvider.System)
    {
    }

    internal JsonLinesAutomaticScheduleTechnicalErrorStore(
        string directoryPath,
        long maximumFileBytes,
        TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumFileBytes, 128);
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.directoryPath = directoryPath;
        this.maximumFileBytes = maximumFileBytes;
        this.timeProvider = timeProvider;
    }

    public bool TryWrite(string operation, AutomaticScheduleError technicalError)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(technicalError);
        if (string.IsNullOrWhiteSpace(technicalError.CorrelationId)
            || technicalError.TechnicalDetails is null)
        {
            return false;
        }

        try
        {
            Write(operation.Trim(), technicalError);
            return true;
        }
        catch (Exception exception)
        {
            Trace.TraceError(
                "Automatic schedule diagnostic write failed. ExceptionType={0}",
                exception.GetType().FullName);
            return false;
        }
    }

    private void Write(string operation, AutomaticScheduleError technicalError)
    {
        AutomaticScheduleTechnicalFailureDetails details =
            technicalError.TechnicalDetails!;
        TechnicalErrorLogEntry entry = new(
            "AutomaticSchedule.TechnicalFailure",
            timeProvider.GetUtcNow(),
            technicalError.CorrelationId!,
            operation,
            technicalError.Code.ToString(),
            details.Stage.ToString(),
            details.ExceptionType,
            details.HResult,
            details.CallPath,
            details.TechnicalContext);
        string line = JsonSerializer.Serialize(entry);
        int entryBytes = Utf8WithoutBom.GetByteCount(line + Environment.NewLine);

        lock (writeLock)
        {
            Directory.CreateDirectory(directoryPath);
            string currentPath = Path.Combine(directoryPath, CurrentFileName);
            RotateIfRequired(currentPath, entryBytes);
            File.AppendAllText(
                currentPath,
                line + Environment.NewLine,
                Utf8WithoutBom);
        }
    }

    private void RotateIfRequired(string currentPath, int entryBytes)
    {
        if (!File.Exists(currentPath)
            || new FileInfo(currentPath).Length + entryBytes <= maximumFileBytes)
        {
            return;
        }

        string previousPath = Path.Combine(directoryPath, PreviousFileName);
        File.Move(currentPath, previousPath, true);
    }

    private sealed record TechnicalErrorLogEntry(
        string EventId,
        DateTimeOffset TimestampUtc,
        string CorrelationId,
        string Operation,
        string ErrorCode,
        string Stage,
        string ExceptionType,
        int HResult,
        IReadOnlyList<string> CallPath,
        string? TechnicalContext);
}
