using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;

namespace XLog.Commands;

internal class StatsCommand : Command<StatsCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "stats";
    public static string Description => "log stats";
    private static readonly string At = " at ";
    public ILogger Logger { get; }

    public StatsCommand(ILogger logger)
    {
        Logger = logger;
    }

    public override void Handle(StatsCommandConfiguration cfg, CancellationToken ct)
    {
        var stats = GetStats(cfg.File);
        Console.WriteLine($"{stats.Count} source(s):");
        foreach (var (source, count) in stats.OrderByDescending(x => x.Value))
            Console.WriteLine($" - {source}: {count}");
    }

    private static IReadOnlyDictionary<string, int> GetStats(string file)
    {
        var stats = new Dictionary<string, int>();

        using var reader = File.OpenText(file);
        while (reader.ReadLine() is { } line)
        {
            var start = line.IndexOf(At, StringComparison.InvariantCulture) + 4;
            if (start < 4)
                continue;

            var source = line[start..line.IndexOf(' ', start)];
            if (!stats.TryAdd(source, 1))
                stats[source]++;
        }

        return stats;
    }
}

public class StatsCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("File to analyze")]
    public string File { get; set; } = string.Empty;
}
