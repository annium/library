using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using XRest.Clients.Csharp.Components;
using XRest.Core.Extensions;
using XRest.Source;
using XRest.Source.Components;

namespace XRest.Clients.Csharp.Commands;

internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>, ILogSubject<GenerateCommand>
{
    public override string Id => "gen";
    public override string Description => "generate client";
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
        this.Log().Info($"Generate client for {cfg.Server}");

        this.Log().Info($"Load model from {cfg.Server}");
        var model = await _loader.Load(cfg);

        this.Log().Info("Process api model to api view");
        var ns = cfg.Namespace.ToNamespace();
        var view = _processor.Process(ns, model);

        this.Log().Info($"Write api view to {cfg.Output}");
        _writer.Write(cfg.Output, view, cfg.TestClient);
        this.Log().Info($"Client written to {cfg.Output}");
    }
}

internal class GenerateCommandConfiguration : ISourceLoaderConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server to load model from.")]
    public Uri Server { get; set; } = default!;

    [Option("o", isRequired: true)]
    [Help("Output directory. Will be removed if exists.")]
    public string Output
    {
        get => _output;
        set => _output = Path.GetFullPath(value);
    }

    [Option("ns", isRequired: true)]
    [Help("Root namespace for created classes.")]
    public string Namespace { get; set; } = string.Empty;

    [Option("t")]
    [Help("Generate test client. Is not ensuring success code and returns data wrapped in responses.")]
    public bool TestClient { get; set; } = false;

    private string _output = string.Empty;
}