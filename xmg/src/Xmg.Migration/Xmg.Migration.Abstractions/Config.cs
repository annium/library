namespace Xmg.Migration.Abstractions
{
    public class Config
    {
        public string MigrationNamespace { get; }
        public string MigrationName { get; }
        public string MigrationVersion { get; }

        public Config(
            string migrationNamespace,
            string migrationName,
            string migrationVersion
        )
        {
            MigrationNamespace = migrationNamespace;
            MigrationName = migrationName;
            MigrationVersion = migrationVersion;
        }
    }
}