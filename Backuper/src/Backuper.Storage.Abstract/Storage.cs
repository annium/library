using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Abstract
{
    public abstract class Storage
    {
        public string Name { get; }

        protected readonly ILogger logger;

        public Storage(
            string name,
            ILogger logger
        )
        {
            Name = name;
            this.logger = logger;
        }

        public abstract Task SetupAsync();
    }
}