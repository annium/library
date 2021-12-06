using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;
using XSass.Internal;
using XSass.Internal.Components;

namespace XSass;

internal class Program
{
    private static async Task Run(
        IServiceProvider provider,
        CancellationToken ct
    )
    {
        var configuration = provider.Resolve<Configuration>();
        var logSubject = provider.Resolve<ILogSubject<Program>>();
        logSubject.Log().Info($"Sass compilation at: {configuration.Root}");
        await provider.Resolve<Crawler>().Run(configuration.Root);
        logSubject.Log().Info("Sass compilation succeed");
    }

    internal static Task<int> Main() => new Entrypoint()
        .UseServicePack<ServicePack>()
        .Run(Run);
}