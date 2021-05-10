using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Infrastructure.MessageBus.Node;

namespace MessageBus.Sink
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            CancellationToken ct
        )
        {
            var logger = provider.Resolve<ILogger<Program>>();
            var socket = provider.Resolve<IMessageBusSocket>();

            var cfg = provider.Resolve<EndpointsConfiguration>();
            Console.WriteLine($"Start sink with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

            socket.Subscribe(x => logger.Info(x));

            await ct;
        }

        public static Task<int> Main() => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run);
    }
}
