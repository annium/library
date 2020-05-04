using System;
using System.Collections.Generic;
using System.Linq;
using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Abstractions.Components;

namespace Xmg.Configuration.Components
{
    internal class ConfiguratorFactory : IConfiguratorFactory
    {
        private readonly IEnumerable<IConfigurator> _configurators;

        public ConfiguratorFactory(
            IEnumerable<IConfigurator> configurators
        )
        {
            _configurators = configurators;
        }

        public IConfigurator GetForProvider(ConfigurationProvider provider) =>
            _configurators.SingleOrDefault(x => x.Provider == provider) ??
            throw new InvalidOperationException($"No configurator registered for provider {provider}");
    }
}