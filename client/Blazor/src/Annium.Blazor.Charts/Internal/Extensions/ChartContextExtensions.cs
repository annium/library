using System;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Extensions;

internal static class ChartContextExtensions
{
    public static Action OnUpdate(this IChartContext context, Action draw)
    {
        context.Updated += draw;

        return () => context.Updated -= draw;
    }

    public static Action OnLookupChanged(this IChartContext context, Action<Instant?, Point?> handle)
    {
        context.LookupChanged += handle;

        return () => context.LookupChanged -= handle;
    }
}