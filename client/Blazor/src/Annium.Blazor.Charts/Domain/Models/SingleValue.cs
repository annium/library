using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record SingleValue<T>(Instant Moment, T Item) : ISingleValue<T>;
