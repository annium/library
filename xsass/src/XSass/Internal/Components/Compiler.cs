using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using LibSassHost;

namespace XSass.Internal.Components;

internal class Compiler : ILogSubject<Compiler>
{
    public ILogger<Compiler> Logger { get; }
    private const string CSS = ".css";
    private readonly CompilationOptions _opts;

    public Compiler(
        Configuration cfg,
        ILogger<Compiler> logger
    )
    {
        Logger = logger;
        _opts = new CompilationOptions
        {
            IncludePaths = new List<string> { cfg.Root }
                .Concat(cfg.LoadPaths.Select(x => Path.Combine(cfg.Root, x)))
                .ToList(),
            OutputStyle = OutputStyle.Compressed,
        };
    }

    public async Task CompileFilesAsync(IEnumerable<string> files)
    {
        await Task.WhenAll(files.Select(CompileFileAsync));
    }

    private async Task CompileFileAsync(string file)
    {
        var fileInfo = new FileInfo(file);
        if (fileInfo.Name.StartsWith("_"))
        {
            this.Log().Trace($"Compile: skip private {file}");
            return;
        }

        this.Log().Debug($"Compile: process {file}");
        var result = SassCompiler.CompileFile(file, options: _opts);
        var newFile = fileInfo.FullName.Replace(fileInfo.Extension, CSS);

        if (File.Exists(newFile) && result.CompiledContent == await File.ReadAllTextAsync(newFile))
        {
            this.Log().Trace($"Compile: skip unchanged {file}");
            return;
        }

        await File.WriteAllTextAsync(newFile, result.CompiledContent);
    }
}