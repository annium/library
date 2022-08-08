using System;
using Annium.Core.DependencyInjection;
using XRest.Core.Components;
using XRest.Core.Components.Implementations;

namespace XRest.Core;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();
        container.AddJsonSerializers()
            .Configure(opts =>
            {
                opts.UseCamelCaseNamingPolicy();
            })
            .SetDefault();

        container.Add<ITemplateWriter, TemplateWriter>().Singleton();

        container.AddAssemblyLoader();
        container.AddResourceLoader();
    }
}