using System;
using System.Collections.Generic;
using System.IO;
using Annium.XRest.Clients.Shared.Components;

namespace Annium.XRest.Clients.Csharp.Components.Writers;

internal class FileWriter
{
    private readonly ITemplateWriter _templateWriter;

    // a case-insensitive filesystem — macOS and Windows both, by default — resolves `UsersClient.cs`
    // and `usersClient.cs` to one file, so the second write replaced the first and left a client the
    // container still referenced but nothing declared
    private readonly Dictionary<string, string> _written = new(StringComparer.OrdinalIgnoreCase);

    public FileWriter(ITemplateWriter templateWriter)
    {
        _templateWriter = templateWriter;
    }

    public void Write<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var contents = _templateWriter.Write(template, data);
        Write(output, fileName, contents);
    }

    public void TryWrite<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var contents = _templateWriter.Write(template, data);
        TryWrite(output, fileName, contents);
    }

    public void Append<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var contents = _templateWriter.Write(template, data);
        Append(output, fileName, contents);
    }

    public void Write(string output, string fileName, string contents)
    {
        var path = OutputFile(output, fileName);
        if (_written.TryGetValue(path, out var previous))
            throw new InvalidOperationException(
                previous == path
                    ? $"Can't write '{path}' twice: the second write would silently replace the first"
                    : $"Can't write '{path}': '{previous}' differs from it only by case, and one would silently replace the other"
            );

        _written[path] = path;
        File.WriteAllText(path, contents);
    }

    public void TryWrite(string output, string fileName, string contents)
    {
        var path = OutputFile(output, fileName);
        if (!File.Exists(path))
            File.WriteAllText(path, contents);
    }

    public void Append(string output, string fileName, string contents)
    {
        var path = OutputFile(output, fileName);
        File.AppendAllText(path, contents);
    }

    private string OutputFile(string output, string fileName) => Path.Combine(output, $"{fileName}.cs");
}
