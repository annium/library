using System;
using Annium.Net.Types.Refs;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class DefaultValueProcessor
{
    private const string New = "new()";
    private const string Default = "default!";

    public static string Resolve(IRef reference, ProcessingContext ctx) => reference switch
    {
        BaseTypeRef x       => Resolve(x),
        NullableRef         => string.Empty,
        GenericParameterRef => Default,
        EnumRef             => string.Empty,
        ArrayRef x          => Resolve(x, ctx),
        RecordRef x         => New,
        StructRef           => Default,
        InterfaceRef        => Default,
        _                   => throw new ArgumentOutOfRangeException(nameof(reference), reference, $"Unsupported ref {reference}")
    };

    private static string Resolve(BaseTypeRef reference)
    {
        switch (reference.Name)
        {
            case BaseType.Object:
                return New;
            case BaseType.String:
                return "string.Empty";
            case BaseType.Bool:
            case BaseType.Byte:
            case BaseType.SByte:
            case BaseType.Int:
            case BaseType.UInt:
            case BaseType.Long:
            case BaseType.ULong:
            case BaseType.Float:
            case BaseType.Double:
            case BaseType.Decimal:
            case BaseType.Guid:
            case BaseType.DateTime:
            case BaseType.DateTimeOffset:
            case BaseType.Date:
            case BaseType.Time:
            case BaseType.TimeSpan:
            case BaseType.YearMonth:
                return string.Empty;
            default:
                throw new ArgumentOutOfRangeException(nameof(reference), reference, $"Unsupported type {reference} for default value");
        }
    }

    private static string Resolve(ArrayRef reference, ProcessingContext ctx)
    {
        ctx.UseNamespace(typeof(Array));
        return $"Array.Empty<{RefProcessor.Process(reference.Value, ctx)}>()";
    }
}