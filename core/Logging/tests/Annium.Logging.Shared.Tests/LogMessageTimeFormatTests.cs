using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Logging.Shared.Tests;

/// <summary>
/// Pins the shape of a log timestamp. The stamps are now produced from patterns built once with the
/// invariant culture instead of <c>ToString(format, null)</c>, which followed the current one.
/// </summary>
/// <remarks>
/// That is a behaviour change, not only a speed one, so it is written down here rather than left to be
/// discovered: under a culture whose time separator is not <c>:</c> the old path produced a different
/// stamp for the same instant. fi-FI is such a culture, which is why it appears below by name.
/// </remarks>
public class LogMessageTimeFormatTests
{
    /// <summary>
    /// Cultures that leave the two formats alone, so old and new output must agree.
    /// </summary>
    /// <returns>Culture names under which the two formats render identically.</returns>
    public static TheoryData<string> UnaffectedCultures() => new("", "en-US", "ru-RU", "de-DE", "fr-FR");

    /// <summary>
    /// Under a culture that leaves these formats alone, the stamp is what it always was.
    /// </summary>
    /// <param name="cultureName">Culture to run under.</param>
    [Theory]
    [MemberData(nameof(UnaffectedCultures))]
    public void Stamps_MatchWhatTheCultureUsedToProduce(string cultureName)
    {
        // arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var instant = Instant.FromUtc(2026, 9, 13, 14, 5, 6) + Duration.FromMilliseconds(789);
        var message = Message(instant);

        // act
        var (dateTime, time) = WithCulture(
            culture,
            () =>
                (
                    instant.InUtc().LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null),
                    instant.InUtc().LocalDateTime.ToString("HH:mm:ss.fff", null)
                )
        );

        // assert
        LogMessageExtensions.UtcDateTime(message).Is(dateTime);
        LogMessageExtensions.UtcTime(message).Is(time);
    }

    /// <summary>
    /// Under a culture that does not, the stamp no longer follows it - which is the behaviour change.
    /// </summary>
    [Fact]
    public void Stamps_DoNotFollowTheCulture()
    {
        // arrange - fi-FI writes the time separator as a period, so the old path produced 14.05.06.789
        // here and 14:05:06.789 everywhere else. The stamp no longer moves with the machine.
        var instant = Instant.FromUtc(2026, 9, 13, 14, 5, 6) + Duration.FromMilliseconds(789);
        var message = Message(instant);
        var finnish = CultureInfo.GetCultureInfo("fi-FI");

        // act
        var (dateTime, time) = WithCulture(
            finnish,
            () => (LogMessageExtensions.UtcDateTime(message), LogMessageExtensions.UtcTime(message))
        );

        // assert
        dateTime.Is("13.09.26 14:05:06.789");
        time.Is("14:05:06.789");

        // and the old path really did differ, so this test is guarding something
        WithCulture(finnish, () => instant.InUtc().LocalDateTime.ToString("HH:mm:ss.fff", null)).IsNot(time);
    }

    /// <summary>
    /// Builds a message carrying the given moment; nothing else about it matters here.
    /// </summary>
    /// <param name="instant">Moment the stamp is taken of.</param>
    /// <returns>A message to format.</returns>
    private static LogMessage<DefaultLogContext> Message(Instant instant) =>
        new(
            new DefaultLogContext(),
            instant,
            nameof(LogMessageTimeFormatTests),
            string.Empty,
            LogLevel.Info,
            1,
            "message",
            null,
            "message",
            new Dictionary<string, object?>(),
            nameof(LogMessageTimeFormatTests),
            nameof(Message),
            0
        );

    /// <summary>
    /// Runs the callback under the given culture and restores whatever was there before.
    /// </summary>
    /// <typeparam name="T">The callback's result type.</typeparam>
    /// <param name="culture">Culture to run under.</param>
    /// <param name="act">The callback.</param>
    /// <returns>Whatever the callback returned.</returns>
    private static T WithCulture<T>(CultureInfo culture, Func<T> act)
    {
        var saved = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
        try
        {
            return act();
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }
}
