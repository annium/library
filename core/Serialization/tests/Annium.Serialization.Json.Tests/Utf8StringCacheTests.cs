using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Pins <see cref="Utf8StringCache"/>: that it answers with the same instance for the same bytes, that the
/// answer is always the right text, and that it stops growing.
/// </summary>
/// <remarks>
/// The reason to reuse an instance is allocation, and the reason that is safe is that strings are
/// immutable - so the test that matters is reference equality on a repeat, and text equality always.
/// </remarks>
public class Utf8StringCacheTests
{
    /// <summary>
    /// The same bytes twice give back the same instance, which is the entire point.
    /// </summary>
    [Fact]
    public void SameBytes_GiveTheSameInstance()
    {
        var cache = new Utf8StringCache();

        var first = Read(cache, "btcusdt@bookTicker");
        var second = Read(cache, "btcusdt@bookTicker");

        first.Is("btcusdt@bookTicker");
        ReferenceEquals(first, second).IsTrue("a repeated value allocated a second string");
    }

    /// <summary>
    /// Different bytes give different values — a cache that collided would corrupt every message it touched.
    /// </summary>
    /// <param name="value">The value to round-trip.</param>
    [Theory]
    [InlineData("BTCUSDT")]
    [InlineData("ETHUSDT")]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("btcusdt@bookTicker")]
    [InlineData("привет")]
    [InlineData("a b/c=d")]
    public void AnyValue_ReadsBackAsItself(string value)
    {
        var cache = new Utf8StringCache();

        Read(cache, value).Is(value);
        Read(cache, value).Is(value);
    }

    /// <summary>
    /// Values that differ only past the first bytes are not confused with each other.
    /// </summary>
    [Fact]
    public void ValuesSharingAPrefix_StayDistinct()
    {
        var cache = new Utf8StringCache();

        Read(cache, "BTCUSDT").Is("BTCUSDT");
        Read(cache, "BTCUSDC").Is("BTCUSDC");
        Read(cache, "BTCUSD").Is("BTCUSD");
        Read(cache, "BTCUSDT").Is("BTCUSDT");
    }

    /// <summary>
    /// An escaped string is still read correctly, through the reader rather than from the raw bytes.
    /// </summary>
    /// <remarks>
    /// The raw span of an escaped token holds the escape sequences, so answering from it would hand back
    /// <c>\u0061bc</c> where the value is <c>abc</c>. The cache steps aside for these instead.
    /// </remarks>
    [Fact]
    public void EscapedValue_IsUnescaped()
    {
        var cache = new Utf8StringCache();
        var json = Encoding.UTF8.GetBytes("""{"v":"\u0061bc"}""");
        var reader = new Utf8JsonReader(json);

        // {, property name, value
        reader.Read();
        reader.Read();
        reader.Read();

        cache.GetString(ref reader).Is("abc");
    }

    /// <summary>
    /// Past its bound the cache stops storing, and keeps answering correctly.
    /// </summary>
    /// <remarks>
    /// A cache with no bound is a memory leak whenever the sender's set of values is larger than the one
    /// this was written for. What must not happen past the bound is a wrong answer.
    /// </remarks>
    [Fact]
    public void PastItsBound_ItStopsStoringButKeepsAnswering()
    {
        var cache = new Utf8StringCache();
        var seen = new List<string>();

        for (var i = 0; i < 1100; i++)
        {
            var value = $"symbol-{i}";
            Read(cache, value).Is(value);
            seen.Add(value);
        }

        // the ones stored early still come back as the same instance
        ReferenceEquals(Read(cache, seen[0]), Read(cache, seen[0])).IsTrue("an early value stopped being cached");

        // a value first seen past the bound is answered correctly and not stored - which is the bound
        // itself, and the only assertion here that fails if it is removed
        Read(cache, "late-comer").Is("late-comer");
        ReferenceEquals(Read(cache, "late-comer"), Read(cache, "late-comer"))
            .IsFalse("the cache kept growing past its bound");
    }

    /// <summary>
    /// Reads a value through the cache as a JSON string token.
    /// </summary>
    /// <param name="cache">The cache under test.</param>
    /// <param name="value">The value to encode and read back.</param>
    /// <returns>What the cache answered.</returns>
    private static string Read(Utf8StringCache cache, string value)
    {
        var json = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        var reader = new Utf8JsonReader(json);

        reader.Read();

        return cache.GetString(ref reader) ?? throw new InvalidOperationException("null for a string token");
    }
}
