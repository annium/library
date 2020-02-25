using System.IO;

namespace xdomains.Tools
{
    internal class Cache
    {
        private const string cacheDir = "cache";

        private readonly Settings settings;

        public Cache(
            Settings settings
        )
        {
            this.settings = settings;
            Directory.CreateDirectory(settings.RootedPath(cacheDir));
        }

        public string Get(string domain)
        {
            var path = cache(domain);

            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public void Set(string domain, string resolution)
        {
            File.WriteAllText(cache(domain), resolution);
        }

        public void Cleanup()
        {
            foreach (var file in Directory.GetFiles(settings.RootedPath(cacheDir)))
                if (File.ReadAllText(file).Contains("exceed"))
                    File.Delete(file);
        }

        private string cache(string domain) => settings.RootedPath(cacheDir, domain);
    }
}