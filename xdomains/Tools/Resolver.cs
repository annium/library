using System.Diagnostics;
using System.Threading.Tasks;

namespace xdomains.Tools;

internal class Resolver
{
    private readonly Cache cache;

    public Resolver(
        Cache cache
    )
    {
        this.cache = cache;
    }

    public Task<string> ResolveAsync(string domain)
    {
        var cached = cache.Get(domain);
        if (!string.IsNullOrWhiteSpace(cached))
            return Task.FromResult(cached);

        var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.FileName = "whois";
        process.StartInfo.Arguments = domain;

        var tcs = new TaskCompletionSource<string>();

        process.Exited += (sender, e) => handleExit();

        process.Start();

        return tcs.Task;

        void handleExit()
        {
            var result = process.StandardOutput.ReadToEnd();
            cache.Set(domain, result);
            tcs.SetResult(result);
            process.Dispose();
        }
    }
}