using Annium.Blazor.Charts.Domain;
using NodaTime;

namespace Demo.Blazor.Charts.Domain;

public sealed record Candle(
    Instant Moment,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : ICandle;