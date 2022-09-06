using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

public static class PaneContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IPaneContext ctx, Instant moment) =>
        ctx.Chart.ToX(moment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToY(this IPaneContext ctx, decimal value) =>
        ((ctx.View.End - value) / ctx.DotPerPx).RoundInt32();
}