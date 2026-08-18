using System;
using System.IO;

namespace Annium.DocLint.Tests;

// A throwaway directory tree of C# sources, for the paths that read from disk rather than from a
// string: LintService.LintAsync and the whole of LintCommand.
internal sealed class TempSources : IDisposable
{
    public string Root { get; }

    private TempSources(string root)
    {
        Root = root;
    }

    public static TempSources Create(params (string Path, string Content)[] files)
    {
        var root = Path.Combine(Path.GetTempPath(), $"doclint-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        foreach (var (path, content) in files)
        {
            var full = Path.Combine(root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(full).NotNull());
            File.WriteAllText(full, content);
        }

        return new TempSources(root);
    }

    public string PathOf(string relative) => Path.Combine(Root, relative);

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
