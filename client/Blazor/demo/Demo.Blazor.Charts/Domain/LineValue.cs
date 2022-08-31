using Annium.Blazor.Charts.Domain;
using NodaTime;

namespace Demo.Blazor.Charts.Domain;

public sealed record LineValue(Instant Moment, decimal Value) : IValue;