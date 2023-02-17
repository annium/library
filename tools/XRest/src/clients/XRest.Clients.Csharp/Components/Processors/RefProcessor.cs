using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;
using NodaTime;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class RefProcessor
{
    public static string Process(IRef reference, ProcessingContext ctx) => reference switch
    {
        BaseTypeRef x         => Process(x, ctx),
        NullableRef x         => Process(x, ctx),
        GenericParameterRef x => Process(x),
        EnumRef x             => Process(x, ctx),
        ArrayRef x            => Process(x, ctx),
        RecordRef x           => Process(x, ctx),
        StructRef x           => Process(x, ctx),
        InterfaceRef x        => Process(x, ctx),
        PromiseRef x          => Process(x, ctx),
        _                     => throw new ArgumentOutOfRangeException(nameof(reference), reference, $"Unsupported ref {reference}")
    };

    private static string Process(BaseTypeRef reference, ProcessingContext ctx)
    {
        switch (reference.Name)
        {
            case BaseType.Object:
            case BaseType.Bool:
            case BaseType.String:
            case BaseType.Byte:
            case BaseType.SByte:
            case BaseType.Int:
            case BaseType.UInt:
            case BaseType.Long:
            case BaseType.ULong:
            case BaseType.Float:
            case BaseType.Double:
            case BaseType.Decimal:
                return reference.Name;
            case BaseType.Guid:
                return Type<Guid>();
            case BaseType.DateTime:
                return Type<LocalTime>();
            case BaseType.DateTimeOffset:
                return Type<Instant>();
            case BaseType.Date:
                return Type<DateOnly>();
            case BaseType.Time:
                return Type<TimeOnly>();
            case BaseType.TimeSpan:
                return Type<Duration>();
            case BaseType.YearMonth:
                return Type<YearMonth>();
            case BaseType.Void:
                return typeof(void).Name;
            default:
                throw new ArgumentOutOfRangeException(nameof(reference), reference, $"Unsupported base type {reference}");
        }

        string Type<T>()
        {
            ctx.UseNamespace(typeof(T));
            return typeof(T).PureName();
        }
    }

    private static string Process(NullableRef reference, ProcessingContext ctx)
    {
        return $"{Process(reference.Value, ctx)}?";
    }

    private static string Process(GenericParameterRef reference)
    {
        return reference.Name;
    }

    private static string Process(EnumRef reference, ProcessingContext ctx)
    {
        ctx.UseNamespace(reference.Namespace.ToNamespace().Prepend(ctx.ModelsNamespace));
        return reference.Name;
    }

    private static string Process(ArrayRef reference, ProcessingContext ctx)
    {
        return $"{Process(reference.Value, ctx)}[]";
    }

    private static string Process(RecordRef reference, ProcessingContext ctx)
    {
        ctx.UseNamespace(typeof(Dictionary<,>));
        return $"Dictionary<{Process(reference.Key, ctx)}, {Process(reference.Value, ctx)}>";
    }

    private static string Process(StructRef reference, ProcessingContext ctx)
    {
        return Process(reference, reference.Args, ctx);
    }

    private static string Process(InterfaceRef reference, ProcessingContext ctx)
    {
        return Process(reference, reference.Args, ctx);
    }

    private static string Process(IModelRef reference, IRef[] refArgs, ProcessingContext ctx)
    {
        ctx.UseNamespace(reference.Namespace.ToNamespace().Prepend(ctx.ModelsNamespace));
        if (refArgs.Length == 0)
            return reference.Name;

        var sb = new StringBuilder(reference.Name);
        sb.Append('<');
        var args = refArgs.Select(x => Process(x, ctx)).ToArray();
        sb.AppendJoin(", ", args);
        sb.Append('>');

        return sb.ToString();
    }

    private static string Process(PromiseRef reference, ProcessingContext ctx)
    {
        if (reference.Value is null)
        {
            ctx.UseNamespace(typeof(Task));
            return "Task";
        }

        ctx.UseNamespace(typeof(Task<>));
        return $"Task<{Process(reference.Value, ctx)}>";
    }
}