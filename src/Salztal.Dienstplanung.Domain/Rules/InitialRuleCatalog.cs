namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialRuleCatalog
{
    private static readonly RuleCatalogVersion VersionOneValue = CreateVersion(1);
    private static readonly RuleCatalogVersion CurrentVersionValue = CreateVersion(2);
    private static readonly RuleCatalog VersionOneCatalog = new(
        VersionOneValue,
        InitialStructureRuleDefinitions.All
            .Concat(InitialAutomaticHardRuleDefinitions.All)
            .Concat(InitialSoftRuleDefinitions.All)
            .Concat(InitialNoticeRuleDefinitions.All)
            .Concat(InitialStabilityRuleDefinitions.All));
    private static readonly RuleCatalog CurrentCatalog = new(
        CurrentVersionValue,
        InitialStructureRuleDefinitions.All
            .Concat(InitialAutomaticHardRuleDefinitions.All)
            .Concat(CurrentSoftRuleDefinitions.All)
            .Concat(InitialNoticeRuleDefinitions.All)
            .Concat(InitialStabilityRuleDefinitions.All));

    public static RuleCatalogVersion VersionOne => VersionOneValue;

    public static RuleCatalogVersion Version => CurrentVersionValue;

    public static RuleCatalogReadResult Read(RuleCatalogVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (version == VersionOneValue)
        {
            return RuleCatalogReadResult.Success(VersionOneCatalog);
        }

        return version == CurrentVersionValue
            ? RuleCatalogReadResult.Success(CurrentCatalog)
            : RuleCatalogReadResult.Failure(version);
    }

    private static RuleCatalogVersion CreateVersion(int value)
    {
        return RuleCatalogVersion.TryCreate(value, out RuleCatalogVersion? version)
            ? version
            : throw new InvalidOperationException("Rule catalog version is invalid.");
    }
}
