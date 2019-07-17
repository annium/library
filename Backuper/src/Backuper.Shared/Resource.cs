using System;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Shared
{
    public class Resource<T> where T : class
    {
        protected T Entity { get; }
        private readonly string category;
        private readonly string type;
        private readonly ILogger logger;

        public Resource(
            T entity,
            string category,
            string type,
            ILogger logger
        )
        {
            Entity = entity;
            this.category = category;
            this.type = type;
            this.logger = logger;
        }

        public async Task<TResult> SafeAsync<TResult>(string operation, Func<Task<TResult>> handleAsync)
        {
            try
            {
                debug($"{operation} start");
                var result = await handleAsync();
                debug($"{operation} succeed");

                return result;
            }
            catch
            {
                debug($"{operation} failed");
                throw;
            }
        }

        public async Task SafeAsync(string operation, Func<Task> handleAsync)
        {
            try
            {
                debug($"{operation} start");
                await handleAsync();
                debug($"{operation} succeed");
            }
            catch
            {
                debug($"{operation} failed");
                throw;
            }
        }

        private void debug(string message) => logger.Debug(msg(message));

        private string msg(string message) => $"{category} {type}: {message}";
    }
}