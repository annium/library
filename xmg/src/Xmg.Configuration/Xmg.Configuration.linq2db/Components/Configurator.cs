using System;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Abstractions.Components;
using Xmg.Core.Models;

namespace Xmg.Configuration.linq2db.Components
{
    internal class Configurator : IConfigurator
    {
        private readonly ILoader _loader;
        public ConfigurationProvider Provider => ConfigurationProvider.linq2db;

        public Configurator(
            ILoader loader
        )
        {
            _loader = loader;
        }


        public Database LoadConfiguration(IConfiguration cfg)
        {
            var mappingSchema = _loader.LoadMappingSchema(cfg.Assembly);

            throw new NotImplementedException();
        }
    }
}