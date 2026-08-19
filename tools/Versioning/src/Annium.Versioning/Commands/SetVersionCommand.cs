using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Versioning.Models;
using Annium.Versioning.Services;

namespace Annium.Versioning.Commands;

internal class SetVersionCommand(IVersionService versionService, ILogger logger)
    : Command<SetVersionCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public ILogger Logger { get; } = logger;
    public static string Id => "set-version";
    public static string Description => "set version tag in git repository";

    public override void Handle(SetVersionCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = WorkingDirectory.Resolve(cfg.WorkingDirectory);

        // Validate and parse version chain format X.Y
        if (!VersionChain.TryParse(cfg.Version, out var versionChain))
            throw new ArgumentException("Version must be in format X.Y where X and Y are natural numbers");

        var result = versionService.SetVersion(workingDirectory, versionChain);
        if (result.IsT0)
            Console.WriteLine(result.AsT0.ToString());
        else
            throw new Exception($"Failed to set version: {result.AsT1}");
    }
}

internal class SetVersionCommandConfiguration
{
    [Option("w")]
    [Help("Working directory.")]
    public string WorkingDirectory { get; set; } = string.Empty;

    [Option("v", true)]
    [Help("Version to set.")]
    public string Version { get; set; } = string.Empty;
}
