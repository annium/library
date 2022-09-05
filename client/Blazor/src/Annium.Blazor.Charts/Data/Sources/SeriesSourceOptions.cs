namespace Annium.Blazor.Charts.Data.Sources;

public sealed record SeriesSourceOptions(
    long BufferZone,
    long LoadZone,
    long CacheZone
);