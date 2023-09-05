using DbUp.Builder;

namespace Xdb.Core.Migrations;

public class PostgresqlMigrationEngine : MigrationEngineBase<PostgresqlMigrationEngine>
{
    public PostgresqlMigrationEngine(UpgradeEngineBuilder initBuilder, UpgradeEngineBuilder migrationsBuilder, string schema) : base(initBuilder, migrationsBuilder)
    {
        MigrationsBuilder.JournalToPostgresqlTable(schema, "db_migrations");
    }
}