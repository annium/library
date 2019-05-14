using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Local
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        private readonly string dir;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base("local", name, logger)
        {
            this.cfg = cfg;
            this.dir = Path.Combine(cfg.Path, Name);
        }

        protected override Task DoSetupAsync()
        {
            Directory.CreateDirectory(dir);

            return Task.CompletedTask;
        }

        protected override Task<string[]> DoListAsync()
        {
            var entries = Directory.GetFiles(dir).Select(e => Path.GetRelativePath(dir, e)).ToArray();

            return Task.FromResult(entries);
        }

        protected override async Task DoUploadAsync(string path, string name)
        {
            var target = Path.Combine(dir, name);
            if (File.Exists(target))
                throw new IOException($"File {name} already exists");

            using(var srcStream = File.Open(path, FileMode.Open))
            using(var tgtStream = File.Open(target, FileMode.CreateNew))
            {
                srcStream.Position = 0;
                await srcStream.CopyToAsync(tgtStream);
            }
        }

        protected override async Task DoDownloadAsync(string name, string path)
        {
            var target = Path.Combine(dir, name);
            if (!File.Exists(target))
                throw new FileNotFoundException($"File {name} doesn't exist");

            using(var srcStream = File.Open(target, FileMode.Open))
            using(var tgtStream = File.Open(path, FileMode.CreateNew))
            {
                srcStream.Position = 0;
                await srcStream.CopyToAsync(tgtStream);
            }
        }

        protected override Task DoDeleteAsync(string name)
        {
            var target = Path.Combine(dir, name);
            if (!File.Exists(target))
                throw new FileNotFoundException($"File {name} doesn't exist");

            File.Delete(target);

            return Task.CompletedTask;
        }
    }
}