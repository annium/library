using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.S3
{
    public class Storage : Abstract.Storage
    {
        public Storage(
            string name,
            ILogger logger
        ) : base(name, logger)
        {

        }

        public override Task SetupAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}