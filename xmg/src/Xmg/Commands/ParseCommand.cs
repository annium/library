using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Serialization.Json;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Components;

namespace Xmg.Commands
{
    internal class ParseCommand : Command<ParseCommandConfiguration>
    {
        private readonly IConfiguratorFactory _configuratorFactory;
        private readonly ILogger<ParseCommand> _logger;
        public override string Id { get; } = "parse";
        public override string Description { get; } = "parse database configuration";

        public ParseCommand(
            IConfiguratorFactory configuratorFactory,
            ILogger<ParseCommand> logger
        )
        {
            _configuratorFactory = configuratorFactory;
            _logger = logger;
        }

        public override void Handle(
            ParseCommandConfiguration cfg,
            CancellationToken token
        )
        {
            _logger.Debug($"Load '{cfg.ProjectName}' configuration from '{cfg.Assembly}'");
            var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
            var database = configurator.LoadConfiguration(cfg);

            _logger.Debug($"Save '{cfg.ProjectName}' configuration to {cfg.Output}");
            var serializer = StringSerializer.Configure(opts => { opts.WriteIndented = true; });
            File.WriteAllText(cfg.Output, serializer.Serialize(database));
        }
    }


    internal class ParseCommandConfiguration : IConfiguration, Migration.Abstractions.IConfiguration
    {
        [Option("cp", true)]
        [Help("Configuration provider.")]
        public ConfigurationProvider ConfigurationProvider { get; set; }

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
        [Help("Output file. Will be created if missing.")]
        public string Output
        {
            get => _output;
            set => _output = Path.GetFullPath(value);
        }

        private string _assembly = string.Empty;
        private string _output = string.Empty;
    }
}