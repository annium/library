using System;
using System.IO;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Loader;
using Annium.Extensions.Arguments;
using Annium.linq2db.PostgreSql;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Yaml.Constants;

namespace Xdb.Commands.Migrate;

internal class MigratePostgresqlCommand
    : Command<MigratePostgresqlCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "postgresql";
    public static string Description => "migrate postgresql database";
    public ILogger Logger { get; }
    private readonly IAssemblyLoaderBuilder _assemblyLoaderBuilder;
    private readonly ISerializer<string> _serializer;

    public MigratePostgresqlCommand(IServiceProvider sp, IAssemblyLoaderBuilder assemblyLoaderBuilder, ILogger logger)
    {
        Logger = logger;
        _assemblyLoaderBuilder = assemblyLoaderBuilder;
        var serializerKey = SerializerKey.CreateDefault(Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
    }

    public override void Handle(MigratePostgresqlCommandConfiguration cfg, CancellationToken ct)
    {
        var config = _serializer.Deserialize<PostgreSqlConfiguration>(File.ReadAllText(cfg.Config));

        var engine = Migrator.Instance.ForPostgresql(config.ConnectionString, cfg.Schema);

        if (!string.IsNullOrWhiteSpace(cfg.Directory))
            engine.WithScriptsFromDirectory(cfg.Directory);
        else if (!string.IsNullOrWhiteSpace(cfg.Assembly))
        {
            var loader = _assemblyLoaderBuilder.UseFileSystemLoader(Path.GetDirectoryName(cfg.Assembly)!).Build();
            var assembly = loader.Load(Path.GetFileNameWithoutExtension(cfg.Assembly));
            engine.WithScriptsFromAssembly(assembly);
        }
        else
            throw new ArgumentException("Specify either directory or assembly with migrations");

        engine.Execute();
    }
}

internal sealed record MigratePostgresqlCommandConfiguration : MigrateCommandConfigurationBase
{
    [Position(2)]
    [Help("schema for migrations journal")]
    public string Schema { get; set; } = string.Empty;
}
