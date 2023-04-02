using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Clients.Shared;
using XRest.Clients.Shared.Components;
using XRest.Clients.TypeScript.Components;

namespace XRest.Clients.TypeScript.Commands;

internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>, ICommandDescriptor, ILogSubject<GenerateCommand>
{
    public static string Id => "gen";
    public static string Description => "generate client";
    public ILogger<GenerateCommand> Logger { get; }
    private readonly IApiModelLoader _apiModelLoader;
    private readonly IProcessor _processor;
    private readonly IWriter _writer;

    public GenerateCommand(
        IApiModelLoader apiModelLoader,
        IProcessor processor,
        IWriter writer,
        ILogger<GenerateCommand> logger
    )
    {
        _apiModelLoader = apiModelLoader;
        _processor = processor;
        _writer = writer;
        Logger = logger;
    }

    public override async Task HandleAsync(GenerateCommandConfiguration cfg, CancellationToken ct)
    {
        this.Log().Info($"Generate '{cfg.ProjectName}' client");

        this.Log().Info($"Load '{cfg.ProjectName}' model");
        var model = await _apiModelLoader.Load(cfg);

        this.Log().Info("Process api model to api view");
        var view = _processor.Process(model);

        this.Log().Info($"Write api view to {cfg.Output}");
        _writer.Write(cfg.Output, view);
        this.Log().Info($"Client written to {cfg.Output}");
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