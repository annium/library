using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using XRest.Clients.Shared;
using XRest.Clients.Shared.Components;
using Constants = XRest.Core.Constants;

namespace XRest.Commands;

internal class ParseCommand : AsyncCommand<ParseCommandConfiguration>
{
    public override string Id => "parse";
    public override string Description => "parse API";
    private readonly IApiModelLoader _apiModelLoader;
    private readonly ISerializer<string> _serializer;
    public ILogger<ParseCommand> Logger { get; }

    public ParseCommand(
        IApiModelLoader apiModelLoader,
        ILogger<ParseCommand> logger,
        IIndex<SerializerKey, ISerializer<string>> serializers
    )
    {
        _apiModelLoader = apiModelLoader;
        Logger = logger;
        _serializer = serializers[SerializerKey.Create(Constants.IndexKey, MediaTypeNames.Application.Json)];
    }

    public override async Task HandleAsync(ParseCommandConfiguration cfg, CancellationToken ct)
    {
        var model = await _apiModelLoader.Load(cfg);
        Console.WriteLine(_serializer.Serialize(model));
    }
}

internal class ParseCommandConfiguration : ISourceLoaderConfiguration
{
    [Option("s")]
    [Help("Server to load model from.")]
    public Uri Server { get; set; } = default!;
}