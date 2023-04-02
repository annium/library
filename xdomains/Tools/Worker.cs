using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace xdomains.Tools;

internal class Worker
{
    private readonly Resolver _resolver;

    private readonly Parser _parser;

    private readonly Settings _settings;

    public Worker(
        Resolver resolver,
        Parser parser,
        Settings settings
    )
    {
        _resolver = resolver;
        _parser = parser;
        _settings = settings;
    }

    public async Task RunAsync(
        string[] query,
        string[] zones,
        int degreeOfParallelism,
        CancellationToken ct
    )
    {
        var domains = GetDomains(query, zones).ToArray();

        Directory.CreateDirectory("results");
        foreach (var domain in query)
            File.WriteAllText(Path.Combine("results", $"{domain}.txt"), string.Empty);

        var locker = new object();
        var i = -1;
        Trace();

        await CheckDomains(domains, HandleResolved, degreeOfParallelism, ct);

        void HandleResolved(string domain, string result)
        {
            var isFree = _parser.IsFree(result);

            lock(locker)
            {
                Trace();
                if (isFree)
                    File.AppendAllLines(Path.Combine("results", $"{domain.Split('.')[0]}.txt"), new [] { domain });
            }
        }

        void Trace()
        {
            Console.CursorLeft = 0;
            Console.Write($"{++i}/{domains.Length}");
        }
    }

    private IEnumerable<string> GetDomains(string[] query, string[] zones)
    {
        if (zones.Length == 0)
            zones = File.ReadAllLines(_settings.RootedPath("zones.txt"));

        foreach (var name in query)
        foreach (var zone in zones)
            yield return $"{name}{zone}";
    }

    private Task CheckDomains(string[] domains, Action<string, string> done, int degreeOfParallelism, CancellationToken ct)
    {
        var semaphore = new Semaphore(degreeOfParallelism, degreeOfParallelism);

        return Task.WhenAll(domains.Select(async domain =>
        {
            if (ct.IsCancellationRequested)
                return string.Empty;

            semaphore.WaitOne();
            var result = await _resolver.ResolveAsync(domain);
            semaphore.Release();
            done(domain, result);

            return result;
        }));
    }
}