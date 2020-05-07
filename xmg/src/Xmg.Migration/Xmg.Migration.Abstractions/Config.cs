namespace Xmg.Migration.Abstractions
{
    public class Config
    {
        public string MigrationName { get; }
        public string MigrationVersion { get; }

        public Config(
            string migrationName,
            string migrationVersion
        )
        {
            MigrationName = migrationName;
            MigrationVersion = migrationVersion;
        }
    }
}