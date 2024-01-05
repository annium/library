using DbUp.Builder;

// ReSharper disable once CheckNamespace

namespace Xdb;

public sealed class PostgresqlMigrationEngine : MigrationEngineBase<PostgresqlMigrationEngine>
{
    public PostgresqlMigrationEngine(
        UpgradeEngineBuilder initBuilder,
        UpgradeEngineBuilder migrationsBuilder,
        string schema
    )
        : base(initBuilder, migrationsBuilder)
    {
        MigrationsBuilder.JournalToPostgresqlTable(schema, "db_migrations");
    }
}
