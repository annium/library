using Xmg.Core.Models;

namespace Xmg.Configuration.Abstractions.Components
{
    public interface IConfigurator
    {
        ConfigurationProvider Provider { get; }
        Database LoadConfiguration(Config cfg);
    }
}