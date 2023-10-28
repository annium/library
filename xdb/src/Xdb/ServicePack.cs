using System;
using Annium.Core.DependencyInjection;
using YamlDotNet.Serialization.NamingConventions;

namespace Xdb;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();
        container
            .AddSerializers()
            .WithYaml(
                (s, d) =>
                {
                    s.WithNamingConvention(CamelCaseNamingConvention.Instance);
                    d.WithNamingConvention(CamelCaseNamingConvention.Instance);
                },
                isDefault: true
            );
        container.AddAssemblyLoader();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
