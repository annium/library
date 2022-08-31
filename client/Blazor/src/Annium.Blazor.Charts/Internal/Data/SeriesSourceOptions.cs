namespace Annium.Blazor.Charts.Internal.Data;

public sealed record SeriesSourceOptions(
    long BufferZone,
    long LoadZone,
    long CacheZone
);