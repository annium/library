using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Charts.Pages.Home;

public partial class Page
{
    [Inject]
    private ITimeProvider TimeProvider { get; set; } = default!;

    [Inject]
    private IChartContext ChartContext { get; set; } = default!;

    [Inject]
    private IHttpRequestFactory Api { get; set; } = default!;

    [Inject]
    private IIndex<SerializerKey, ISerializer<string>> Serializers { get; set; } = default!;

    [Inject]
    private ISeriesSourceFactory SeriesSourceFactory { get; set; } = default!;

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger<Page> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ChartContext.Configure(ImmutableArray.Create(1, 2, 4, 8, 16), ImmutableArray.Create(1, 5, 30));

        await Task.CompletedTask;
    }
}