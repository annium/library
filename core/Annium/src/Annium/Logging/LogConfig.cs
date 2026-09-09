using System;
using System.Threading;

namespace Annium.Logging;

/// <summary>
/// Provides configuration for logging, including the global log level.
/// </summary>
public static class LogConfig
{
    /// <summary>
    /// Backing storage for <see cref="Level"/>; accessed via <see cref="Volatile"/> for cross-thread visibility.
    /// </summary>
    private static int _level;

    /// <summary>
    /// Gets the current global log level.
    /// </summary>
    public static LogLevel Level => (LogLevel)Volatile.Read(ref _level);

    /// <summary>
    /// Initializes the global log level from the <c>-trace</c> / <c>-debug</c> command-line switches,
    /// falling back to the <c>ANNIUM_LOG</c> environment variable and then to <see cref="LogLevel.Info"/>.
    /// </summary>
    static LogConfig()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Contains("-trace"))
        {
            Volatile.Write(ref _level, (int)LogLevel.Trace);
            return;
        }

        if (args.Contains("-debug"))
        {
            Volatile.Write(ref _level, (int)LogLevel.Debug);
            return;
        }

        var raw = Environment.GetEnvironmentVariable("ANNIUM_LOG");
        switch (raw?.Trim())
        {
            case "trace":
                Volatile.Write(ref _level, (int)LogLevel.Trace);
                break;
            case "debug":
                Volatile.Write(ref _level, (int)LogLevel.Debug);
                break;
            default:
                Volatile.Write(ref _level, (int)LogLevel.Info);
                break;
        }
    }

    /// <summary>
    /// Says whether a message at the given level would be logged at all.
    /// </summary>
    /// <remarks>
    /// For call sites whose arguments cost something to produce. The <c>Trace</c> / <c>Debug</c> / … methods
    /// check the level themselves, but C# evaluates their arguments first, so a call like
    /// <c>this.Trace("state: {s}", Describe(everything))</c> pays for <c>Describe</c> even with logging off.
    /// Guarding such a call with this makes the cost follow the level. A call whose arguments are fields or
    /// locals needs no guard.
    /// </remarks>
    /// <param name="level">The level to check.</param>
    /// <returns>Whether a message at that level passes the global level filter.</returns>
    public static bool IsEnabled(LogLevel level) => Level <= level;

    /// <summary>
    /// Sets the global log level.
    /// </summary>
    /// <param name="level">The log level to set.</param>
    public static void SetLevel(LogLevel level) => Volatile.Write(ref _level, (int)level);
}
