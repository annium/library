using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.Console.Internal;

/// <summary>
/// Log handler that writes log messages to the console with optional color formatting.
/// Provides thread-safe console output with color coding based on log levels.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
internal class ConsoleLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Function to format log messages for console output.
    /// </summary>
    private readonly Func<LogMessage<TContext>, string> _format;

    /// <summary>
    /// Whether colors can actually be seen. A redirected stream takes the bytes and no one reads a color
    /// from them, so setting one per message is work with no reader.
    /// </summary>
    private readonly bool _isColored;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogHandler{TContext}"/> class.
    /// </summary>
    /// <param name="format">Formatter turning a log message into the console line.</param>
    /// <param name="color">Whether lines are colored per log level.</param>
    public ConsoleLogHandler(Func<LogMessage<TContext>, string> format, bool color)
    {
        _format = format;
        _isColored = color && !System.Console.IsOutputRedirected;
    }

    /// <summary>
    /// Writes the batch of log messages to the console under the shared console lock.
    /// </summary>
    /// <remarks>
    /// Two paths, because coloring and batching cannot both be had. A terminal gets a color per line, which
    /// means a write per line. Anything else - a file, a pipe - gets the whole batch formatted into one
    /// buffer and written once: <c>Console.Out</c> auto-flushes, so a write per line is a syscall per line,
    /// and at a batch of a thousand that is three orders of magnitude of them not made.
    ///
    /// Formatting happens before the lock either way. It is an interpolation per message, and holding a
    /// process-wide lock through it makes every other writer wait for this batch's string building.
    /// </remarks>
    /// <param name="messages">The log messages to write</param>
    /// <param name="ct">Cancellation token (unused — console writes are synchronous)</param>
    /// <returns>A completed value task</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct)
    {
        if (messages.Count == 0)
            return ValueTask.CompletedTask;

        if (_isColored)
            WriteColored(messages);
        else
            WritePlain(messages);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Formats the whole batch into one buffer and writes it in a single call.
    /// </summary>
    /// <param name="messages">The log messages to write.</param>
    private void WritePlain(IReadOnlyList<LogMessage<TContext>> messages)
    {
        var sb = new StringBuilder();
        foreach (var msg in messages)
            sb.AppendLine(_format(msg));

        var batch = sb.ToString();

        lock (StaticState.ConsoleLock)
            System.Console.Out.Write(batch);
    }

    /// <summary>
    /// Writes each line under its level's color, which a terminal can show and a batch cannot carry.
    /// </summary>
    /// <param name="messages">The log messages to write.</param>
    private void WriteColored(IReadOnlyList<LogMessage<TContext>> messages)
    {
        var lines = new (string Text, LogLevel Level)[messages.Count];
        for (var i = 0; i < messages.Count; i++)
            lines[i] = (_format(messages[i]), messages[i].Level);

        lock (StaticState.ConsoleLock)
        {
            var currentColor = System.Console.ForegroundColor;
            try
            {
                foreach (var (text, level) in lines)
                {
                    // fall back to a neutral color for any level absent from the map (e.g. LogLevel.None)
                    System.Console.ForegroundColor = StaticState.LevelColors.TryGetValue(level, out var c)
                        ? c
                        : ConsoleColor.White;
                    System.Console.WriteLine(text);
                }
            }
            finally
            {
                System.Console.ForegroundColor = currentColor;
            }
        }
    }
}
