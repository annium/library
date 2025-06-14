using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a single value with a timestamp and a generic item.
/// </summary>
/// <typeparam name="T">The type of the item being stored.</typeparam>
/// <param name="Moment">The timestamp when the item was recorded.</param>
/// <param name="Item">The item value at the specified moment.</param>
public record SingleValue<T>(Instant Moment, T Item) : ISingleValue<T>;
