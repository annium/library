using System.Collections.Generic;

namespace Annium.Blazor.Charts.Domain;

public interface IMultiValue<out T> : ITimeSeries
{
    IReadOnlyCollection<T> Values { get; }
}