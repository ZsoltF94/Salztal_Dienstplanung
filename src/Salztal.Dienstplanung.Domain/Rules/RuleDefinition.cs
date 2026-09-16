namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleDefinition
{
    private RuleDefinition(
        RuleId id,
        RuleFamily family,
        RuleScope scope,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority,
        RuleParameters parameters,
        RuleDescriptionKey descriptionKey)
    {
        Id = id;
        Family = family;
        Scope = scope;
        AutomaticEffect = automaticEffect;
        ManualEffect = manualEffect;
        Priority = priority;
        Parameters = parameters;
        DescriptionKey = descriptionKey;
    }

    public RuleId Id { get; }

    public RuleFamily Family { get; }

    public RuleScope Scope { get; }

    public RuleAutomaticEffect AutomaticEffect { get; }

    public RuleManualEffect ManualEffect { get; }

    public RulePriority? Priority { get; }

    public RuleParameters Parameters { get; }

    public RuleDescriptionKey DescriptionKey { get; }

    public static RuleDefinitionValidationResult Create(
        string? id,
        RuleFamily family,
        RuleScope scope,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority,
        RuleParameters? parameters,
        string? descriptionKey)
    {
        List<RuleDefinitionValidationError> errors = [];

        if (!RuleId.TryCreate(id, out RuleId? ruleId))
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.IdentifierInvalid));
        }

        if (!RuleDescriptionKey.TryCreate(
                descriptionKey,
                out RuleDescriptionKey? ruleDescriptionKey))
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.DescriptionKeyRequired));
        }

        bool familyIsKnown = Enum.IsDefined(family);
        bool scopeIsKnown = Enum.IsDefined(scope);
        bool automaticEffectIsKnown = Enum.IsDefined(automaticEffect);
        bool manualEffectIsKnown = Enum.IsDefined(manualEffect);
        bool priorityIsKnown = priority is null || Enum.IsDefined(priority.Value);

        AddUnknownValueErrors(
            errors,
            familyIsKnown,
            scopeIsKnown,
            automaticEffectIsKnown,
            manualEffectIsKnown,
            priorityIsKnown);

        if (parameters is null)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.ParametersRequired));
        }

        if (familyIsKnown && automaticEffectIsKnown && manualEffectIsKnown
            && !IsEffectCombinationValid(family, automaticEffect, manualEffect))
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.EffectCombinationInvalid));
        }

        if (familyIsKnown && priorityIsKnown)
        {
            AddPriorityErrors(errors, family, priority);
        }

        if (errors.Count > 0)
        {
            return RuleDefinitionValidationResult.Failure(errors);
        }

        RuleDefinition definition = new(
            ruleId!,
            family,
            scope,
            automaticEffect,
            manualEffect,
            priority,
            parameters!,
            ruleDescriptionKey!);

        return RuleDefinitionValidationResult.Success(definition);
    }

    private static void AddUnknownValueErrors(
        List<RuleDefinitionValidationError> errors,
        bool familyIsKnown,
        bool scopeIsKnown,
        bool automaticEffectIsKnown,
        bool manualEffectIsKnown,
        bool priorityIsKnown)
    {
        if (!familyIsKnown)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.FamilyUnknown));
        }

        if (!scopeIsKnown)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.ScopeUnknown));
        }

        if (!automaticEffectIsKnown)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.AutomaticEffectUnknown));
        }

        if (!manualEffectIsKnown)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.ManualEffectUnknown));
        }

        if (!priorityIsKnown)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.PriorityUnknown));
        }
    }

    private static void AddPriorityErrors(
        List<RuleDefinitionValidationError> errors,
        RuleFamily family,
        RulePriority? priority)
    {
        if (family == RuleFamily.Soft && priority is null)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.PriorityRequired));
        }
        else if (family != RuleFamily.Soft && priority is not null)
        {
            errors.Add(new RuleDefinitionValidationError(
                RuleDefinitionValidationCode.PriorityNotAllowed));
        }
    }

    private static bool IsEffectCombinationValid(
        RuleFamily family,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect)
    {
        return family switch
        {
            RuleFamily.Structure =>
                automaticEffect == RuleAutomaticEffect.BlockInvalidStructure
                && manualEffect == RuleManualEffect.Block,
            RuleFamily.AutomaticHard =>
                automaticEffect == RuleAutomaticEffect.HardConstraint
                && manualEffect is RuleManualEffect.Block
                    or RuleManualEffect.WarnAndRequireConfirmation
                    or RuleManualEffect.NotApplicable,
            RuleFamily.Soft =>
                automaticEffect == RuleAutomaticEffect.OptimizationObjective
                && manualEffect == RuleManualEffect.WarnAndRequireConfirmation,
            RuleFamily.Notice =>
                automaticEffect == RuleAutomaticEffect.ReportNotice
                && manualEffect == RuleManualEffect.ReportNotice,
            RuleFamily.Stability =>
                automaticEffect == RuleAutomaticEffect.StabilityTieBreaker
                && manualEffect == RuleManualEffect.NotApplicable,
            _ => false,
        };
    }
}
