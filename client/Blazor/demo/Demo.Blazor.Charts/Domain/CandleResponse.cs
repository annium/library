using System;
using System.Text.Json.Serialization;

namespace Demo.Blazor.Charts.Domain;

public record CandleResponse
{
    [JsonPropertyName("t")]
    public long[] Moments { get; init; } = Array.Empty<long>();

    [JsonPropertyName("o")]
    public decimal[] Opens { get; init; } = Array.Empty<decimal>();

    [JsonPropertyName("h")]
    public decimal[] Highs { get; init; } = Array.Empty<decimal>();

    [JsonPropertyName("l")]
    public decimal[] Lows { get; init; } = Array.Empty<decimal>();

    [JsonPropertyName("c")]
    public decimal[] Closes { get; init; } = Array.Empty<decimal>();

    [JsonPropertyName("v")]
    public decimal[] Volumes { get; init; } = Array.Empty<decimal>();
}
