using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.Abstract;

public class ConnectionFactory
{
    private readonly IServiceProvider _provider;

    public ConnectionFactory(
        IServiceProvider provider
    )
    {
        _provider = provider;
    }

    public IConnection CreateConnection(ConfigurationBase configuration)
    {
        var factoryType = typeof(Func<,>).MakeGenericType(configuration.GetType(), typeof(IConnection));

        var factory = (Delegate) _provider.GetRequiredService(factoryType);

        try
        {
            var storage = (IConnection) factory.DynamicInvoke(configuration)!;

            return storage;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }
    }
}