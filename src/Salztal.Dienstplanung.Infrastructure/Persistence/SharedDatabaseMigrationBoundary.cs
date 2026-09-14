namespace Salztal.Dienstplanung.Infrastructure.Persistence;

internal static class SharedDatabaseMigrationBoundary
{
    internal const string HistoryTableName = "__EFMigrationsHistory";

    internal static string AssemblyName { get; } =
        typeof(SharedDatabaseMigrationBoundary).Assembly.GetName().Name
        ?? throw new InvalidOperationException("The Infrastructure assembly has no name.");
}
