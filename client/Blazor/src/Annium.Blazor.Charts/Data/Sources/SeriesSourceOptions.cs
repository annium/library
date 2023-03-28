namespace Annium.Blazor.Charts.Data.Sources;

public sealed record SeriesSourceOptions
{
    public static SeriesSourceOptions Default { get; } = new()
    {
        BufferZone = 0.25m,
        LoadZone = 0.4m,
    };

    public decimal BufferZone { get; init; }
    public decimal LoadZone { get; init; }
}