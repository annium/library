using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;
using XSass.Internal;
using XSass.Internal.Components;

namespace XSass
{
    internal class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            CancellationToken token
        )
        {
            var configuration = provider.Resolve<Configuration>();
            var logger = provider.Resolve<ILogger<Program>>();
            logger.Info($"Sass compilation at: {configuration.Root}");
            await provider.Resolve<Crawler>().Run(configuration.Root);
            logger.Info("Sass compilation succeed");
        }

        internal static Task<int> Main() => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run);
    }
}