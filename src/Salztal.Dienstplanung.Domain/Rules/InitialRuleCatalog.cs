namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialRuleCatalog
{
    private static readonly RuleCatalogVersion CatalogVersion = CreateCatalogVersion();
    private static readonly RuleCatalog Catalog = new(
        CatalogVersion,
        InitialStructureRuleDefinitions.All
            .Concat(InitialAutomaticHardRuleDefinitions.All)
            .Concat(InitialSoftRuleDefinitions.All)
            .Concat(InitialNoticeRuleDefinitions.All)
            .Concat(InitialStabilityRuleDefinitions.All));

    public static RuleCatalogVersion Version => CatalogVersion;

    public static RuleCatalogReadResult Read(RuleCatalogVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        return version == CatalogVersion
            ? RuleCatalogReadResult.Success(Catalog)
            : RuleCatalogReadResult.Failure(version);
    }

    private static RuleCatalogVersion CreateCatalogVersion()
    {
        return RuleCatalogVersion.TryCreate(1, out RuleCatalogVersion? version)
            ? version
            : throw new InvalidOperationException("Initial rule catalog version is invalid.");
    }
}
