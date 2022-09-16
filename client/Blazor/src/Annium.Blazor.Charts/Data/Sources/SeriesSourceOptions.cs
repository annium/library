namespace Annium.Blazor.Charts.Data.Sources;

public sealed record SeriesSourceOptions
{
    public static SeriesSourceOptions Default { get; } = new()
    {
        BufferZone = 1L,
        LoadZone = 3L,
    };

    public long BufferZone { get; init; }
    public long LoadZone { get; init; }
}