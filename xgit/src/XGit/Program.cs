using System;
using System.Threading;
using Annium.Core.Entrypoint;

namespace XGit
{
    internal static class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            Console.WriteLine("Hello from XGit");
        }

        internal static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}