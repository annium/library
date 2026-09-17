using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Annium.Serialization.Json;

/// <summary>
/// Turns a UTF-8 JSON string token into a <see cref="string"/>, reusing the instance when the same bytes
/// have been seen before.
/// </summary>
/// <remarks>
/// <para>
/// For values drawn from a small fixed set — a stream name, an instrument symbol — every message carries
/// the same handful of bytes, and <c>reader.GetString()</c> allocates a fresh string for each one. At ten
/// thousand messages a second that is ten thousand identical strings a second, all of them garbage by the
/// next collection.
/// </para>
/// <para>
/// <b>This is not <see cref="string.Intern(string)"/>, and interning would not do.</b> To intern a string
/// you must first have one, so the allocation has already happened; interning only deduplicates afterwards,
/// into a pool that is never collected. The point here is to answer from the bytes, before a string exists:
/// the dictionary is looked up through an alternate comparer keyed on <c>ReadOnlySpan&lt;byte&gt;</c>, and
/// only a value never seen before allocates.
/// </para>
/// <para>
/// The cache is bounded. The values it is meant for are few, but "few" is a property of the sender, not of
/// this code, and a cache that grows with whatever arrives is a memory leak addressed by a stranger. Past
/// the bound it simply stops adding and keeps answering correctly, more slowly.
/// </para>
/// </remarks>
public sealed class Utf8StringCache
{
    /// <summary>
    /// How many distinct values the cache will hold.
    /// </summary>
    /// <remarks>
    /// Chosen well above any plausible set of stream names or subscribed symbols, and far below anything
    /// that would matter as memory. The bound exists to cap a pathological sender, not to be tuned.
    /// </remarks>
    private const int Capacity = 1024;

    /// <summary>The cached instances, keyed by their own text.</summary>
    private readonly ConcurrentDictionary<string, string> _values;

    /// <summary>The lookup that answers from UTF-8 bytes without building a string first.</summary>
    private readonly ConcurrentDictionary<string, string>.AlternateLookup<ReadOnlySpan<byte>> _lookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="Utf8StringCache"/> class.
    /// </summary>
    public Utf8StringCache()
    {
        _values = new ConcurrentDictionary<string, string>(Utf8Comparer.Instance);
        _lookup = _values.GetAlternateLookup<ReadOnlySpan<byte>>();
    }

    /// <summary>
    /// Reads the reader's current string token, reusing a previously cached instance when the bytes match.
    /// </summary>
    /// <param name="reader">A reader positioned on a string token.</param>
    /// <returns>The string, or null if the token is null.</returns>
    public string? GetString(ref Utf8JsonReader reader)
    {
        // an escaped or multi-segment token is not the case this exists for, and reconstructing the bytes
        // to look it up would cost more than the allocation it saves
        if (reader.HasValueSequence || reader.ValueIsEscaped)
        {
            return reader.GetString();
        }

        var span = reader.ValueSpan;

        if (_lookup.TryGetValue(span, out var cached))
        {
            return cached;
        }

        var value = Encoding.UTF8.GetString(span);

        if (_values.Count < Capacity)
        {
            _values.TryAdd(value, value);
        }

        return value;
    }

    /// <summary>
    /// Compares cached strings by text, and looks them up by their UTF-8 bytes.
    /// </summary>
    private sealed class Utf8Comparer
        : IEqualityComparer<string>,
            IAlternateEqualityComparer<ReadOnlySpan<byte>, string>
    {
        /// <summary>The shared instance; the comparer holds no state.</summary>
        public static readonly Utf8Comparer Instance = new();

        /// <summary>Compares two keys by ordinal text.</summary>
        /// <param name="x">The first key.</param>
        /// <param name="y">The second key.</param>
        /// <returns>True when the two are the same text.</returns>
        public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);

        /// <summary>Hashes a key by ordinal text.</summary>
        /// <param name="obj">The key.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(string obj) => obj.GetHashCode(StringComparison.Ordinal);

        /// <summary>Compares a candidate's bytes against a stored key without building a string.</summary>
        /// <param name="alternate">The UTF-8 bytes.</param>
        /// <param name="other">The stored key.</param>
        /// <returns>True when the bytes are that key's UTF-8 form.</returns>
        public bool Equals(ReadOnlySpan<byte> alternate, string other)
        {
            Span<char> chars = stackalloc char[other.Length];

            return Encoding.UTF8.TryGetChars(alternate, chars, out var written)
                && written == other.Length
                && chars[..written].SequenceEqual(other);
        }

        /// <summary>Hashes UTF-8 bytes to the same value their string form hashes to.</summary>
        /// <param name="alternate">The UTF-8 bytes.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(ReadOnlySpan<byte> alternate)
        {
            // the values this cache holds are short names; a stack buffer covers them, and anything longer
            // falls back rather than growing the stack by however much a sender decided to send
            if (alternate.Length <= 256)
            {
                Span<char> chars = stackalloc char[256];
                if (Encoding.UTF8.TryGetChars(alternate, chars, out var written))
                {
                    return string.GetHashCode(chars[..written], StringComparison.Ordinal);
                }
            }

            return Encoding.UTF8.GetString(alternate).GetHashCode(StringComparison.Ordinal);
        }

        /// <summary>Builds the key to store when a value is added through the alternate lookup.</summary>
        /// <param name="alternate">The UTF-8 bytes.</param>
        /// <returns>The string form.</returns>
        public string Create(ReadOnlySpan<byte> alternate) => Encoding.UTF8.GetString(alternate);
    }
}
