using System.IO;
using XRest.Core.Components;

namespace XRest.Clients.Csharp.Components.Writers;

internal class FileWriter
{
    private readonly ITemplateWriter _templateWriter;

    public FileWriter(
        ITemplateWriter templateWriter
    )
    {
        _templateWriter = templateWriter;
    }

    public void Write<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var contents = _templateWriter.Write(template, data);
        Write(output, fileName, contents);
    }

    public void Append<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var contents = _templateWriter.Write(template, data);
        Append(output, fileName, contents);
    }

    public void Write(string output, string fileName, string contents)
    {
        File.WriteAllText(Path.Combine(output, $"{fileName}.cs"), contents);
    }

    public void Append(string output, string fileName, string contents)
    {
        File.AppendAllText(Path.Combine(output, $"{fileName}.cs"), contents);
    }
}