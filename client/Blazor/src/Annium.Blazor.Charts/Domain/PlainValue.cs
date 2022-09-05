using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record PlainValue(Instant Moment, decimal Value) : ITimeSeries;