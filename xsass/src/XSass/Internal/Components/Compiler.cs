using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using LibSassHost;

namespace XSass.Internal.Components;

internal class Compiler : ILogSubject
{
    public ILogger Logger { get; }
    private const string Css = ".css";
    private readonly CompilationOptions _opts;

    public Compiler(
        Configuration cfg,
        ILogger logger
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
            this.Trace($"Compile: skip private {file}");
            return;
        }

        this.Debug($"Compile: process {file}");
        var result = SassCompiler.CompileFile(file, options: _opts);
        var newFile = fileInfo.FullName.Replace(fileInfo.Extension, Css);

        if (File.Exists(newFile) && result.CompiledContent == await File.ReadAllTextAsync(newFile))
        {
            this.Trace($"Compile: skip unchanged {file}");
            return;
        }

        await File.WriteAllTextAsync(newFile, result.CompiledContent);
    }
}