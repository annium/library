using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record MultiValue<T>(Instant Moment, IReadOnlyCollection<T> Values) : IMultiValue<T>;