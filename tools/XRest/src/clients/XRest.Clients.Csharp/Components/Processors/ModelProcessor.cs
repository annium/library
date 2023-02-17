using System;
using System.Linq;
using Annium.Core.Primitives.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using XRest.Clients.Csharp.Extensions;
using XRest.Clients.Csharp.Views.Models;
using XRest.Clients.Csharp.Views.Models.Fields;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ModelProcessor
{
    public static IModelView Process(IModel model, ProcessingContext ctx) => model switch
    {
        StructModel x    => Process(x, ctx),
        InterfaceModel x => Process(x, ctx),
        EnumModel x      => Process(x, ctx),
        _                => throw new ArgumentOutOfRangeException(nameof(model), model, $"Unsupported model {model}")
    };

    private static StructView Process(StructModel model, ProcessingContext ctx)
    {
        var @namespace = model.Namespace.Prepend(ctx.ModelsNamespace);
        var isAbstract = model.IsAbstract;
        var name = model.Name;
        var argsCount = model.Args.Count;
        var args = model.Args
            .Select(x => RefProcessor.Process(x, ctx))
            .Join(", ");
        var extends = model
            .Base.Yield()
            .OfType<IRef>()
            .Concat(model.Interfaces)
            .Select(x => RefProcessor.Process(x, ctx))
            .Join(", ");
        var fields = model.Fields.Select(x =>
        {
            var type = RefProcessor.Process(x.Type, ctx);
            var defaultValue = DefaultValueProcessor.Resolve(x.Type, ctx);

            return new StructFieldView(type, x.Name, !string.IsNullOrWhiteSpace(defaultValue), defaultValue);
        }).ToArray();
        var usages = ctx.Usages.ToUsagesFrom(@namespace).ToUsageStrings();

        return new StructView(usages, @namespace.ToString(), isAbstract, name, argsCount, args, !string.IsNullOrWhiteSpace(extends), extends, fields);
    }

    private static InterfaceView Process(InterfaceModel model, ProcessingContext ctx)
    {
        var @namespace = model.Namespace.Prepend(ctx.ModelsNamespace);
        var name = model.Name;
        var argsCount = model.Args.Count;
        var args = model.Args
            .Select(x => RefProcessor.Process(x, ctx))
            .Join(", ");
        var extends = model.Interfaces
            .Select(x => RefProcessor.Process(x, ctx))
            .Join(", ");
        var fields = model.Fields.Select(x =>
        {
            var type = RefProcessor.Process(x.Type, ctx);

            return new InterfaceFieldView(type, x.Name);
        }).ToArray();
        var usages = ctx.Usages.ToUsagesFrom(@namespace).ToUsageStrings();

        return new InterfaceView(usages, @namespace.ToString(), name, argsCount, args, !string.IsNullOrWhiteSpace(extends), extends, fields);
    }

    private static EnumView Process(EnumModel model, ProcessingContext ctx)
    {
        var @namespace = model.Namespace.Prepend(ctx.ModelsNamespace);
        var name = model.Name;

        return new EnumView(Array.Empty<string>(), @namespace, name, model.Values.ToDictionary());
    }
}