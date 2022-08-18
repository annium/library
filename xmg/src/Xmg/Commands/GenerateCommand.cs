using System.IO;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Components;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Components;
using Config = Xmg.Configuration.Abstractions.Config;

namespace Xmg.Commands;

internal class GenerateCommand : Command<GenerateCommandConfig>, ILogSubject<GenerateCommand>
{
    public override string Id { get; } = "gen";
    public override string Description { get; } = "generate Migration";
    public ILogger<GenerateCommand> Logger { get; }
    private readonly ITimeProvider _timeProvider;
    private readonly IConfiguratorFactory _configuratorFactory;
    private readonly IMigratorFactory _migratorFactory;

    public GenerateCommand(
        ITimeProvider timeProvider,
        IConfiguratorFactory configuratorFactory,
        IMigratorFactory migratorFactory,
        ILogger<GenerateCommand> logger
    )
    {
        _timeProvider = timeProvider;
        _configuratorFactory = configuratorFactory;
        _migratorFactory = migratorFactory;
        Logger = logger;
    }

    public override void Handle(
        GenerateCommandConfig cfg,
        CancellationToken ct
    )
    {
        this.Log().Debug($"Load '{cfg.ProjectName}' configuration from '{cfg.Assembly}'");

        var configurator = _configuratorFactory.GetForProvider(cfg.ConfigurationProvider);
        var configurationCfg = new Config(cfg.Assembly);
        var database = configurator.LoadConfiguration(configurationCfg);

        var migrationName = cfg.Name;
        var migrationVersion = _timeProvider.Now.ToDateTimeOffset().ToString("yyyyMMdd");


        this.Log().Debug($"Create '{cfg.ProjectName}' migration '{migrationName}' ({migrationVersion}) from Database model");

        var migrator = _migratorFactory.GetForProvider(cfg.MigrationProvider);
        var migrationCfg = new Migration.Abstractions.Config(cfg.Namespace, migrationName, migrationVersion);
        var migration = migrator.CreateMigration(database, migrationCfg);

        this.Log().Debug($"Create '{cfg.ProjectName}' migration '{migrationName}' ({migrationVersion}) files");
        if (!Directory.Exists(cfg.Output))
            Directory.CreateDirectory(cfg.Output);
        foreach (var (file, content) in migration.Files)
            File.WriteAllText(Path.Combine(cfg.Output, file), content);
    }
}


internal class GenerateCommandConfig
{
    [Option("cp", true)]
    [Help("Config provider.")]
    public ConfigurationProvider ConfigurationProvider { get; set; }

    [Option("mp", true)]
    [Help("Migration provider.")]
    public MigrationProvider MigrationProvider { get; set; }

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
    [Help("Output directory. Will be created if missing.")]
    public string Output
    {
        get => _output;
        set => _output = Path.GetFullPath(value);
    }

    [Option("ns", true)]
    [Help("Migrations namespace.")]
    public string Namespace { get; set; } = string.Empty;

    [Option("n", true)]
    [Help("Migration name.")]
    public string Name { get; set; } = string.Empty;

    private string _assembly = string.Empty;
    private string _output = string.Empty;
}