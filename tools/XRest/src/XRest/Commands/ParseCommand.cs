using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using XRest.Source;
using XRest.Source.Components;
using Constants = XRest.Core.Constants;

namespace XRest.Commands;

internal class ParseCommand : AsyncCommand<ParseCommandConfiguration>, ILogSubject<ParseCommand>
{
    public override string Id { get; } = "parse";
    public override string Description { get; } = "parse API";
    private readonly ILoader _loader;
    private readonly ISerializer<string> _serializer;
    public ILogger<ParseCommand> Logger { get; }

    public ParseCommand(
        ILoader loader,
        ILogger<ParseCommand> logger,
        IIndex<SerializerKey, ISerializer<string>> serializers
    )
    {
        _loader = loader;
        Logger = logger;
        _serializer = serializers[SerializerKey.Create(Constants.IndexKey, MediaTypeNames.Application.Json)];
    }

    public override async Task HandleAsync(ParseCommandConfiguration cfg, CancellationToken ct)
    {
        this.Log().Info($"Load '{cfg.Server}' model");
        var model = await _loader.Load(cfg);

        this.Log().Debug($"Serialize '{cfg.Server}' model and write to stdout");
        Console.WriteLine(_serializer.Serialize(model));
    }
}

internal class ParseCommandConfiguration : ISourceLoaderConfiguration
{
    [Option("s")]
    [Help("Server to load model from.")]
    public Uri Server { get; set; } = default!;
}