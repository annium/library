using Xmg.Configuration.Abstractions;
using Xmg.Configuration.Abstractions.Components;

namespace Xmg.Configuration.Components
{
    public interface IConfiguratorFactory
    {
        IConfigurator GetForProvider(ConfigurationProvider provider);
    }
}