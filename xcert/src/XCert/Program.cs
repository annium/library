using System;
using System.Threading;
using Annium.Core.Entrypoint;

namespace XCert
{
    internal static class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            Console.WriteLine("When have time, this will be certs updating daemon");
        }

        internal static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}