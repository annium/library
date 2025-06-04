using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.DocLint.Internal.Services;
using Annium.Extensions.Arguments;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Annium.DocLint.Commands;

internal class LintCommand(LintService lintService) : AsyncCommand<LintCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "lint";
    public static string Description => "lint documentation of files in specified directory";

    public override async Task HandleAsync(LintCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = cfg.WorkingDirectory.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(cfg.WorkingDirectory);
        if (!Directory.Exists(workingDirectory))
        {
            Console.WriteLine($"Working directory {workingDirectory} does not exist");
            return;
        }

        var paths = ResolvePaths(workingDirectory, cfg.Include, cfg.Exclude);
        if (paths.Count == 0)
        {
            Console.WriteLine("No files matched");
            return;
        }

        Console.WriteLine($"run linting on {paths.Count} files");
        // var results = await Task.WhenAll(paths.Select(lintService.LintAsync));
        var results = new List<IReadOnlyList<string>>(paths.Count);
        foreach (var path in paths)
            results.Add(await lintService.LintAsync(path));

        var validFiles = results.Count(x => x.Count == 0);
        if (validFiles > 0)
            Console.WriteLine($"linted {validFiles} files successfully");

        var invalidFiles = results.Count(x => x.Count > 0);
        if (invalidFiles == 0)
            return;

        var totalErrors = results.Sum(x => x.Count);
        Console.WriteLine($"linted {invalidFiles} files with {totalErrors} error(s):");

        for (var i = 0; i < paths.Count; i++)
        {
            var errors = results[i];
            if (errors.Count == 0)
                continue;

            Console.WriteLine($"file {paths[i]} has {errors.Count} error(s):");
            foreach (var error in errors)
                Console.WriteLine($"- {error}");
        }

        throw new Exception("linting failed");
    }

    private IReadOnlyList<string> ResolvePaths(string workingDirectory, string[] includes, string[] excludes)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns(includes);
        matcher.AddExcludePatterns(excludes);

        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(workingDirectory)));

        return result.Files.OrderBy(x => x.Path).Select(x => Path.Combine(workingDirectory, x.Path)).ToArray();
    }
}

internal class LintCommandConfiguration
{
    [Option("w")]
    [Help("Working directory.")]
    public string WorkingDirectory { get; set; } = string.Empty;

    [Option("i")]
    [Help("Paths to include in linting.")]
    public string[] Include { get; set; } = [];

    [Option("e")]
    [Help("Paths to exclude from linting.")]
    public string[] Exclude { get; set; } = [];
}
