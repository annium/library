using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

/// <summary>
/// Extension methods for Instant and ValueRange&lt;Instant&gt; types to provide string formatting
/// </summary>
public static class InstantExtensions
{
    /// <summary>
    /// The default time zone used for formatting instants
    /// </summary>
    private static readonly DateTimeZone _timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// Formats an Instant as a short string representation in the local time zone
    /// </summary>
    /// <param name="t">The instant to format</param>
    /// <returns>A formatted string in "dd.MM.yyyy HH:mm" format</returns>
    public static string S(this Instant t) => t.InZone(_timeZone).LocalDateTime.ToString("dd.MM.yyyy HH:mm", null);

    /// <summary>
    /// Formats a ValueRange of Instants as a short string representation showing the range
    /// </summary>
    /// <param name="r">The value range to format</param>
    /// <returns>A formatted string showing "start - end" in short format</returns>
    public static string S(this ValueRange<Instant> r) => $"{r.Start.S()} - {r.End.S()}";
}
