namespace Salztal.Dienstplanung.Application.Scheduling;

public interface IAutomaticScheduleTechnicalErrorStore
{
    public bool TryWrite(
        string operation,
        AutomaticScheduleError technicalError);
}
