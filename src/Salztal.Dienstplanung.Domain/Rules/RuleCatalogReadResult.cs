namespace Salztal.Dienstplanung.Domain.Rules;

public sealed class RuleCatalogReadResult
{
    private RuleCatalogReadResult(
        RuleCatalog? value,
        RuleCatalogReadError? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Value is not null;

    public RuleCatalog? Value { get; }

    public RuleCatalogReadError? Error { get; }

    internal static RuleCatalogReadResult Success(RuleCatalog value)
    {
        return new RuleCatalogReadResult(value, null);
    }

    internal static RuleCatalogReadResult Failure(RuleCatalogVersion requestedVersion)
    {
        return new RuleCatalogReadResult(
            null,
            new RuleCatalogReadError(
                RuleCatalogReadCode.UnsupportedVersion,
                requestedVersion));
    }
}
