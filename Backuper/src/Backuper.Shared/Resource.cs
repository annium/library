using System;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Shared
{
    public class Resource<T> : ILogSubject
        where T : class
    {
        public ILogger Logger { get; }
        protected T Entity { get; }
        private readonly string _category;
        private readonly string _type;

        public Resource(
            T entity,
            string category,
            string type,
            ILogger logger
        )
        {
            Entity = entity;
            _category = category;
            _type = type;
            Logger = logger;
        }

        public async Task<TResult> SafeAsync<TResult>(string operation, Func<Task<TResult>> handleAsync)
        {
            try
            {
                Debug($"{operation} start");
                var result = await handleAsync();
                Debug($"{operation} succeed");

                return result;
            }
            catch
            {
                Debug($"{operation} failed");
                throw;
            }
        }

        public async Task SafeAsync(string operation, Func<Task> handleAsync)
        {
            try
            {
                Debug($"{operation} start");
                await handleAsync();
                Debug($"{operation} succeed");
            }
            catch
            {
                Debug($"{operation} failed");
                throw;
            }
        }

        private void Debug(string message) => this.Log().Debug(Msg(message));

        private string Msg(string message) => $"{_category} {_type}: {message}";
    }
}