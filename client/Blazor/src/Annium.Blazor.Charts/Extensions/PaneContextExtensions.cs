using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

public static class PaneContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IPaneContext ctx, Instant moment) =>
        ctx.Chart.ToX(moment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromX(this IPaneContext ctx, int x) =>
        ctx.Chart.FromX(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToY(this IPaneContext ctx, decimal value) =>
        ctx.DotPerPx == 0 ? 0 : ((ctx.View.End - value) / ctx.DotPerPx).RoundInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal FromY(this IPaneContext ctx, int y) =>
        ctx.View.End - y * ctx.DotPerPx;
}