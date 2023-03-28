using System.Collections.Generic;
using Annium.Blazor.Charts.Internal.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public class SeriesSourceOptionsBuilder
{
    private readonly Dictionary<Duration, SeriesSourceResolutionOptions> _options;

    public static readonly ISeriesSourceOptions Default = Init(Duration.FromMinutes(1), 9.6m, 12.8m)
        .Set(Duration.FromMinutes(15), 4.8m, 6.4m)
        .Set(Duration.FromMinutes(30), 2.4m, 3.2m)
        .Set(Duration.FromMinutes(60), 1.2m, 1.6m)
        .Set(Duration.FromMinutes(120), 0.6m, 0.8m)
        .Set(Duration.FromMinutes(240), 0.3m, 0.4m)
        .Build();

    public static SeriesSourceOptionsBuilder Init(Duration resolution, decimal bufferZone, decimal loadZone)
    {
        var options = new Dictionary<Duration, SeriesSourceResolutionOptions>() { { resolution, new SeriesSourceResolutionOptions(bufferZone, loadZone) } };

        return new SeriesSourceOptionsBuilder(options);
    }

    private SeriesSourceOptionsBuilder(Dictionary<Duration, SeriesSourceResolutionOptions> options)
    {
        _options = options;
    }

    public SeriesSourceOptionsBuilder Set(Duration resolution, decimal bufferZone, decimal loadZone)
    {
        _options.Add(resolution, new SeriesSourceResolutionOptions(bufferZone, loadZone));

        return this;
    }

    public ISeriesSourceOptions Build()
    {
        return new SeriesSourceOptions(_options);
    }
}