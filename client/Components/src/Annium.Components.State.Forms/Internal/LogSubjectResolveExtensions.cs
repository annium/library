using System;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Provides a shared resolve-or-log helper for <see cref="ILogSubject"/> implementations used by state containers.
/// </summary>
internal static class LogSubjectResolveExtensions
{
    /// <summary>
    /// Invokes the specified resolution function, logging and rethrowing any exception it raises.
    /// </summary>
    /// <typeparam name="T">The type of value being resolved.</typeparam>
    /// <param name="subject">The log subject to log errors against.</param>
    /// <param name="resolve">The resolution function to invoke.</param>
    /// <returns>The value returned by <paramref name="resolve"/>.</returns>
    internal static T ResolveOrLog<T>(this ILogSubject subject, Func<T> resolve)
    {
        try
        {
            return resolve();
        }
        catch (Exception e)
        {
            subject.Error(e);
            throw;
        }
    }
}
