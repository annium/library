using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using GlobExpressions;

namespace Xa.Commands;

internal class GlobCommand : AsyncCommand<GlobCommandConfiguration>
{
    public override string Id => "glob";
    public override string Description => "glob file";

    public override async Task HandleAsync(GlobCommandConfiguration cfg, CancellationToken ct)
    {
        var glob = new Glob(cfg.Pattern, GlobOptions.Compiled | GlobOptions.CaseInsensitive);

        while (true)
        {
            var line = await Console.In.ReadLineAsync();
            if (line is null)
                break;
            if (glob.IsMatch(line))
                Console.WriteLine(line);
        }
    }
}

internal class GlobCommandConfiguration
{
    [Position(1)]
    [Help("pattern")]
    public string Pattern { get; set; } = string.Empty;
}