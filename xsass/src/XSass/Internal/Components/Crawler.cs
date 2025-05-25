using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;

namespace XSass.Internal.Components;

internal class Crawler : ILogSubject
{
    public ILogger Logger { get; }
    private readonly Configuration _cfg;
    private readonly Compiler _compiler;

    public Crawler(Configuration cfg, Compiler compiler, ILogger logger)
    {
        _cfg = cfg;
        _compiler = compiler;
        Logger = logger;
    }

    public async Task RunAsync(string directory)
    {
        this.Debug<string>("Run in {directory}", directory);
        if (_cfg.Include.Length == 0)
            await ProcessAsync(directory);
        else
            await Task.WhenAll(_cfg.Include.Select(x => ProcessAsync(Path.Combine(directory, x))));
    }

    private async Task ProcessAsync(string directory)
    {
        this.Debug<string>("Process {directory}", directory);
        var files = Directory
            .EnumerateFiles(directory)
            .Where(file => _cfg.Extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

        await _compiler.CompileFilesAsync(files);

        foreach (var subDirectory in Directory.EnumerateDirectories(directory))
        {
            var subDirectoryName = Path.GetFileName(subDirectory);
            if (_cfg.Exclude.Any(dir => dir.Equals(subDirectoryName, StringComparison.OrdinalIgnoreCase)))
                continue;

            await ProcessAsync(subDirectory);
        }
    }
}
