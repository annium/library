using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using XRest.Clients.Shared;
using XRest.Clients.Shared.Components;
using Constants = XRest.Core.Constants;

namespace XRest.Commands;

internal class ParseCommand : AsyncCommand<ParseCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "parse";
    public static string Description => "parse API";
    public ILogger Logger { get; }
    private readonly IApiModelLoader _apiModelLoader;
    private readonly ISerializer<string> _serializer;

    public ParseCommand(IServiceProvider sp, IApiModelLoader apiModelLoader, ILogger logger)
    {
        _apiModelLoader = apiModelLoader;
        Logger = logger;
        var serializerKey = SerializerKey.Create(Constants.IndexKey, MediaTypeNames.Application.Json);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
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
