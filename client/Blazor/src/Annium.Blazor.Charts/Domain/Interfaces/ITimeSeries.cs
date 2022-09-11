using NodaTime;

namespace Annium.Blazor.Charts.Domain.Interfaces;

public interface ITimeSeries
{
    public Instant Moment { get; }
}