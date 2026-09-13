using System.Diagnostics;

namespace Salztal.Dienstplanung.Desktop.Shared;

internal sealed class TraceUnexpectedErrorReporter : IUnexpectedErrorReporter
{
    public void Report(Exception exception, string operation)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        Trace.TraceError(
            "Unexpected desktop operation failure. Operation={0}; ExceptionType={1}",
            operation,
            exception.GetType().FullName);
    }
}
