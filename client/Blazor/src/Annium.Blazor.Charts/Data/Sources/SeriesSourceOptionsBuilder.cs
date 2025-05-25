using System.Collections.Generic;
using Annium.Blazor.Charts.Internal.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public class SeriesSourceOptionsBuilder
{
    private readonly Dictionary<Duration, SeriesSourceResolutionOptions> _options;

    public static readonly ISeriesSourceOptions Default = Init(Duration.FromMinutes(1), 1.3m, 1.5m)
        .Set(Duration.FromMinutes(15), 1.1m, 1.3m)
        .Set(Duration.FromMinutes(30), 0.9m, 1.1m)
        .Set(Duration.FromMinutes(60), 0.7m, 0.9m)
        .Set(Duration.FromMinutes(120), 0.5m, 0.7m)
        .Set(Duration.FromMinutes(240), 0.3m, 0.5m)
        .Build();

    public static SeriesSourceOptionsBuilder Init(Duration resolution, decimal bufferZone, decimal loadZone)
    {
        var options = new Dictionary<Duration, SeriesSourceResolutionOptions>
        {
            { resolution, new SeriesSourceResolutionOptions(bufferZone, loadZone) },
        };

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
