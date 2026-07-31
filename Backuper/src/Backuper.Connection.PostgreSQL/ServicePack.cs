using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell;
using Annium.Logging;
using Backuper.Connection.Abstract;

namespace Backuper.Connection.PostgreSQL;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        Func<Configuration, IConnection> Factory(IServiceProvider sp) =>
            configuration => new ConnectionProxy(
                new Connection(configuration, sp.Resolve<IShell>()),
                configuration.Type,
                sp.Resolve<ILogger>()
            );

        container.Add(Factory).AsSelf().Singleton();

        return Task.CompletedTask;
    }
}
