namespace Annium.Blazor.Charts.Internal.Data.Sources;

public sealed record SeriesSourceOptions(
    long BufferZone,
    long LoadZone,
    long CacheZone
);