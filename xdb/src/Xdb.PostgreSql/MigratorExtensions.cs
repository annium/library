using DbUp;

// ReSharper disable once CheckNamespace

namespace Xdb;

public static class MigratorExtensions
{
    public static PostgresqlMigrationEngine ForPostgresql(
        this Migrator _,
        string connectionString,
        string schema
    ) =>
        new(
            DeployChanges.To.PostgresqlDatabase(connectionString),
            DeployChanges.To.PostgresqlDatabase(connectionString),
            schema
        );
}
