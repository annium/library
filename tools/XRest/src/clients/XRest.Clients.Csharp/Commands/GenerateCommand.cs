using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.Types.Extensions;
using XRest.Clients.Csharp.Components.Processors;
using XRest.Clients.Csharp.Components.Writers;
using XRest.Clients.Shared;
using XRest.Clients.Shared.Components;

namespace XRest.Clients.Csharp.Commands;

internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "gen";
    public static string Description => "generate client";
    public ILogger Logger { get; }
    private readonly IApiModelLoader _apiModelLoader;
    private readonly Writer _writer;

    public GenerateCommand(IApiModelLoader apiModelLoader, Writer writer, ILogger logger)
    {
        _apiModelLoader = apiModelLoader;
        _writer = writer;
        Logger = logger;
    }

    public override async Task HandleAsync(GenerateCommandConfiguration cfg, CancellationToken ct)
    {
        this.Info($"Generate client for {cfg.Server}");

        this.Info($"Load model from {cfg.Server}");
        var model = await _apiModelLoader.Load(cfg);

        this.Info("Process api model to api view");
        var ns = cfg.Namespace.ToNamespace();
        var view = Processor.Process(ns, model);

        this.Info($"Write api view to {cfg.Output}");
        _writer.Write(cfg.Output, view, cfg.TestClient);
        this.Info($"Client written to {cfg.Output}");
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
