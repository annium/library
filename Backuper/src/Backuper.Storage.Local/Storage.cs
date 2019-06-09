using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Local
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        private string dir;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base("local", name, logger)
        {
            this.cfg = cfg;
        }

        protected override Task DoSetupAsync()
        {
            if (Path.GetFullPath(cfg.Path) != cfg.Path)
                throw new InvalidOperationException($"Path {cfg.Path} is not absolute");

            dir = cfg.Path;
            Directory.CreateDirectory(dir);

            return Task.CompletedTask;
        }

        protected override Task<string[]> DoListAsync(string folder)
        {
            var location = Path.Combine(dir, folder);
            if (!Directory.Exists(location))
                return Task.FromResult(Array.Empty<string>());

            var entries = Directory.GetFiles(location).Select(e => Path.GetRelativePath(location, e)).ToArray();

            return Task.FromResult(entries);
        }

        protected override async Task DoUploadAsync(string source, string folder, string name)
        {
            var target = Path.Combine(dir, folder, name);
            if (File.Exists(target))
                throw new IOException($"File {name} already exists");

            Directory.CreateDirectory(Path.Combine(dir, folder));

            using(var srcStream = File.Open(source, FileMode.Open))
            using(var tgtStream = File.Open(target, FileMode.CreateNew))
            {
                srcStream.Position = 0;
                await srcStream.CopyToAsync(tgtStream);
            }
        }

        protected override async Task DoDownloadAsync(string folder, string name, string target)
        {
            var source = Path.Combine(dir, folder, name);
            if (!File.Exists(source))
                throw new FileNotFoundException($"File {name} doesn't exist");

            using(var srcStream = File.Open(source, FileMode.Open))
            using(var tgtStream = File.Open(target, FileMode.CreateNew))
            {
                srcStream.Position = 0;
                await srcStream.CopyToAsync(tgtStream);
            }
        }

        protected override Task DoDeleteAsync(string folder, string name)
        {
            var target = Path.Combine(dir, folder, name);
            if (!File.Exists(target))
                throw new FileNotFoundException($"File {name} doesn't exist");

            File.Delete(target);

            return Task.CompletedTask;
        }
    }
}