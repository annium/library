using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage;

public class StorageFactory
{
    private readonly IServiceProvider provider;
    private readonly IStorageFactory storageFactory;

    public StorageFactory(
        IServiceProvider provider,
        IStorageFactory storageFactory
    )
    {
        this.provider = provider;
        this.storageFactory = storageFactory;
    }

    public IStorage CreateStorage(ConfigurationBase configuration)
    {
        var storage = storageFactory.CreateStorage(configuration);

        return new StorageProxy(
            storage,
            configuration.Type,
            (ILogger) provider.GetRequiredService(typeof(ILogger<>).MakeGenericType(storage.GetType()))
        );
    }
}