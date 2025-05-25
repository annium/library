using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Backuper.Shared;

public abstract class Resource<T> : ILogSubject
    where T : class
{
    public ILogger Logger { get; }
    protected T Entity { get; }

    protected Resource(T entity, string category, string type, ILogger logger)
    {
        Entity = entity;
        Logger = logger;
    }

    protected async Task<TResult> SafeAsync<TResult>(string operation, Func<Task<TResult>> handleAsync)
    {
        try
        {
            this.Debug<string>("{operation} start", operation);
            var result = await handleAsync();
            this.Debug<string>("{operation} succeed", operation);

            return result;
        }
        catch
        {
            this.Debug<string>("{operation} failed", operation);
            throw;
        }
    }

    protected async Task SafeAsync(string operation, Func<Task> handleAsync)
    {
        try
        {
            this.Debug<string>("{operation} start", operation);
            await handleAsync();
            this.Debug<string>("{operation} succeed", operation);
        }
        catch
        {
            this.Debug<string>("{operation} failed", operation);
            throw;
        }
    }
}
