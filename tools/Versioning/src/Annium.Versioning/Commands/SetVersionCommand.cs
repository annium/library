using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Versioning.Services;

namespace Annium.Versioning.Commands;

internal class SetVersionCommand(IVersionService versionService, ILogger logger)
    : AsyncCommand<SetVersionCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public ILogger Logger { get; } = logger;
    public static string Id => "set";
    public static string Description => "set version tag in git repository";

    public override async Task HandleAsync(SetVersionCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = cfg.WorkingDirectory.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(cfg.WorkingDirectory);
        if (!Directory.Exists(workingDirectory))
        {
            Console.WriteLine($"Working directory {workingDirectory} does not exist");
            return;
        }

        if (string.IsNullOrWhiteSpace(cfg.Version))
        {
            Console.WriteLine("Version is required");
            return;
        }

        // Validate version format X.Y
        var versionPattern = @"^(\d+)\.(\d+)$";
        var match = Regex.Match(cfg.Version, versionPattern);
        if (!match.Success)
        {
            Console.WriteLine("Version must be in format X.Y where X and Y are natural numbers");
            return;
        }

        var major = uint.Parse(match.Groups[1].Value);
        var minor = uint.Parse(match.Groups[2].Value);

        try
        {
            var newVersion = await versionService.SetVersionAsync(workingDirectory, major, minor);
            Console.WriteLine($"Version {newVersion} set");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set version: {ex.Message}");
        }
    }
}

internal class SetVersionCommandConfiguration
{
    [Option("w")]
    [Help("Working directory.")]
    public string WorkingDirectory { get; set; } = string.Empty;

    [Option("v")]
    [Help("Version to set.")]
    public string Version { get; set; } = string.Empty;
}
