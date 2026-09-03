using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

/// <summary>
/// Provides configuration options for series sources based on resolution.
/// </summary>
public interface ISeriesSourceOptions
{
    /// <summary>
    /// Gets the resolution-specific options for the given duration.
    /// </summary>
    /// <param name="resolution">The resolution duration to get options for.</param>
    /// <returns>The resolution-specific options.</returns>
    SeriesSourceResolutionOptions GetForResolution(Duration resolution);
}

/// <summary>
/// Represents resolution-specific options for series sources, including buffer and load zones.
/// </summary>
/// <param name="BufferZone">The buffer zone multiplier for data caching.</param>
/// <param name="LoadZone">The load zone multiplier for data loading.</param>
public record struct SeriesSourceResolutionOptions(decimal BufferZone, decimal LoadZone);
