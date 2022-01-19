using System;
using Annium.Blazor.Charts.Domain.Contexts;

namespace Annium.Blazor.Charts.Internal.Extensions;

internal static class ChartContextExtensions
{
    public static Action OnUpdate(this IChartContext context, Action draw)
    {
        context.Updated += draw;

        return () => context.Updated -= draw;
    }
    //
    // public static Action OnPointerMove(this IChartContext context, Action<int, int> handle)
    // {
    //     context.PointerMoved += handle;
    //
    //     return () => context.PointerMoved -= handle;
    // }
}