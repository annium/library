using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.AspNetCore.Extensions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.NodaTime.Serialization.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.XRest.Sources.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.XRest.Demo.Server;

internal class ServicePack : ServicePackBase
{
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddMapper();

        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRealTime().SetDefault();
        container
            .AddSerializers()
            .WithJson(opts => opts.ConfigureForOperations().ConfigureForNodaTime(), isDefault: true);
        container.AddLogging();
        container.AddXRest();

        // server
        container.Collection.AddCors();
        container.Collection.AddControllers().AddDefaultJsonOptions();
        container.Add(new WebHostConfiguration()).AsSelf().Singleton();

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
