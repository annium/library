using System.IO;
using Annium.XRest.Clients.Shared.Components;

namespace Annium.XRest.Clients.Csharp.Components.Writers;

internal class FileWriter
{
    private readonly ITemplateWriter _templateWriter;

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
