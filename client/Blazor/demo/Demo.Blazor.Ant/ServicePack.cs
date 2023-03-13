using System;
using System.Text;
using Annium.Blazor.State;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;

namespace Demo.Blazor.Ant;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // core
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
        container.AddHttpRequestFactory().SetDefault();
        container.AddSerializers()
            .WithJson(opts => opts.ConfigureForOperations().ConfigureForNodaTime(), isDefault: true);

        // web
        container.AddRouting();
        container.AddHostHttpRequestFactory();
        container.AddApiServices();
        container.AddStorages();
        container.AddComponentFormStateFactory();
        container.AddCss();
        container.AddInterop();
        container.AddComponentFormStateFactory();
        container.AddAntDesign();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole(m =>
                {
                    var sb = new StringBuilder();
                    sb.Append(m.Subject());
                    if (m.Line != 0)
                        sb.Append(m.Location());

                    return $"{sb} >> {m.Message}";
                }
            )
        );
    }
}