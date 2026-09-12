using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NodaTime;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Bridge implementation that converts log registration calls to log messages and forwards them to the sentry
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class LogSentryBridge<TContext> : ILogSentryBridge
    where TContext : class
{
    /// <summary>
    /// Cache of source-file names, keyed on the path instance.
    /// </summary>
    /// <remarks>
    /// The path arrives from <c>[CallerFilePath]</c>, so it is a compile-time constant and the same
    /// instance on every call from a given site — the file name derived from it never changes, and
    /// computing it per message allocated a string per message. Weak for the same reason the template
    /// cache is: an external bridge can pass something else, and an entry keyed on it dies with it.
    /// </remarks>
    private static readonly ConditionalWeakTable<string, string> _fileNames = new();

    /// <summary>
    /// Held in a field so the method group is converted to a delegate once rather than per lookup.
    /// </summary>
    private static readonly ConditionalWeakTable<string, string>.CreateValueCallback _resolveFileName =
        Path.GetFileNameWithoutExtension;

    /// <summary>
    /// Time provider for generating timestamps
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// The log context instance
    /// </summary>
    private readonly TContext _context;

    /// <summary>
    /// The log sentry for receiving log messages
    /// </summary>
    private readonly ILogSentry<TContext> _logSentry;

    /// <summary>
    /// Live view of the registered schedulers used to evaluate <see cref="IsLevelEnabled"/>. Indexed
    /// rather than enumerated, because enumerating through the interface boxes an enumerator per call.
    /// </summary>
    /// <remarks>
    /// Cannot be snapshotted: routes are registered by <c>UseLogging</c> after the provider — and so
    /// this bridge — is built, so a copy taken in the constructor would be empty forever.
    /// </remarks>
    private readonly IReadOnlyList<ILogScheduler<TContext>> _schedulers;

    /// <summary>
    /// Synthetic messages for <see cref="IsLevelEnabled"/>, one slot per <see cref="LogLevel"/>, filled
    /// on first use of that level.
    /// </summary>
    private readonly LogMessage<TContext>?[] _synthetic = new LogMessage<TContext>?[Enum.GetValues<LogLevel>().Length];

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSentryBridge{TContext}"/> class.
    /// </summary>
    /// <param name="timeProvider">Time provider used to stamp messages.</param>
    /// <param name="context">Context attached to every produced message.</param>
    /// <param name="logSentry">Sentry the produced messages are registered with.</param>
    /// <param name="schedulers">Schedulers consulted to decide whether a level is enabled.</param>
    public LogSentryBridge(
        ITimeProvider timeProvider,
        TContext context,
        ILogSentry<TContext> logSentry,
        IReadOnlyList<ILogScheduler<TContext>> schedulers
    )
    {
        _timeProvider = timeProvider;
        _context = context;
        _logSentry = logSentry;
        _schedulers = schedulers;
    }

    /// <summary>
    /// Reports whether any registered scheduler's filter would accept a message at the given level.
    /// </summary>
    /// <param name="level">The level to test</param>
    /// <returns>True if at least one scheduler's filter accepts; false otherwise</returns>
    public bool IsLevelEnabled(LogLevel level)
    {
        var schedulers = _schedulers;
        if (schedulers.Count == 0)
            return false;

        var synthetic = GetSynthetic(level);

        // a plain loop, not Any(predicate): the predicate would capture the synthetic message, so each
        // call allocated a closure, a delegate and an enumerator to answer a question about a bool
        for (var i = 0; i < schedulers.Count; i++)
            if (schedulers[i].Filter(synthetic))
                return true;

        return false;
    }

    /// <summary>
    /// Registers a log message by creating a LogMessage and forwarding it to the sentry
    /// </summary>
    /// <param name="subjectType">The type of the logging subject</param>
    /// <param name="subjectId">The identifier of the logging subject</param>
    /// <param name="file">The source file where the log was generated</param>
    /// <param name="member">The member where the log was generated</param>
    /// <param name="line">The line number where the log was generated</param>
    /// <param name="level">The log level</param>
    /// <param name="messageTemplate">The message template</param>
    /// <param name="exception">The exception associated with the log</param>
    /// <param name="dataItems">Additional data items for the log</param>
    public void Register(
        string subjectType,
        string subjectId,
        string file,
        string member,
        int line,
        LogLevel level,
        string messageTemplate,
        Exception? exception,
        IReadOnlyList<object?> dataItems
    )
    {
        var instant = _timeProvider.Now;
        var (message, data) = Helper.Process(messageTemplate, dataItems);

        var msg = new LogMessage<TContext>(
            _context,
            instant,
            subjectType,
            subjectId,
            level,
            Thread.CurrentThread.ManagedThreadId,
            exception is null ? message : LogMessageEnricher.GetExceptionMessage(exception),
            exception?.Demystify(),
            messageTemplate,
            data,
            _fileNames.GetValue(file, _resolveFileName),
            member,
            line
        );

        _logSentry.Register(msg);
    }

    /// <summary>
    /// Returns the synthetic message used to probe the filters at the given level, building it on first
    /// use of that level and reusing it afterwards.
    /// </summary>
    /// <param name="level">The level to probe.</param>
    /// <returns>A message whose only meaningful field is the level.</returns>
    /// <remarks>
    /// Only <c>Level</c> is meaningful; a filter that keys off another field evaluates against an
    /// empty or default value, which is intentional — <c>IsLevelEnabled</c> is an optimization hint and
    /// the real filter still runs on the real message in the router. Reused rather than rebuilt because
    /// building one allocated a message per probe, and a probe exists to avoid allocating.
    /// </remarks>
    private LogMessage<TContext> GetSynthetic(LogLevel level)
    {
        var index = (int)level;
        if ((uint)index >= (uint)_synthetic.Length)
            return BuildSynthetic(level);

        var synthetic = Volatile.Read(ref _synthetic[index]);
        if (synthetic is not null)
            return synthetic;

        // two probes at the same level can both build one; the first to publish wins and they are
        // equivalent, so which one does is immaterial
        return Interlocked.CompareExchange(ref _synthetic[index], synthetic = BuildSynthetic(level), null) ?? synthetic;
    }

    /// <summary>
    /// Builds a synthetic message carrying nothing but the level.
    /// </summary>
    /// <param name="level">The level to carry.</param>
    /// <returns>The synthetic message.</returns>
    private LogMessage<TContext> BuildSynthetic(LogLevel level) =>
        new(
            _context,
            Instant.MinValue,
            string.Empty,
            string.Empty,
            level,
            0,
            string.Empty,
            null,
            string.Empty,
            LogMessageData.Empty,
            string.Empty,
            string.Empty,
            0
        );
}
