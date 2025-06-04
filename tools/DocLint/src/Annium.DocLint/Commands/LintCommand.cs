using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.DocLint.Internal.Services;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Annium.DocLint.Commands;

internal class LintCommand(LintService lintService, ILogger logger)
    : AsyncCommand<LintCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "lint";
    public static string Description => "lint documentation of files in specified directory";
    public ILogger Logger { get; } = logger;

    public override async Task HandleAsync(LintCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = cfg.WorkingDirectory.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : cfg.WorkingDirectory;
        if (!Directory.Exists(workingDirectory))
        {
            this.Error<string>("Working directory {directory} does not exist", workingDirectory);
            return;
        }

        var paths = ResolvePaths(workingDirectory, cfg.Include, cfg.Exclude);
        if (paths.Count == 0)
        {
            this.Error("No files matched");
            return;
        }

        this.Info("run linting on {count} files", paths.Count);
        var results = await Task.WhenAll(paths.Select(lintService.LintAsync));

        var totalErrors = results.Sum(x => x.Count);
        if (totalErrors == 0)
        {
            this.Info("linted {count} files successfully", paths.Count);
            return;
        }

        this.Error("linting of {count} files failed with {errors} error(s)", paths.Count, totalErrors);
        for (var i = 0; i < paths.Count; i++)
        {
            var errors = results[i];
            if (errors.Count == 0)
                continue;
            this.Error("file {file} has {errors} error(s):", errors.Count);
            foreach (var error in errors)
                this.Error<string>("- {error}", error);
        }
    }

    private IReadOnlyList<string> ResolvePaths(string workingDirectory, string[] includes, string[] excludes)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns(includes);
        matcher.AddExcludePatterns(excludes);

        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(workingDirectory)));

        return result.Files.Select(x => x.Path).ToArray();
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
