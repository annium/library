using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using xdomains.Tools;

namespace xdomains.Commands;

internal class QueryCommand : AsyncCommand<QueryCommandConfiguration>
{
    public override string Id { get; } = "query";

    public override string Description { get; } = "query domains";

    private readonly Worker worker;

    public QueryCommand(
        Worker worker
    )
    {
        this.worker = worker;
    }

    public override Task HandleAsync(QueryCommandConfiguration cfg, CancellationToken ct)
    {
        var query = File.ReadAllLines("query.txt").OfType<string>().ToArray();
        var filter = cfg.Filter ? File.ReadAllLines("filter.txt").OfType<string>().ToArray() : Array.Empty<string>();

        return worker.RunAsync(query, filter, cfg.DegreeOfParallelism, ct);
    }
}

internal class QueryCommandConfiguration
{
    [Option("f", isRequired : false)]
    public bool Filter { get; set; }

    [Option("dp", isRequired : false)]
    public int DegreeOfParallelism { get; set; } = 100;
}