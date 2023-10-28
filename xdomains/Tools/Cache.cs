using System.IO;

namespace xdomains.Tools;

internal class Cache
{
    private const string CacheDir = "cache";
    private readonly Settings _settings;

    public Cache(Settings settings)
    {
        _settings = settings;
        Directory.CreateDirectory(settings.RootedPath(CacheDir));
    }

    public string Get(string domain)
    {
        var path = CachePath(domain);

        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    public void Set(string domain, string resolution)
    {
        File.WriteAllText(CachePath(domain), resolution);
    }

    public void Cleanup()
    {
        foreach (var file in Directory.GetFiles(_settings.RootedPath(CacheDir)))
            if (File.ReadAllText(file).Contains("exceed"))
                File.Delete(file);
    }

    private string CachePath(string domain) => _settings.RootedPath(CacheDir, domain);
}
