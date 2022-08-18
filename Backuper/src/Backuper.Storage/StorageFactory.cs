using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Storage;

public class StorageFactory
{
    private readonly IServiceProvider _sp;
    private readonly IStorageFactory _storageFactory;

    public StorageFactory(
        IServiceProvider sp,
        IStorageFactory storageFactory
    )
    {
        _sp = sp;
        _storageFactory = storageFactory;
    }

    public IStorage CreateStorage(ConfigurationBase configuration)
    {
        var storage = _storageFactory.CreateStorage(configuration);

        return new StorageProxy(
            storage,
            configuration.Type,
            (ILogger<StorageProxy>) _sp.GetRequiredService(typeof(ILogger<>).MakeGenericType(storage.GetType()))
        );
    }
}