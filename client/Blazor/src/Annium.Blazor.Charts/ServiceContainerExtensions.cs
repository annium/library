using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Data.Sources;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddCharts(this IServiceContainer container)
    {
        // data
        container.Add<ISeriesSourceFactory, SeriesSourceFactory>().Transient();

        // contexts
        container.Add<IChartContext, ChartContext>().Transient();
        container.Add<IManagedPaneContext, PaneContext>().Transient();
        container.Add<IManagedSeriesContext, SeriesContext>().Transient();
        container.Add<IManagedHorizontalSideContext, HorizontalSideContext>().Transient();
        container.Add<IManagedVerticalSideContext, VerticalSideContext>().Transient();

        return container;
    }
}
