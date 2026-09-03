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
        // a mistyped path or a pattern matching nothing must fail the run: this command is a CI
        // gate, and exiting 0 without having linted anything reports success for work not done
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException($"Working directory {workingDirectory} does not exist");

        var paths = ResolvePaths(workingDirectory, cfg.Include, cfg.Exclude);
        if (paths.Count == 0)
            throw new InvalidOperationException(
                $"No files matched in {workingDirectory} (include: {string.Join(", ", cfg.Include)}; exclude: {string.Join(", ", cfg.Exclude)})"
            );

        Console.WriteLine($"run linting on {paths.Count} files");
        var lintResults = new List<LintResult>(paths.Count);
        foreach (var path in paths)
            lintResults.Add(await lintService.LintAsync(path, ct));

        // a partial type carries its documentation on one declaration only, so a type-level report is
        // dropped once any file in this run turns out to document that type
        var documentedPartialTypes = lintResults.SelectMany(x => x.DocumentedPartialTypes).ToHashSet();
        var results = lintResults
            .Select(x =>
                x.Findings.Where(f => f.PartialType is null || !documentedPartialTypes.Contains(f.PartialType))
                    .Select(f => f.Message)
                    .ToArray()
            )
            .ToArray();

        var validFiles = results.Count(x => x.Length == 0);
        if (validFiles > 0)
            Console.WriteLine($"linted {validFiles} files successfully");

        var invalidFiles = results.Count(x => x.Length > 0);
        if (invalidFiles == 0)
            return;

        var totalErrors = results.Sum(x => x.Length);
        Console.WriteLine($"linted {invalidFiles} files with {totalErrors} error(s):");

        for (var i = 0; i < paths.Count; i++)
        {
            var errors = results[i];
            if (errors.Length == 0)
                continue;

            Console.WriteLine($"file {paths[i]} has {errors.Length} error(s):");
            foreach (var error in errors)
                Console.WriteLine($"- {error}");
        }

        throw new Exception("linting failed");
    }

    private static IReadOnlyList<string> ResolvePaths(string workingDirectory, string[] includes, string[] excludes)
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
