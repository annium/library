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

    public void WriteIfMissing<T>(string output, string fileName, string template, T data)
        where T : class
    {
        var path = Path.Combine(output, $"{fileName}.cs");
        if (!File.Exists(path))
            Write(output, fileName, template, data);
    }

    public void Write<T>(string output, string fileName, string template, T data)
        where T : class
    {
        File.WriteAllText(Path.Combine(output, $"{fileName}.cs"), _templateWriter.Write(template, data));
    }
}