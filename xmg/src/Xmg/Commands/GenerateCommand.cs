using System;
using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using NodaTime;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Components;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Components;

namespace Xmg.Commands
{
    internal class GenerateCommand : Command<GenerateCommandConfig>
    {
        private readonly Func<Instant> _getInstant;
        private readonly IConfiguratorFactory _configuratorFactory;
        private readonly IMigratorFactory _migratorFactory;
        private readonly ILogger<GenerateCommand> _logger;
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate Migration";

        public GenerateCommand(
            Func<Instant> getInstant,
            IConfiguratorFactory configuratorFactory,
            IMigratorFactory migratorFactory,
            ILogger<GenerateCommand> logger
        )
        {
            _getInstant = getInstant;
            _configuratorFactory = configuratorFactory;
            _migratorFactory = migratorFactory;
            _logger = logger;
        }

        public override void Handle(
            GenerateCommandConfig cfg,
            CancellationToken token
        )
        {
            _logger.Debug($"Load '{cfg.ProjectName}' configuration from '{cfg.Assembly}'");

            var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
            var configurationCfg = new Configuration.Abstractions.Config(cfg.Assembly);
            var database = configurator.LoadConfiguration(configurationCfg);

            var migrationName = cfg.Name;
            var migrationVersion = _getInstant().ToDateTimeOffset().ToString("yyyyMMdd");


            _logger.Debug($"Create '{cfg.ProjectName}' migration '{migrationName}' ({migrationVersion}) from Database model");

            var migrator = _migratorFactory.GetForProvider(cfg.MigrationProvider);
            var migrationCfg = new Migration.Abstractions.Config(cfg.Namespace, migrationName, migrationVersion);
            var migration = migrator.CreateMigration(database, migrationCfg);

            _logger.Debug($"Create '{cfg.ProjectName}' migration '{migrationName}' ({migrationVersion}) files");
            if (!Directory.Exists(cfg.Output))
                Directory.CreateDirectory(cfg.Output);
            foreach (var (file, content) in migration.Files)
                File.WriteAllText(Path.Combine(cfg.Output, file), content);
        }
    }


    internal class GenerateCommandConfig
    {
        [Option("cp", true)]
        [Help("Config provider.")]
        public ConfigurationProvider ConfigurationProvider { get; set; }

        [Option("mp", true)]
        [Help("Migration provider.")]
        public MigrationProvider MigrationProvider { get; set; }

        [Option("a", true)]
        [Help("Path to Db assembly.")]
        public string Assembly
        {
            get => _assembly;
            set
            {
                _assembly = Path.GetFullPath(value);
                ProjectName = Path.GetFileNameWithoutExtension(value);
            }
        }

        public string ProjectName { get; private set; } = string.Empty;

        [Option("o", true)]
        [Help("Output directory. Will be created if missing.")]
        public string Output
        {
            get => _output;
            set => _output = Path.GetFullPath(value);
        }

        [Option("ns", true)]
        [Help("Migrations namespace.")]
        public string Namespace { get; set; } = string.Empty;

        [Option("n", true)]
        [Help("Migration name.")]
        public string Name { get; set; } = string.Empty;

        private string _assembly = string.Empty;
        private string _output = string.Empty;
    }
}