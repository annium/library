using DbUp;

namespace Xdb.Core.Migrations;

public static class Migrator
{
    // public static IMigrationEngine ForSqlServer(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SqlDatabase(connectionString));
    public static PostgresqlMigrationEngine ForPostgresql(string connectionString, string schema) =>
        new(DeployChanges.To.PostgresqlDatabase(connectionString), DeployChanges.To.PostgresqlDatabase(connectionString), schema);
    // public static IMigrationEngine ForMysql(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.MySqlDatabase(connectionString));
    // public static IMigrationEngine ForSqlite(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SQLiteDatabase(connectionString));
}