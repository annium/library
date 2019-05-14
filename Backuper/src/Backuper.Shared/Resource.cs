using System;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Shared
{
    public abstract class Resource
    {
        public string Category { get; }

        public string Type { get; }

        public string Name { get; }

        private readonly ILogger logger;

        public Resource(
            string category,
            string type,
            string name,
            ILogger logger
        )
        {
            Category = category;
            Type = type;
            Name = name;
            this.logger = logger;
        }

        protected async Task<T> SafeAsync<T>(string operation, Func<Task<T>> handleAsync)
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

        protected async Task SafeAsync(string operation, Func<Task> handleAsync)
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

        protected void debug(string message) => logger.Debug(msg(message));

        protected string msg(string message) => $"{Category} {Type} {Name}: {message}";
    }
}