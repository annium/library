using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Core.Components;

namespace XRest.TypeScript.Commands
{
    internal class GenerateCommand : Command<GenerateCommandConfiguration>
    {
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate client";
        private readonly ILoader _loader;
        private readonly IParser _parser;
        private readonly ILogger<GenerateCommand> _logger;

        public GenerateCommand(
            ILoader loader,
            IParser parser,
            ILogger<GenerateCommand> logger
        )
        {
            _loader = loader;
            _parser = parser;
            _logger = logger;
        }

        public override void Handle(GenerateCommandConfiguration cfg, CancellationToken token)
        {
            _logger.Info($"Load '{cfg.ProjectName}' metadata from '{cfg.Assembly}'");
            var controllerTypes = _loader.LoadControllerTypes(cfg.Assembly);

            _logger.Info($"Parse '{cfg.ProjectName}' metadata");
            var api = _parser.Parse(controllerTypes);

            // run same flow as parse to obtain ApiModel
            //
        }
    }

    internal class GenerateCommandConfiguration
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