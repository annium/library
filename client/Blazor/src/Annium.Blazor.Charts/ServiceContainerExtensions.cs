using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Data;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Models.Contexts;
using Annium.Core.DependencyInjection;

namespace Annium.Blazor.Charts;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddCharts(this IServiceContainer container)
    {
        container.Add<IChartContext, ChartContext>().Transient();
        container.Add<IManagedPaneContext, PaneContext>().Transient();
        container.Add<IManagedSeriesContext, SeriesContext>().Transient();
        container.Add<IManagedHorizontalSideContext, HorizontalSideContext>().Transient();
        container.Add<IManagedVerticalSideContext, VerticalSideContext>().Transient();
        container.Add<ISeriesSourceFactory, SeriesSourceFactory>().Transient();

        return container;
    }
}