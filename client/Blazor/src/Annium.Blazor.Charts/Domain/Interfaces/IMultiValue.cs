using System.Collections.Generic;

namespace Annium.Blazor.Charts.Domain.Interfaces;

public interface IMultiValue<out T> : ITimeSeries
{
    IReadOnlyCollection<T> Values { get; }
}