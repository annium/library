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
        this.Debug<string, string>(
            "Load '{projectName}' configuration from '{assembly}'",
            cfg.ProjectName,
            cfg.Assembly
        );
        var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
        var database = configurator.LoadConfiguration(new Config(cfg.Assembly));

        this.Debug<string, string>("Save '{projectName}' configuration to {output}", cfg.ProjectName, cfg.Output);
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
        get;
        set
        {
            field = Path.GetFullPath(value);
            ProjectName = Path.GetFileNameWithoutExtension(value);
        }
    } = string.Empty;

    public string ProjectName { get; private set; } = string.Empty;

    [Option("o", true)]
    [Help("Output file. Will be created if missing.")]
    public string Output
    {
        get;
        set => field = Path.GetFullPath(value);
    } = string.Empty;
}
