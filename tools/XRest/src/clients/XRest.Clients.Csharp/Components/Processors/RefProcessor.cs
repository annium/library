using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;
using NodaTime;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class RefProcessor
{
    public static string Process(IRef @ref, ProcessingContext ctx) => @ref switch
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
        _                     => throw new ArgumentOutOfRangeException(nameof(@ref), @ref, $"Unsupported ref {@ref}")
    };

    private static string Process(BaseTypeRef @ref, ProcessingContext ctx)
    {
        switch (@ref.Name)
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
            case BaseType.Decimal:
                return @ref.Name;
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
                throw new ArgumentOutOfRangeException(nameof(@ref), @ref, $"Unsupported base type {@ref}");
        }

        string Type<T>()
        {
            ctx.TrackNamespace(typeof(T));
            return nameof(T);
        }
    }

    private static string Process(NullableRef @ref, ProcessingContext ctx)
    {
        return $"{Process(@ref.Value, ctx)}?";
    }

    private static string Process(GenericParameterRef @ref)
    {
        return @ref.Name;
    }

    private static string Process(EnumRef @ref, ProcessingContext ctx)
    {
        ctx.TrackNamespace(@ref.Namespace.ToNamespace().From(ctx.ModelsNamespace));
        return @ref.Name;
    }

    private static string Process(ArrayRef @ref, ProcessingContext ctx)
    {
        return $"{Process(@ref.Value, ctx)}[]";
    }

    private static string Process(RecordRef @ref, ProcessingContext ctx)
    {
        ctx.TrackNamespace(typeof(Dictionary<,>));
        return $"Dictionary<{Process(@ref.Key, ctx)}, {Process(@ref.Value, ctx)}>";
    }

    private static string Process(StructRef @ref, ProcessingContext ctx)
    {
        return Process(@ref, @ref.Args, ctx);
    }

    private static string Process(InterfaceRef @ref, ProcessingContext ctx)
    {
        return Process(@ref, @ref.Args, ctx);
    }

    private static string Process(IModelRef @ref, IRef[] refArgs, ProcessingContext ctx)
    {
        ctx.TrackNamespace(@ref.Namespace.ToNamespace().From(ctx.ModelsNamespace));
        if (refArgs.Length == 0)
            return @ref.Name;

        var sb = new StringBuilder(@ref.Name);
        sb.Append('<');
        var args = refArgs.Select(x => Process(x, ctx)).ToArray();
        sb.AppendJoin(", ", args);
        sb.Append('>');

        return sb.ToString();
    }

    private static string Process(PromiseRef @ref, ProcessingContext ctx)
    {
        if (@ref.Value is null)
        {
            ctx.TrackNamespace(typeof(Task));
            return "Task";
        }

        ctx.TrackNamespace(typeof(Task<>));
        return $"Task<{Process(@ref.Value, ctx)}>";
    }
}