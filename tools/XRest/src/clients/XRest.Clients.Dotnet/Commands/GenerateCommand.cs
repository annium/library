using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Clients.Dotnet.Components;
using XRest.Core.Models;
using XRest.Sources;
using XRest.Sources.Components;

namespace XRest.Clients.Dotnet.Commands;

internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>, ILogSubject<GenerateCommand>
{
    public override string Id { get; } = "gen";
    public override string Description { get; } = "generate client";
    public ILogger<GenerateCommand> Logger { get; }
    private readonly ILoader _loader;
    private readonly IProcessor _processor;
    private readonly IWriter _writer;

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
        Logger = logger;
    }

    public override async Task HandleAsync(GenerateCommandConfiguration cfg, CancellationToken ct)
    {
        this.Log().Info($"Generate '{cfg.ProjectName}' client");

        this.Log().Info($"Load '{cfg.ProjectName}' model");
        var model = await _loader.Load(cfg);

        this.Log().Info("Process api model to api view");
        var ns = Namespace.New(string.IsNullOrWhiteSpace(cfg.Namespace) ? Path.GetFileName(cfg.Output) : cfg.Namespace);
        var view = _processor.Process(ns, model);

        this.Log().Info($"Write api view to {cfg.Output}");
        _writer.Write(cfg.Output, view, cfg.TestClient);
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

    [Option("ns")]
    [Help("Root namespace for created classes.")]
    public string Namespace { get; set; } = string.Empty;

    [Option("t")]
    [Help("Generate test client. Is not ensuring success code and returns data wrapped in responses.")]
    public bool TestClient { get; set; } = false;

    private string _assembly = string.Empty;
    private string _output = string.Empty;
}