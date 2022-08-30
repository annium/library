using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Blazor.Charts;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // core
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
        container.AddHttpRequestFactory();
        container.AddJsonSerializers()
            .Configure(opts => opts.ConfigureForOperations().ConfigureForNodaTime())
            .SetDefault();

        // web
        container.AddRouting();
        container.AddHostHttpRequestFactory();
        container.AddApiServices();
        container.AddStorages();
        container.AddComponentFormStateFactory();
        container.AddCss();
        container.Collection.AddAntDesign();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}