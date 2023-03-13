using System;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;

namespace Demo.Blazor.Interop;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // core
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper();

        // web
        container.AddRouting();
        container.AddCss();
        container.AddInterop();
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