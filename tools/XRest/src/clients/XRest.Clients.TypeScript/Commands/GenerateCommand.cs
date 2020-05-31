using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Clients.TypeScript.Components;
using XRest.Sources;
using XRest.Sources.Components;

namespace XRest.Clients.TypeScript.Commands
{
    internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>
    {
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate client";
        private readonly ILoader _loader;
        private readonly IProcessor _processor;
        private readonly IWriter _writer;
        private readonly ILogger<GenerateCommand> _logger;

        public GenerateCommand(
            ILoader loader,
            IProcessor processor,
            IWriter writer,
            ILogger<GenerateCommand> logger
        )
        {
            _loader = loader;
            _processor = processor;
            _writer = writer;
            _logger = logger;
        }

        public override async Task HandleAsync(GenerateCommandConfiguration cfg, CancellationToken token)
        {
            _logger.Info($"Generate '{cfg.ProjectName}' client");

            _logger.Info($"Load '{cfg.ProjectName}' model");
            var model = await _loader.Load(cfg);

            _logger.Info("Process api model to api view");
            var view = _processor.Process(model);

            _logger.Info($"Write api view to {cfg.Output}");
            _writer.Write(cfg.Output, view);
            _logger.Info($"Client written to {cfg.Output}");
        }
    }

    internal class GenerateCommandConfiguration : ISourceLoaderConfiguration
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

        [Option("s")]
        [Help("Server to load model from.")]
        public Uri Server { get; set; } = default!;

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