using System.Text.Json;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Infrastructure.Logging;

namespace Salztal.Dienstplanung.Infrastructure.Tests.Logging;

public sealed class JsonLinesAutomaticScheduleTechnicalErrorStoreTests
{
    [Fact]
    public void WriteCreatesPrivacySafeStructuredEntryWithoutBom()
    {
        const string SensitiveMessage = "Mitarbeiter Erika Mustermann kompletter Dienstplan";
        using TemporaryDirectory directory = new();
        DateTimeOffset now = new(2026, 9, 17, 18, 30, 0, TimeSpan.Zero);
        JsonLinesAutomaticScheduleTechnicalErrorStore store = new(
            directory.Path,
            4096,
            new FixedTimeProvider(now));
        AutomaticScheduleError error = CreateError(
            "0123456789abcdef0123456789abcdef",
            new InvalidDataException(SensitiveMessage));

        bool written = store.TryWrite("GenerateAutomaticSchedule", error);

        Assert.True(written);
        string path = System.IO.Path.Combine(
            directory.Path,
            JsonLinesAutomaticScheduleTechnicalErrorStore.CurrentFileName);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.False(bytes.Length >= 3
            && bytes[0] == 0xEF
            && bytes[1] == 0xBB
            && bytes[2] == 0xBF);
        string content = File.ReadAllText(path);
        Assert.DoesNotContain(SensitiveMessage, content, StringComparison.Ordinal);
        Assert.DoesNotContain(directory.Path, content, StringComparison.Ordinal);
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement root = json.RootElement;
        Assert.Equal("AutomaticSchedule.TechnicalFailure",
            root.GetProperty("EventId").GetString());
        Assert.Equal(now, root.GetProperty("TimestampUtc").GetDateTimeOffset());
        Assert.Equal(error.CorrelationId, root.GetProperty("CorrelationId").GetString());
        Assert.Equal("GenerateAutomaticSchedule",
            root.GetProperty("Operation").GetString());
        Assert.Equal("Solving", root.GetProperty("Stage").GetString());
        Assert.Equal(typeof(InvalidDataException).FullName,
            root.GetProperty("ExceptionType").GetString());
        Assert.Equal(JsonValueKind.Null,
            root.GetProperty("TechnicalContext").ValueKind);
    }

    [Fact]
    public void WriteRotatesToExactlyOnePreviousFile()
    {
        using TemporaryDirectory directory = new();
        JsonLinesAutomaticScheduleTechnicalErrorStore store = new(
            directory.Path,
            128,
            TimeProvider.System);

        Assert.True(store.TryWrite("GenerateAutomaticSchedule",
            CreateError("first-correlation", new InvalidOperationException())));
        Assert.True(store.TryWrite("GenerateAutomaticSchedule",
            CreateError("second-correlation", new InvalidOperationException())));
        Assert.True(store.TryWrite("GenerateAutomaticSchedule",
            CreateError("third-correlation", new InvalidOperationException())));

        string current = File.ReadAllText(System.IO.Path.Combine(
            directory.Path,
            JsonLinesAutomaticScheduleTechnicalErrorStore.CurrentFileName));
        string previous = File.ReadAllText(System.IO.Path.Combine(
            directory.Path,
            JsonLinesAutomaticScheduleTechnicalErrorStore.PreviousFileName));
        Assert.Contains("third-correlation", current, StringComparison.Ordinal);
        Assert.Contains("second-correlation", previous, StringComparison.Ordinal);
        Assert.DoesNotContain("first-correlation", current + previous, StringComparison.Ordinal);
        Assert.Equal(2, Directory.GetFiles(directory.Path).Length);
    }

    [Fact]
    public void WriteFailureReturnsFalseWithoutThrowing()
    {
        using TemporaryDirectory directory = new();
        string fileInsteadOfDirectory = System.IO.Path.Combine(directory.Path, "occupied");
        File.WriteAllText(fileInsteadOfDirectory, "occupied");
        JsonLinesAutomaticScheduleTechnicalErrorStore store = new(fileInsteadOfDirectory);

        bool written = store.TryWrite(
            "GenerateAutomaticSchedule",
            CreateError("failed-correlation", new IOException()));

        Assert.False(written);
    }

    private static AutomaticScheduleError CreateError(
        string correlationId,
        Exception exception) => new(
            AutomaticScheduleErrorCode.TechnicalFailure,
            CorrelationId: correlationId,
            Parameter: exception.GetType().FullName,
            TechnicalDetails: AutomaticScheduleTechnicalFailureDetails.FromException(
                AutomaticScheduleTechnicalStage.Solving,
                exception));

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Salztal-Dienstplanung-Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
