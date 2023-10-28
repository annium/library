using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Abstractions.Components;
using Xmg.Core.Models;

namespace Xmg.Configuration.linq2db.Components;

internal class Configurator : IConfigurator
{
    private readonly ILoader _loader;
    private readonly IMetadataProcessor _metadataProcessor;
    public ConfigurationProvider Provider => ConfigurationProvider.Linq2db;

    public Configurator(ILoader loader, IMetadataProcessor metadataProcessor)
    {
        _loader = loader;
        _metadataProcessor = metadataProcessor;
    }

    public Database LoadConfiguration(Config cfg)
    {
        var metadata = _loader.LoadMetadata(cfg.Assembly);
        var database = _metadataProcessor.Process(metadata);

        return database;
    }
}
