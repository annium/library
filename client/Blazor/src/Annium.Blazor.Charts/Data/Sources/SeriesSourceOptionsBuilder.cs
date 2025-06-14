using System.Collections.Generic;
using Annium.Blazor.Charts.Internal.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

/// <summary>
/// Builder for creating series source options with resolution-specific configurations.
/// </summary>
public class SeriesSourceOptionsBuilder
{
    /// <summary>
    /// The dictionary containing resolution-specific options.
    /// </summary>
    private readonly Dictionary<Duration, SeriesSourceResolutionOptions> _options;

    /// <summary>
    /// Default series source options with predefined resolution configurations.
    /// </summary>
    public static readonly ISeriesSourceOptions Default = Init(Duration.FromMinutes(1), 1.3m, 1.5m)
        .Set(Duration.FromMinutes(15), 1.1m, 1.3m)
        .Set(Duration.FromMinutes(30), 0.9m, 1.1m)
        .Set(Duration.FromMinutes(60), 0.7m, 0.9m)
        .Set(Duration.FromMinutes(120), 0.5m, 0.7m)
        .Set(Duration.FromMinutes(240), 0.3m, 0.5m)
        .Build();

    /// <summary>
    /// Initializes a new series source options builder with the specified initial resolution configuration.
    /// </summary>
    /// <param name="resolution">The initial resolution to configure.</param>
    /// <param name="bufferZone">The buffer zone multiplier for the resolution.</param>
    /// <param name="loadZone">The load zone multiplier for the resolution.</param>
    /// <returns>A new SeriesSourceOptionsBuilder instance.</returns>
    public static SeriesSourceOptionsBuilder Init(Duration resolution, decimal bufferZone, decimal loadZone)
    {
        var options = new Dictionary<Duration, SeriesSourceResolutionOptions>
        {
            { resolution, new SeriesSourceResolutionOptions(bufferZone, loadZone) },
        };

        return new SeriesSourceOptionsBuilder(options);
    }

    /// <summary>
    /// Initializes a new instance of the SeriesSourceOptionsBuilder class.
    /// </summary>
    /// <param name="options">The initial dictionary of resolution options.</param>
    private SeriesSourceOptionsBuilder(Dictionary<Duration, SeriesSourceResolutionOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the options for the specified resolution.
    /// </summary>
    /// <param name="resolution">The resolution to configure.</param>
    /// <param name="bufferZone">The buffer zone multiplier for the resolution.</param>
    /// <param name="loadZone">The load zone multiplier for the resolution.</param>
    /// <returns>The current SeriesSourceOptionsBuilder instance for method chaining.</returns>
    public SeriesSourceOptionsBuilder Set(Duration resolution, decimal bufferZone, decimal loadZone)
    {
        _options.Add(resolution, new SeriesSourceResolutionOptions(bufferZone, loadZone));

        return this;
    }

    /// <summary>
    /// Builds the series source options from the configured resolutions.
    /// </summary>
    /// <returns>A new ISeriesSourceOptions instance.</returns>
    public ISeriesSourceOptions Build()
    {
        return new SeriesSourceOptions(_options);
    }
}
