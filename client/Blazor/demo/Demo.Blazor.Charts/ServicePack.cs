using System;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Demo.Blazor.Charts.Domain.Converters;

namespace Demo.Blazor.Charts;

/// <summary>
/// Service pack for the Demo.Blazor.Charts application, configuring core services, charts, and custom JSON converters
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers all required services for the Blazor Charts demo application
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for accessing already registered services</param>
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // core
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
        container.AddHttpRequestFactory(true);
        container
            .AddSerializers()
            .WithJson(opts => opts.ResetConverters().AddConverter<CandleConverter>(), isDefault: true);

        // web
        container.AddRouting();
        container.AddHostHttpRequestFactory();
        container.AddApiServices();
        container.AddStates();
        container.AddStateFactory();
        container.AddCss();
        container.AddInterop();
        container.AddCharts();
        container.AddAntDesign();
    }

    /// <summary>
    /// Sets up logging configuration for the application
    /// </summary>
    /// <param name="provider">The service provider to configure</param>
    public override void Setup(IServiceProvider provider)
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
    }
}
