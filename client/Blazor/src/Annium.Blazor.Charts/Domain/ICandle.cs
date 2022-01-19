namespace Annium.Blazor.Charts.Domain;

public interface ICandle : ITimeSeries
{
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public decimal Volume { get; }
}