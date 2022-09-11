using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record RangeValue(Instant Moment, decimal Low, decimal High) : IRangeValue;