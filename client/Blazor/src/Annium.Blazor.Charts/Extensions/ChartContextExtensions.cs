using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

internal static class ChartContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IChartContext ctx, Instant moment) =>
        ((moment - ctx.View.Start).TotalMilliseconds.FloorInt64() / (decimal) ctx.MsPerPx).FloorInt32();
}