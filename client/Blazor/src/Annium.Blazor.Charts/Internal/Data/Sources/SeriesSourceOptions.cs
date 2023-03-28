using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal sealed record SeriesSourceOptions : ISeriesSourceOptions
{
    private readonly IReadOnlyDictionary<Duration, SeriesSourceResolutionOptions> _options;

    public SeriesSourceOptions(IReadOnlyDictionary<Duration, SeriesSourceResolutionOptions> options)
    {
        _options = options;
    }

    public SeriesSourceResolutionOptions GetForResolution(Duration resolution)
    {
        foreach (var (target, options) in _options)
            if (target <= resolution)
                return options;

        throw new InvalidOperationException($"No configuration for resolution {resolution}. Add resolution configuration for this or lesser resolution");
    }
}