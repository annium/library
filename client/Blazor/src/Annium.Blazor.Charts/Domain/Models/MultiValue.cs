using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a collection of multiple values at a specific moment in time.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
/// <param name="Moment">The specific moment in time when these values were recorded.</param>
/// <param name="Items">The collection of items at the specified moment.</param>
public record MultiValue<T>(Instant Moment, IReadOnlyCollection<T> Items) : IMultiValue<T>;
