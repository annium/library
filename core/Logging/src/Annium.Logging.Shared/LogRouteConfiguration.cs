using System;

namespace Annium.Logging.Shared;

/// <summary>
/// Configuration settings for log route buffering behavior
/// </summary>
public record LogRouteConfiguration
{
    /// <summary>
    /// Gets the maximum time to buffer log messages before flushing
    /// </summary>
    public TimeSpan BufferTime { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the maximum number of log messages to buffer before flushing
    /// </summary>
    /// <remarks>
    /// A sink's per-batch overhead — a lock, a write, a round trip — is paid once per batch, so a batch of
    /// five means paying it forty million times for forty million lines. The time bound is what keeps a
    /// quiet process's lines from waiting; this one is what makes a busy one's batches worth batching.
    /// </remarks>
    public int BufferCount { get; init; } = 1000;
}
