using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using XRest.Core.Views;
using XRest.Sources;
using XRest.Sources.Components;

namespace XRest.Commands;

internal class ParseCommand : AsyncCommand<ParseCommandConfiguration>, ILogSubject
{
    public override string Id { get; } = "parse";
    public override string Description { get; } = "parse API";
    private readonly ILoader _loader;
    private readonly ISerializer<string> _serializer;
    private readonly IMapper _mapper;
    public ILogger Logger { get; }

    public ParseCommand(
        ILoader loader,
        IMapper mapper,
        ILogger<ParseCommand> logger,
        IIndex<SerializerKey, ISerializer<string>> serializers
    )
    {
        _loader = loader;
        _mapper = mapper;
        Logger = logger;
        _serializer = serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)];
    }

    public override async Task HandleAsync(ParseCommandConfiguration cfg, CancellationToken ct)
    {
        this.Log().Info($"Load '{cfg.ProjectName}' model");
        var model = await _loader.Load(cfg);

        this.Log().Debug($"Save '{cfg.ProjectName}' model view to '{cfg.Output}'");
        var view = _mapper.Map<ApiView>(model);
        SaveView(cfg.Output, view);
    }

    private void SaveView(string output, ApiView view)
    {
        if (!Directory.Exists(Path.GetDirectoryName(output)))
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        File.WriteAllText(output, _serializer.Serialize(view));
    }
}

internal class ParseCommandConfiguration : ISourceLoaderConfiguration
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
    [Help("Output file. Will be rewritten if exists.")]
    public string Output
    {
        get => _output;
        set => _output = Path.GetFullPath(value);
    }

    private string _assembly = string.Empty;
    private string _output = string.Empty;
}