namespace Annium.Blazor.Charts.Domain.Lookup;

/// <summary>
/// Specifies the type of match to use when looking up values.
/// </summary>
public enum LookupMatch
{
    /// <summary>
    /// Matches values exactly.
    /// </summary>
    Exact,

    /// <summary>
    /// Matches the nearest value to the left.
    /// </summary>
    NearestLeft,

    /// <summary>
    /// Matches the nearest value to the right.
    /// </summary>
    NearestRight,
}
