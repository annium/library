using System;
using System.IO;
using System.Linq;
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

    public QueryCommand(
        Worker worker
    )
    {
        _worker = worker;
    }

    public override Task HandleAsync(QueryCommandConfiguration cfg, CancellationToken ct)
    {
        var query = File.ReadAllLines("query.txt").OfType<string>().ToArray();
        var filter = cfg.Filter ? File.ReadAllLines("filter.txt").OfType<string>().ToArray() : Array.Empty<string>();

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