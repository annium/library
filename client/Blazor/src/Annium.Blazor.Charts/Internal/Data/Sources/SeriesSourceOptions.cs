using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

/// <summary>
/// Provides series source options mapped by duration resolution
/// </summary>
internal sealed record SeriesSourceOptions : ISeriesSourceOptions
{
    /// <summary>
    /// Dictionary mapping duration resolutions to their corresponding options
    /// </summary>
    private readonly IReadOnlyDictionary<Duration, SeriesSourceResolutionOptions> _options;

    /// <summary>
    /// Initializes a new instance of SeriesSourceOptions
    /// </summary>
    /// <param name="options">Dictionary of duration resolutions and their corresponding options</param>
    public SeriesSourceOptions(IReadOnlyDictionary<Duration, SeriesSourceResolutionOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Gets the series source resolution options for the specified resolution
    /// </summary>
    /// <param name="resolution">The duration resolution to get options for</param>
    /// <returns>The series source resolution options for the specified resolution</returns>
    public SeriesSourceResolutionOptions GetForResolution(Duration resolution)
    {
        foreach (var (target, options) in _options)
            if (target <= resolution)
                return options;

        throw new InvalidOperationException(
            $"No configuration for resolution {resolution}. Add resolution configuration for this or lesser resolution"
        );
    }
}
