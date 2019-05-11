using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Extensions.Entrypoint;

namespace xdomains
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            new Commander(provider).Run<Commands.Group>(args, token);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<Annium.Extensions.Arguments.ServicePack>()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}