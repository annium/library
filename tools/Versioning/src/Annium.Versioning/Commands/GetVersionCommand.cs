using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Versioning.Services;

namespace Annium.Versioning.Commands;

internal class GetVersionCommand(IVersionService versionService, ILogger logger)
    : AsyncCommand<GetVersionCommandConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public ILogger Logger { get; } = logger;
    public static string Id => "get";
    public static string Description => "get current version from git repository";

    public override async Task HandleAsync(GetVersionCommandConfiguration cfg, CancellationToken ct)
    {
        var workingDirectory = cfg.WorkingDirectory.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(cfg.WorkingDirectory);
        if (!Directory.Exists(workingDirectory))
        {
            Console.WriteLine($"Working directory {workingDirectory} does not exist");
            return;
        }

        try
        {
            var version = await versionService.GetCurrentVersionAsync(workingDirectory);
            if (version != null)
            {
                Console.WriteLine(version.ToString());
            }
            else
            {
                Console.WriteLine("No version tags found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get version: {ex.Message}");
        }
    }
}

internal class GetVersionCommandConfiguration
{
    [Option("w")]
    [Help("Working directory.")]
    public string WorkingDirectory { get; set; } = string.Empty;
}
