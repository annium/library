using System;
using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Versioning.Models;
using Annium.Versioning.Services;

namespace Annium.Versioning.Commands;

internal class GetVersionCommand(IVersionService versionService, ILogger logger)
    : Command<GetVersionCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public ILogger Logger { get; } = logger;
    public static string Id => "get-version";
    public static string Description => "get current version from git repository";

    public override void Handle(GetVersionCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = cfg.WorkingDirectory.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(cfg.WorkingDirectory);
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException(workingDirectory);

        VersionChain? versionChain = null;
        if (!string.IsNullOrWhiteSpace(cfg.VersionChain))
        {
            if (!VersionChain.TryParse(cfg.VersionChain, out var chain))
                throw new ArgumentException($"Invalid version chain format: {cfg.VersionChain}. Expected format: X.Y");
            versionChain = chain;
        }

        var result = versionService.GetCurrentVersion(workingDirectory, versionChain);
        if (result.IsT0)
            Console.WriteLine(result.AsT0.ToString());
        else
            throw new Exception($"Failed to get version: {result.AsT1}");
    }
}

internal class GetVersionCommandConfiguration
{
    [Option("w")]
    [Help("Working directory.")]
    public string WorkingDirectory { get; set; } = string.Empty;

    [Option("v")]
    [Help(
        "Version chain in Major.Minor format (e.g., 1.2). If not specified, returns the latest version across all chains."
    )]
    public string VersionChain { get; set; } = string.Empty;
}
