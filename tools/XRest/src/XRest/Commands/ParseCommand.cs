using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Serialization.Json;
using XRest.Core.Infrastructure.JsonConverters;
using XRest.Core.Tools;

namespace XRest.Commands
{
    internal class ParseCommand : Command<ParseCommandConfiguration>
    {
        public override string Id { get; } = "parse";
        public override string Description { get; } = "parse API";
        private readonly Loader _loader;
        private readonly Parser _parser;
        private readonly ILogger<ParseCommand> _logger;

        public ParseCommand(
            Loader loader,
            Parser parser,
            ILogger<ParseCommand> logger
        )
        {
            _loader = loader;
            _parser = parser;
            _logger = logger;
        }

        public override void Handle(ParseCommandConfiguration cfg, CancellationToken token)
        {
            _logger.Info($"Load '{cfg.ProjectName}' metadata from '{cfg.Assembly}'");
            var controllerTypes = _loader.LoadControllerTypes(cfg.Assembly);

            _logger.Info($"Parse '{cfg.ProjectName}' metadata");
            var api = _parser.Parse(controllerTypes);

            _logger.Debug($"Save '{cfg.ProjectName}' definition to '{cfg.Output}'");
            if (!Directory.Exists(Path.GetDirectoryName(cfg.Output)))
                Directory.CreateDirectory(Path.GetDirectoryName(cfg.Output));
            var serializer = StringSerializer.Configure(opts =>
            {
                opts.Converters.Add(new TypeJsonConverter());
                opts.WriteIndented = true;
            });
            File.WriteAllText(cfg.Output, serializer.Serialize(api));
        }
    }

    internal class ParseCommandConfiguration
    {
        [Option("a", true)]
        [Help("Path to API assembly.")]
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
        [Help("Output directory. Will be removed if exists.")]
        public string Output
        {
            get => _output;
            set => _output = Path.GetFullPath(value);
        }

        private string _assembly = string.Empty;
        private string _output = string.Empty;
    }
}