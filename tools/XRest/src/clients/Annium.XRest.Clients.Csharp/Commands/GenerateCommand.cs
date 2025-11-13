using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.Types.Extensions;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Annium.XRest.Clients.Csharp.Components.Writers;
using Annium.XRest.Clients.Shared;
using Annium.XRest.Clients.Shared.Components;

namespace Annium.XRest.Clients.Csharp.Commands;

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
        this.Info("Generate client for {server}", cfg.Server);

        this.Info("Load model from {server}", cfg.Server);
        var model = await _apiModelLoader.LoadAsync(cfg);

        this.Info("Process api model to api view");
        var ns = cfg.Namespace.ToNamespace();
        var view = Processor.Process(ns, model);

        this.Info<string>("Write api view to {output}", cfg.Output);
        _writer.Write(cfg.Output, view, cfg.TestClient);
        this.Info<string>("Client written to {output}", cfg.Output);
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
        get;
        set => field = Path.GetFullPath(value);
    } = string.Empty;

    [Option("ns", isRequired: true)]
    [Help("Root namespace for created classes.")]
    public string Namespace { get; set; } = string.Empty;

    [Option("t")]
    [Help("Generate test client. Is not ensuring success code and returns data wrapped in responses.")]
    public bool TestClient { get; set; } = false;
}
