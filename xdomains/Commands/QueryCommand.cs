using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using xdomains.Tools;

namespace xdomains.Commands;

internal class QueryCommand : AsyncCommand<QueryCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "query";

    public static string Description => "query domains";

    private readonly Worker _worker;

    public QueryCommand(Worker worker)
    {
        _worker = worker;
    }

    public override Task HandleAsync(QueryCommandConfiguration cfg, CancellationToken ct)
    {
        const string queryFile = "query.txt";
        const string filterFile = "filter.txt";

        if (!File.Exists(queryFile))
            Console.WriteLine($"{queryFile} not found");

        if (cfg.Filter && !File.Exists(filterFile))
            Console.WriteLine($"{filterFile} not found");

        var query = File.ReadAllLines(queryFile);
        var filter = cfg.Filter && File.Exists(filterFile) ? File.ReadAllLines(filterFile) : [];

        return _worker.RunAsync(query, filter, cfg.DegreeOfParallelism, ct);
    }
}

internal class QueryCommandConfiguration
{
    [Option("f", isRequired: false)]
    public bool Filter { get; set; }

    [Option("dp", isRequired: false)]
    public int DegreeOfParallelism { get; set; } = 100;
}
