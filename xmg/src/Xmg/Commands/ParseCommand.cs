using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Components;

namespace Xmg.Commands;

internal class ParseCommand : Command<ParseCommandConfig>, ICommandDescriptor, ILogSubject
{
    public static string Id => "parse";
    public static string Description => "parse database configuration";
    public ILogger Logger { get; }
    private readonly IConfiguratorFactory _configuratorFactory;
    private readonly ISerializer<string> _serializer;

    public ParseCommand(IServiceProvider sp, IConfiguratorFactory configuratorFactory, ILogger logger)
    {
        Logger = logger;
        _configuratorFactory = configuratorFactory;
        var serializerKey = SerializerKey.CreateDefault(MediaTypeNames.Application.Json);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
    }

    public override void Handle(ParseCommandConfig cfg, CancellationToken ct)
    {
        this.Debug($"Load '{cfg.ProjectName}' configuration from '{cfg.Assembly}'");
        var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
        var database = configurator.LoadConfiguration(new Config(cfg.Assembly));

        this.Debug($"Save '{cfg.ProjectName}' configuration to {cfg.Output}");
        File.WriteAllText(cfg.Output, _serializer.Serialize(database));
    }
}

internal class ParseCommandConfig
{
    [Option("cp", true)]
    [Help("Config provider.")]
    public ConfigurationProvider ConfigurationProvider { get; set; }

    [Option("a", true)]
    [Help("Path to Db assembly.")]
    public string Assembly
    {
        get => _assembly;
        set
        {
            _assembly = Path.GetFullPath(value);
            ProjectName = Path.GetFileNameWithoutExtension(value);
        }
    }

    public string ProjectName { get; private set; } = string.Empty;

    [Option("o", true)]
    [Help("Output file. Will be created if missing.")]
    public string Output
    {
        get => _output;
        set => _output = Path.GetFullPath(value);
    }

    private string _assembly = string.Empty;
    private string _output = string.Empty;
}
