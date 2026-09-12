using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// The structured data of a log message, built from the template's placeholder names and the values
/// bound to them only when a sink actually reads it.
/// </summary>
/// <remarks>
/// <para>
/// Two sinks in the whole library read <see cref="LogMessage{TContext}.Data"/> — the Graylog and Seq
/// ones, both of which enumerate it into their wire format. Console, file, in-memory and xunit never
/// touch it. Building the dictionary eagerly therefore charged every host for something only those two
/// ask for, which is what this defers.
/// </para>
/// <para>
/// The names and values it holds are the ones the message was produced from: the names come from the
/// cached template parse and are shared by every message from that call site, and the values are the
/// argument list the caller already passed. So deferring costs no extra state.
/// </para>
/// </remarks>
internal sealed class LogMessageData : IReadOnlyDictionary<string, object?>
{
    /// <summary>
    /// The data of a message with no values bound — shared, since it carries nothing.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, object?> Empty = ReadOnlyDictionary<string, object?>.Empty;

    /// <summary>
    /// Placeholder names, in template order. Shared with the cached template parse — never mutated.
    /// </summary>
    private readonly string[] _names;

    /// <summary>
    /// Values bound to the placeholders, positionally. May be shorter than <see cref="_names"/> when the
    /// caller passed fewer values than the template has placeholders.
    /// </summary>
    private readonly IReadOnlyList<object?> _values;

    /// <summary>
    /// The materialized pairs, or null until something reads them.
    /// </summary>
    private Dictionary<string, object?>? _materialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessageData"/> class.
    /// </summary>
    /// <param name="names">Placeholder names, in template order.</param>
    /// <param name="values">Values bound to the placeholders, positionally.</param>
    public LogMessageData(string[] names, IReadOnlyList<object?> values)
    {
        _names = names;
        _values = values;
    }

    /// <summary>
    /// Returns the pairs, binding them on first call and reusing them afterwards. A method rather than a
    /// property because the first call builds a dictionary, which is not what a property should cost.
    /// </summary>
    /// <returns>The bound pairs.</returns>
    /// <remarks>
    /// Two readers racing here both build one and the first to publish wins, so every reader ends up
    /// with the same instance; the interlocked exchange is what makes a built dictionary's contents
    /// visible to a reader that sees the reference.
    /// </remarks>
    private Dictionary<string, object?> GetPairs()
    {
        var pairs = Volatile.Read(ref _materialized);
        if (pairs is not null)
            return pairs;

        return Interlocked.CompareExchange(ref _materialized, pairs = Materialize(), null) ?? pairs;
    }

    /// <summary>
    /// Gets the value bound to a placeholder name.
    /// </summary>
    /// <param name="key">The placeholder name.</param>
    /// <returns>The bound value.</returns>
    public object? this[string key] => GetPairs()[key];

    /// <summary>
    /// Gets the placeholder names that have a value bound.
    /// </summary>
    public IEnumerable<string> Keys => GetPairs().Keys;

    /// <summary>
    /// Gets the bound values.
    /// </summary>
    public IEnumerable<object?> Values => GetPairs().Values;

    /// <summary>
    /// Gets the number of distinct placeholder names that have a value bound.
    /// </summary>
    public int Count => GetPairs().Count;

    /// <summary>
    /// Says whether a placeholder name has a value bound.
    /// </summary>
    /// <param name="key">The placeholder name.</param>
    /// <returns>True when bound.</returns>
    public bool ContainsKey(string key) => GetPairs().ContainsKey(key);

    /// <summary>
    /// Tries to get the value bound to a placeholder name.
    /// </summary>
    /// <param name="key">The placeholder name.</param>
    /// <param name="value">The bound value, when present.</param>
    /// <returns>True when bound.</returns>
    public bool TryGetValue(string key, out object? value) => GetPairs().TryGetValue(key, out value);

    /// <summary>
    /// Enumerates the bound name/value pairs.
    /// </summary>
    /// <returns>An enumerator over the pairs.</returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => GetPairs().GetEnumerator();

    /// <summary>
    /// Enumerates the bound name/value pairs.
    /// </summary>
    /// <returns>An enumerator over the pairs.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Binds names to values positionally.
    /// </summary>
    /// <returns>The bound pairs.</returns>
    /// <remarks>
    /// A name repeated in the template keeps its last value, and a name with no value — the caller
    /// passed fewer values than the template has placeholders, and the renderer re-emitted the
    /// placeholder literally — is left out. Both match what the eager build did.
    /// </remarks>
    private Dictionary<string, object?> Materialize()
    {
        var count = _values.Count < _names.Length ? _values.Count : _names.Length;
        var pairs = new Dictionary<string, object?>(count);

        for (var i = 0; i < count; i++)
            pairs[_names[i]] = _values[i];

        return pairs;
    }
}
