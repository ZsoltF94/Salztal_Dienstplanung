namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record ShiftPatternMinimizationParameters : RuleParameters
{
    internal ShiftPatternMinimizationParameters(ShiftPatternRuleKind patternKind)
    {
        if (!Enum.IsDefined(patternKind))
        {
            throw new ArgumentOutOfRangeException(nameof(patternKind));
        }

        PatternKind = patternKind;
    }

    public ShiftPatternRuleKind PatternKind { get; }
}
