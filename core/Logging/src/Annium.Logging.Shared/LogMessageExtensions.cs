using System;
using System.Runtime.CompilerServices;
using System.Text;
using NodaTime;
using NodaTime.Text;

namespace Annium.Logging.Shared;

/// <summary>
/// Extension methods for LogMessage to provide formatting and time zone conversion functionality
/// </summary>
public static class LogMessageExtensions
{
    /// <summary>
    /// Format for a full local/UTC date-and-time stamp.
    /// </summary>
    private const string DateTimeFormat = "dd.MM.yy HH:mm:ss.fff";

    /// <summary>
    /// Format for a time-only stamp.
    /// </summary>
    private const string TimeFormat = "HH:mm:ss.fff";

    /// <summary>
    /// The two stamp formats, resolved once.
    /// </summary>
    /// <remarks>
    /// <c>LocalDateTime.ToString(format, null)</c> resolves the format info for the current culture and
    /// then looks the pattern up in that culture's pattern cache, on every call - which is once per
    /// message written. Holding the pattern skips both lookups.
    ///
    /// Invariant, and that is a deliberate change of behaviour rather than a detail of the refactor. In a
    /// NodaTime pattern <c>:</c> is the time separator and the <c>.</c> before <c>fff</c> is the decimal
    /// separator, so both follow the culture: the same message reads <c>14:05:06.789</c> on most machines
    /// and <c>14.05.06.789</c> under fi-FI. A timestamp whose shape depends on where the process happens
    /// to run is worse than one that does not, and for the fi-FI form the date and time separators
    /// collide. Pinned to invariant so every host writes the same stamp.
    /// </remarks>
    private static readonly LocalDateTimePattern _dateTimePattern = LocalDateTimePattern.CreateWithInvariantCulture(
        DateTimeFormat
    );

    /// <summary>
    /// Time-only stamp format, resolved once - see <see cref="_dateTimePattern" />.
    /// </summary>
    private static readonly LocalDateTimePattern _timePattern = LocalDateTimePattern.CreateWithInvariantCulture(
        TimeFormat
    );

    /// <summary>
    /// The current system time zone
    /// </summary>
    private static readonly DateTimeZone _currentTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// The UTC time zone
    /// </summary>
    private static readonly DateTimeZone _utcTz = DateTimeZone.Utc;

    /// <summary>
    /// Creates a default formatter function for log messages
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="time">Function to format the time portion of the message</param>
    /// <returns>A formatter function for log messages</returns>
    public static Func<LogMessage<TContext>, string> DefaultFormat<TContext>(Func<LogMessage<TContext>, string> time)
        where TContext : class
    {
        return m =>
        {
            var sb = new StringBuilder();
            sb.Append(m.Subject());
            if (m.Line != 0)
                sb.Append($" at {m.Location()}");

            return $"[{time(m)}] {m.Level} [{m.ThreadId:D3}] {sb} >> {m.Message}";
        };
    }

    /// <summary>
    /// Gets the subject string representation (type and optional ID)
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted subject string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Subject<TContext>(this LogMessage<TContext> m)
        where TContext : class =>
        string.IsNullOrWhiteSpace(m.SubjectId) ? m.SubjectType : $"{m.SubjectType}#{m.SubjectId}";

    /// <summary>
    /// Gets the location string representation (type.member:line)
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted location string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Location<TContext>(this LogMessage<TContext> m)
        where TContext : class => $"{m.Type}.{m.Member}:{m.Line}";

    /// <summary>
    /// Formats the log message timestamp as local date and time
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted local date and time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return _dateTimePattern.Format(m.Instant.InZone(_currentTz).LocalDateTime);
    }

    /// <summary>
    /// Formats the log message timestamp as local time only
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted local time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return _timePattern.Format(m.Instant.InZone(_currentTz).LocalDateTime);
    }

    /// <summary>
    /// Formats the log message timestamp as UTC date and time
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted UTC date and time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return _dateTimePattern.Format(m.Instant.InZone(_utcTz).LocalDateTime);
    }

    /// <summary>
    /// Formats the log message timestamp as UTC time only
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted UTC time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return _timePattern.Format(m.Instant.InZone(_utcTz).LocalDateTime);
    }
}
