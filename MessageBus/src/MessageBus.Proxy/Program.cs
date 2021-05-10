using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using NetMQ.Sockets;

namespace MessageBus.Proxy
{
    public static class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            var cfg = provider.Resolve<Configuration>();
            Console.WriteLine($"Start proxy with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

            using var subscriber = new XSubscriberSocket();
            subscriber.Bind(cfg.PubEndpoint);

            using var publisher = new XPublisherSocket();
            publisher.Bind(cfg.SubEndpoint);

            var proxy = new NetMQ.Proxy(subscriber, publisher);
            ct.Register(proxy.Stop);
            proxy.Start();
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
