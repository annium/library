using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Blazor.Routing;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace Demo.Blazor.Interop;

/// <summary>
/// Service pack for the Demo.Blazor.Interop application, configuring core services and JavaScript interop capabilities
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers all required services for the Blazor Interop demo application
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for accessing already registered services</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
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

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets up logging configuration for the application
    /// </summary>
    /// <param name="provider">The service provider to configure</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route =>
            route.UseConsole(m =>
            {
                var sb = new StringBuilder();
                sb.Append(m.Subject());
                if (m.Line != 0)
                    sb.Append(m.Location());

                return $"{sb} >> {m.Message}";
            })
        );

        return Task.CompletedTask;
    }
}
