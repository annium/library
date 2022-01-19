namespace Annium.Blazor.Charts.Domain;

public interface IValue : ITimeSeries
{
    public decimal Value { get; }
}