using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record Candle(
    Instant Moment,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : ITimeSeries;