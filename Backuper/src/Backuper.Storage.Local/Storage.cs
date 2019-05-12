using System.IO;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Local
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }

        public override Task SetupAsync()
        {
            Debug("setup");

            try
            {
                Directory.CreateDirectory(Path.Combine(cfg.Path, Name));
                Debug("setup done");
            }
            catch
            {
                Debug("setup failed");
                throw;
            }

            return Task.CompletedTask;
        }

        private void Debug(string message) => logger.Debug(msg(message));

        private string msg(string message) => $"Storage local {Name}: {message}";
    }
}