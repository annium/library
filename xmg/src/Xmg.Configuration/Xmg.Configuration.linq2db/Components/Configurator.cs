using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Abstractions.Components;
using Xmg.Core.Models;

namespace Xmg.Configuration.linq2db.Components
{
    internal class Configurator : IConfigurator
    {
        public ConfigurationProvider Provider => ConfigurationProvider.linq2db;

        public Database LoadConfiguration(IConfiguration cfg)
        {
            throw new System.NotImplementedException();
        }
    }
}