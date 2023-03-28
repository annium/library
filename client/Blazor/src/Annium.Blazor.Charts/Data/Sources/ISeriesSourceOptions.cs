using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSourceOptions
{
    SeriesSourceResolutionOptions GetForResolution(Duration resolution);
}

public record struct SeriesSourceResolutionOptions(decimal BufferZone, decimal LoadZone);