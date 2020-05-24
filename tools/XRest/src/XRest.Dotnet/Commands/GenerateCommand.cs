using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Core.Components;
using XRest.Dotnet.Components;

namespace XRest.Dotnet.Commands
{
    internal class GenerateCommand : Command<GenerateCommandConfiguration>
    {
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate client";
        private readonly ILoader _loader;
        private readonly IParser _parser;
        private readonly IProcessor _processor;
        private readonly IWriter _writer;
        private readonly ILogger<GenerateCommand> _logger;

        public GenerateCommand(
            ILoader loader,
            IParser parser,
            IProcessor processor,
            IWriter writer,
            ILogger<GenerateCommand> logger
        )
        {
            _loader = loader;
            _parser = parser;
            _processor = processor;
            _writer = writer;
            _logger = logger;
        }

        public override void Handle(GenerateCommandConfiguration cfg, CancellationToken token)
        {
            _logger.Info($"Generate '{cfg.ProjectName}' client");

            _logger.Info($"Load metadata from '{cfg.Assembly}'");
            var controllerTypes = _loader.LoadControllerTypes(cfg.Assembly);

            _logger.Info("Parse metadata");
            var api = _parser.Parse(controllerTypes);

            _logger.Info("Process api model to api view");
            var view = _processor.Process(Path.GetFileName(cfg.Output), api);

            _logger.Info($"Write api view to {cfg.Output}");
            _writer.Write(cfg.Output, view, cfg.TestClient);
            _logger.Info($"Client written to {cfg.Output}");
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

        [Option("t")]
        [Help("Generate test client. Is not ensuring success code and returns data wrapped in responses.")]
        public bool TestClient { get; set; }

        private string _assembly = string.Empty;
        private string _output = string.Empty;
    }
}