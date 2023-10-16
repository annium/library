using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xf.Commands;

internal class ListCommand : Command<ListCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "ls";
    public static string Description => "list entries";

    public override void Handle(ListCommandConfiguration cfg, CancellationToken ct)
    {
        foreach (var entry in ResolveEntries(cfg))
            Console.WriteLine(entry);
    }

    private static IEnumerable<string> ResolveEntries(ListCommandConfiguration cfg)
    {
        var searchOptions = cfg.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        if (cfg.Dirs)
            return Directory.EnumerateDirectories(cfg.Root, cfg.Pattern, searchOptions);

        if (cfg.Files)
            return Directory.EnumerateFiles(cfg.Root, cfg.Pattern, searchOptions);

        return Directory.EnumerateFileSystemEntries(cfg.Root, cfg.Pattern, searchOptions);
    }
}

internal class ListCommandConfiguration
{
    [Position(1)]
    [Help("Directory to list entries in.")]
    public string Root { get; set; } = string.Empty;

    [Position(2, isRequired: false)]
    [Help("Search pattern.")]
    public string Pattern { get; set; } = "*";

    [Option("d")]
    [Help("List directories only")]
    public bool Dirs { get; set; } = default!;

    [Option("f")]
    [Help("List files only")]
    public bool Files { get; set; } = default!;

    [Option("r")]
    [Help("Recurse")]
    public bool Recurse { get; set; } = default!;
}