using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Components;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Components;

namespace Xmg.Commands
{
    internal class GenerateCommand : Command<GenerateCommandConfiguration>
    {
        private readonly IConfiguratorFactory _configuratorFactory;
        private readonly IMigratorFactory _migratorFactory;
        private readonly ILogger<GenerateCommand> _logger;
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate Migration";

        public GenerateCommand(
            IConfiguratorFactory configuratorFactory,
            IMigratorFactory migratorFactory,
            ILogger<GenerateCommand> logger
        )
        {
            _configuratorFactory = configuratorFactory;
            _migratorFactory = migratorFactory;
            _logger = logger;
        }

        public override void Handle(
            GenerateCommandConfiguration cfg,
            CancellationToken token
        )
        {
            _logger.Debug($"Load '{cfg.ProjectName}' configuration from '{cfg.Assembly}'");
            var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
            var database = configurator.LoadConfiguration(cfg);

            _logger.Debug($"Convert '{cfg.ProjectName}' mapping schema to Database model");

            _logger.Debug($"Convert '{cfg.ProjectName}' Database model to Database view");

            // _logger.Debug($"Save new migration '{migrationName} ({migrationVersion})' to {cfg.Output}");

            /*
             generate flow:
             - load assembly types
             - find out all configurations
             - build MappingSchema with configurations
             - somehow build resulting Database object from MappingSchema
             - convert Database to DatabaseView
             - render DatabaseView
             */
        }
    }


    internal class GenerateCommandConfiguration : Configuration.Abstractions.IConfiguration, Migration.Abstractions.IConfiguration
    {
        [Option("cp", true)]
        [Help("Configuration provider.")]
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

        [Option("n", true)]
        [Help("Migration name.")]
        public string Name { get; set; }

        private string _assembly = string.Empty;
        private string _output = string.Empty;
    }
}