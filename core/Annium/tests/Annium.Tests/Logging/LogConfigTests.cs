using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Logging;

/// <summary>
/// Pins <see cref="LogConfig.IsEnabled"/>, the guard a call site uses when producing its log arguments costs
/// something. The <c>Trace</c> / <c>Debug</c> / … methods check the level themselves, but C# evaluates their
/// arguments first, so a call whose argument decodes a payload or walks a collection pays for it with logging
/// off unless the call site asks first.
/// </summary>
public class LogConfigTests
{
    /// <summary>
    /// Answers for exactly the levels the logger itself would let through — at or above the global level.
    /// </summary>
    /// <param name="configured">The global level to set for the check.</param>
    /// <param name="asked">The level a call site would be asking about.</param>
    /// <param name="expected">Whether that level is expected to pass.</param>
    [Theory]
    [InlineData(LogLevel.Trace, LogLevel.Trace, true)]
    [InlineData(LogLevel.Trace, LogLevel.Error, true)]
    [InlineData(LogLevel.Info, LogLevel.Trace, false)]
    [InlineData(LogLevel.Info, LogLevel.Debug, false)]
    [InlineData(LogLevel.Info, LogLevel.Info, true)]
    [InlineData(LogLevel.Info, LogLevel.Warn, true)]
    [InlineData(LogLevel.None, LogLevel.Error, false)]
    public void IsEnabled_FollowsTheGlobalLevel(LogLevel configured, LogLevel asked, bool expected)
    {
        // arrange
        var initial = LogConfig.Level;
        try
        {
            LogConfig.SetLevel(configured);

            // act & assert
            LogConfig.IsEnabled(asked).Is(expected, $"level {configured}, asked about {asked}");
        }
        finally
        {
            LogConfig.SetLevel(initial);
        }
    }
}
