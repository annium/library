using NodaTime;

namespace Annium.Blazor.Charts.Domain.Interfaces;

public interface ITimeSeries
{
    Instant Moment { get; }
}
