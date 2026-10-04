using System.Text.Json;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests that the value-type converters serve the nullable form of their type too, through the serializer's own
/// nullable handling: a converter that claimed <c>T?</c> itself was refused by <see cref="JsonSerializer"/>, and
/// with it every type that has a <c>T?</c> member anywhere in it.
/// </summary>
public class NullableConversionTest
{
    /// <summary>
    /// A type with a nullable NodaTime member, as configuration records have.
    /// </summary>
    /// <param name="At">The optional date.</param>
    /// <param name="Every">The optional duration.</param>
    private sealed record Holder(LocalDate? At, Duration? Every);

    /// <summary>
    /// Tests that a nullable local date with a value round-trips.
    /// </summary>
    [Fact]
    public void LocalDate_Nullable_WithValue_RoundTrips()
    {
        AssertConversions<LocalDate?>(new LocalDate(2012, 1, 2), "\"2012-01-02\"", Converters.LocalDateConverter);
    }

    /// <summary>
    /// Tests that a nullable local date without a value round-trips as null.
    /// </summary>
    [Fact]
    public void LocalDate_Nullable_Null_RoundTrips()
    {
        AssertConversions<LocalDate?>(null, "null", Converters.LocalDateConverter);
    }

    /// <summary>
    /// Tests that a nullable interval, from a converter that is not a pattern converter, round-trips.
    /// </summary>
    [Fact]
    public void Interval_Nullable_RoundTrips()
    {
        var interval = new Interval(Instant.FromUtc(2012, 1, 2, 3, 4, 5), Instant.FromUtc(2013, 6, 7, 8, 9, 10));

        AssertConversions<Interval?>(
            interval,
            "{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}",
            Converters.IntervalConverter,
            Converters.InstantConverter
        );
        AssertConversions<Interval?>(null, "null", Converters.IntervalConverter, Converters.InstantConverter);
    }

    /// <summary>
    /// Tests that a type with nullable members, set and unset, round-trips under ConfigureForNodaTime.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_TypeWithNullableMembers_RoundTrips()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var set = new Holder(new LocalDate(2012, 1, 2), Duration.FromHours(2));
        var unset = new Holder(null, null);

        JsonSerializer.Deserialize<Holder>(JsonSerializer.Serialize(set, options), options).Is(set);
        JsonSerializer.Deserialize<Holder>(JsonSerializer.Serialize(unset, options), options).Is(unset);
    }

    /// <summary>
    /// Tests that the converter no longer claims the nullable form, which the serializer wraps around it instead.
    /// </summary>
    [Fact]
    public void ValueTypeConverter_ClaimsOnlyItsOwnType()
    {
        Converters.LocalDateConverter.CanConvert(typeof(LocalDate)).IsTrue();
        Converters.LocalDateConverter.CanConvert(typeof(LocalDate?)).IsFalse();
    }
}
