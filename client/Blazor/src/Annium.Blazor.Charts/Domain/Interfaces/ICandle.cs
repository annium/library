namespace Annium.Blazor.Charts.Domain.Interfaces;

public interface ICandle : ITimeSeries
{
    decimal Open { get; }
    decimal High { get; }
    decimal Low { get; }
    decimal Close { get; }
}