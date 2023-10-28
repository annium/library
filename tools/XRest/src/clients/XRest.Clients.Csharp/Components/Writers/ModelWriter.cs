using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Extensions;
using XRest.Clients.Csharp.Views.Models;
using static XRest.Clients.Csharp.Components.Writers.WriterHelper;

namespace XRest.Clients.Csharp.Components.Writers;

internal class ModelWriter
{
    private readonly FileWriter _writer;

    public ModelWriter(FileWriter writer)
    {
        _writer = writer;
    }

    public void Write(string rootDir, Namespace rootNs, IReadOnlyCollection<IModelView> models)
    {
        foreach (var group in models.GroupBy(x => (x.Namespace, x.Name)))
            Write(
                rootDir,
                rootNs,
                group.Key.Namespace,
                group.Key.Name,
                group.OrderByDescending(GetModelOrder).ToArray()
            );
    }

    private void Write(
        string rootDir,
        Namespace rootNs,
        Namespace groupNs,
        string name,
        IReadOnlyCollection<IModelView> models
    )
    {
        var output = GetOutputPath(rootDir, rootNs, groupNs);
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        var usages = models.SelectMany(x => x.Usages).Select(x => x.ToNamespace()).CleanUsages().ToUsageStrings();
        _writer.Write(
            output,
            name,
            "Templates.TypeHeader",
            new { Namespace = groupNs.ToNamespaceString(), Usages = usages }
        );

        foreach (var model in models)
        {
            _writer.Append(output, name, Environment.NewLine.Repeat(2));
            Write(output, name, model);
        }
    }

    private void Write(string output, string name, IModelView model)
    {
        switch (model)
        {
            case StructView:
                _writer.Append(output, name, "Templates.Struct", model);
                break;
            case InterfaceView:
                _writer.Append(output, name, "Templates.Interface", model);
                break;
            case EnumView:
                _writer.Append(output, name, "Templates.Enum", model);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(model), model, $"Unsupported model {model}");
        }
    }

    private static int GetModelOrder(IModelView model) =>
        model switch
        {
            StructView x => x.ArgsCount * 100,
            InterfaceView x => x.ArgsCount,
            EnumView => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, $"Unsupported model {model}")
        };
}
