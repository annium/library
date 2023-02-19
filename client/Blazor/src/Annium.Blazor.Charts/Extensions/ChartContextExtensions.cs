using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

internal static class ChartContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IChartContext ctx, Instant moment) =>
        ctx.MsPerPx == 0 ? 0 : ((moment - ctx.View.Start).TotalMilliseconds.FloorInt64() / (decimal) ctx.MsPerPx).CeilInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromX(this IChartContext ctx, int x) =>
        (ctx.View.Start + x * Duration.FromMilliseconds(ctx.MsPerPx)).RoundTo(ctx.Resolution);
}