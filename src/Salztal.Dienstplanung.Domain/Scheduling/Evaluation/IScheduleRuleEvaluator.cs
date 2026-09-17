using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

public interface IScheduleRuleEvaluator
{
    public RuleId RuleId { get; }

    public RuleEvaluationResult Evaluate(ScheduleEvaluationContext context);
}
