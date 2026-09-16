namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleEvaluationResult
{
    private RuleEvaluationResult(
        RuleId ruleId,
        RuleEvaluationStatus status,
        RuleResultParameters parameters)
    {
        RuleId = ruleId;
        Status = status;
        Parameters = parameters;
    }

    public RuleId RuleId { get; }

    public RuleEvaluationStatus Status { get; }

    public RuleResultParameters Parameters { get; }

    public static RuleEvaluationResult Create(
        RuleId ruleId,
        RuleEvaluationStatus status,
        RuleResultParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(ruleId);
        ArgumentNullException.ThrowIfNull(parameters);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        return new RuleEvaluationResult(ruleId, status, parameters);
    }
}
