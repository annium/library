namespace Annium.Blazor.Charts.Domain.Interfaces;

public interface ISingleValue<out T> : ITimeSeries
{
    T Item { get; }
}