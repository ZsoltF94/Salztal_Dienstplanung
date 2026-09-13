namespace Salztal.Dienstplanung.Desktop.Shared;

internal interface IUnexpectedErrorReporter
{
    public void Report(Exception exception, string operation);
}
