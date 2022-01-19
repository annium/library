using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public interface ITimeSeries
{
    public Instant Moment { get; }
}