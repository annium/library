using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Backuper.Connection.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.PostgreSQL
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            Func<IServiceProvider, Func<Configuration, IConnection>> factory =
                sp => configuration => new ConnectionProxy(
                    new Connection(
                        configuration,
                        sp.GetRequiredService<IShell>()
                    ),
                    configuration.Type,
                    sp.GetRequiredService<ILogger<Connection>>()
                );

            services.AddSingleton<Func<Configuration, IConnection>>(factory);
        }
    }
}