using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

public static class InstantExtensions
{
    private static readonly DateTimeZone TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    public static string S(this Instant t) => t.InZone(TimeZone).LocalDateTime.ToString("dd.MM.yy HH:mm", null);
}