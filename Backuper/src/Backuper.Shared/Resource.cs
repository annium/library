using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Backuper.Shared;

public abstract class Resource<T> : ILogSubject
    where T : class
{
    public ILogger Logger { get; }
    protected T Entity { get; }
    private readonly string _category;
    private readonly string _type;

    protected Resource(T entity, string category, string type, ILogger logger)
    {
        Entity = entity;
        _category = category;
        _type = type;
        Logger = logger;
    }

    protected async Task<TResult> SafeAsync<TResult>(string operation, Func<Task<TResult>> handleAsync)
    {
        try
        {
            this.Debug($"{operation} start");
            var result = await handleAsync();
            this.Debug($"{operation} succeed");

            return result;
        }
        catch
        {
            this.Debug($"{operation} failed");
            throw;
        }
    }

    protected async Task SafeAsync(string operation, Func<Task> handleAsync)
    {
        try
        {
            this.Debug($"{operation} start");
            await handleAsync();
            this.Debug($"{operation} succeed");
        }
        catch
        {
            this.Debug($"{operation} failed");
            throw;
        }
    }
}
