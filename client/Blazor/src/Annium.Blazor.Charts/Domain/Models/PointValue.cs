using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record PointValue(Instant Moment, decimal Value) : IPointValue;
