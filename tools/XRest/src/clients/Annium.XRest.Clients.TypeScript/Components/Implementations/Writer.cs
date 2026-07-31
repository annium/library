using System;
using System.IO;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.XRest.Clients.Shared.Components;
using Annium.XRest.Clients.TypeScript.Views;

namespace Annium.XRest.Clients.TypeScript.Components.Implementations;

internal class Writer : IWriter
{
    private readonly ITemplateWriter _templateWriter;

    public Writer(ITemplateWriter templateWriter)
    {
        _templateWriter = templateWriter;
    }

    public void Write(string output, ApiView api)
    {
        EnsureSafeToWipe(output);

        if (Directory.Exists(output))
            Directory.Delete(output, true);
        Directory.CreateDirectory(output);

        if (api.SharedExports.Count > 0)
            Write(output, "shared.ts", "Templates.SharedExports", new { Exports = api.SharedExports });

        foreach (var controllerView in api.Controllers.Where(x => x.Actions.Count > 0))
        {
            var directory = controllerView.Namespace.ToNamespace().ToPath(output);
            Directory.CreateDirectory(directory);

            Write(directory, $"{controllerView.Name.CamelCase()}Api.ts", "Templates.Api", controllerView);
        }
    }

    private void Write<T>(string output, string file, string template, T data)
        where T : class
    {
        File.WriteAllText(Path.Combine(output, file), _templateWriter.Write(template, data));
    }

    /// <summary>
    /// The output directory is deleted recursively before generation, so a mistyped <c>-o</c> is
    /// destructive. Refuse the obviously wrong targets rather than wiping a working tree.
    /// </summary>
    private static void EnsureSafeToWipe(string output)
    {
        var path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(output));

        if (Path.GetPathRoot(path) == path)
            throw new InvalidOperationException($"Refusing to generate into a filesystem root: {path}");

        var protectedPaths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Directory.GetCurrentDirectory(),
        };

        foreach (var protectedPath in protectedPaths)
            if (
                protectedPath.Length > 0
                && path.Equals(
                    Path.TrimEndingDirectorySeparator(Path.GetFullPath(protectedPath)),
                    StringComparison.Ordinal
                )
            )
                throw new InvalidOperationException($"Refusing to generate into {path} — it would be deleted");

        if (Directory.Exists(Path.Combine(path, ".git")))
            throw new InvalidOperationException($"Refusing to generate into a repository root: {path}");
    }
}
