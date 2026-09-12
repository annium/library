using System;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for the Helper class message template processing
/// </summary>
public class HelperTest
{
    /// <summary>
    /// Tests that normal message template processing works correctly
    /// </summary>
    [Fact]
    public void Normal_Works()
    {
        // arrange
        var user = "alex";
        var time = "01.01.2021 08:00";

        // act
        var (message, data) = Helper.Process("Log-in by {user} at {time}", new object[] { user, time });

        // assert
        message.Is("Log-in by alex at 01.01.2021 08:00");
        data.Has(2);
        data.At("user").As<string>().Is(user);
        data.At("time").As<string>().Is(time);
    }

    /// <summary>
    /// Tests that corner case message template processing works correctly
    /// </summary>
    [Fact]
    public void Corner_Works()
    {
        // arrange
        var user = "alex";
        var time = "01.01.2021 08:00";

        // act
        var (message, data) = Helper.Process("{user}{time}", new object[] { user, time });

        // assert
        message.Is("alex01.01.2021 08:00");
        data.Has(2);
        data.At("user").As<string>().Is(user);
        data.At("time").As<string>().Is(time);
    }

    /// <summary>
    /// Tests that nested placeholders without data are ignored correctly
    /// </summary>
    [Fact]
    public void Nested_NoData_IgnoredCorrectly()
    {
        // act
        var (message, data) = Helper.Process("some {{nested} data} here", Array.Empty<object>());

        // assert
        message.Is("some {{nested} data} here");
        data.IsEmpty();
    }

    /// <summary>
    /// Tests that nested placeholders with data work correctly
    /// </summary>
    [Fact]
    public void Nested_WithData_Works()
    {
        // act
        var (message, data) = Helper.Process("some {{nested} data} here", new object[] { "demo" });

        // assert
        message.Is("some demo here");
        data.Has(1);
        data.At("{nested} data").As<string>().Is("demo");
    }

    /// <summary>
    /// When more data items are supplied than the template has placeholders,
    /// <see cref="Helper.Process"/> must throw <see cref="InvalidOperationException"/>.
    /// The guard is on line ~67-69 of Helper.cs: <c>if (extra > 0) throw ...</c>.
    /// This test fails if that guard is removed or weakened.
    /// </summary>
    [Fact]
    public void ExtraDataItems_ThrowsInvalidOperationException()
    {
        Wrap.It(() => Helper.Process("hello {name}", new object?[] { "alex", "extra" }))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// When data items exactly match the template placeholders,
    /// <see cref="Helper.Process"/> must succeed and substitute the value.
    /// Verifies the exact-match path does not throw and produces the expected message and data.
    /// </summary>
    [Fact]
    public void ExactDataItems_DoesNotThrow_ProducesExpected()
    {
        var (message, data) = Helper.Process("hello {name}", new object?[] { "alex" });

        message.Is("hello alex");
        data.Has(1);
        data.At("name").As<string>().Is("alex");
    }

    /// <summary>
    /// The template parse is cached per template instance, but the render is not: the same template with
    /// different values must produce different messages. Fails if the cache ever holds the rendered
    /// message rather than the parse.
    /// </summary>
    [Fact]
    public void SameTemplate_DifferentValues_RendersEach()
    {
        const string template = "order {id} at {price}";

        var (first, firstData) = Helper.Process(template, new object?[] { "A-1", 10 });
        var (second, secondData) = Helper.Process(template, new object?[] { "B-2", 20 });

        first.Is("order A-1 at 10");
        second.Is("order B-2 at 20");
        firstData.At("id").As<string>().Is("A-1");
        secondData.At("id").As<string>().Is("B-2");
    }

    /// <summary>
    /// A name repeated in the template keeps its last value in the data, while every occurrence renders
    /// its own value. Fails if the deferred data build overwrites in the wrong order or collapses the
    /// render.
    /// </summary>
    [Fact]
    public void DuplicateName_RendersBoth_KeepsLastInData()
    {
        var (message, data) = Helper.Process("{x} and {x}", new object?[] { "a", "b" });

        message.Is("a and b");
        data.Has(1);
        data.At("x").As<string>().Is("b");
    }

    /// <summary>
    /// Fewer values than placeholders: the unbound placeholder re-emits itself in braces and is left out
    /// of the data, and this is not an error — only surplus values are.
    /// </summary>
    [Fact]
    public void FewerDataItems_ReEmitsPlaceholder_AndOmitsItFromData()
    {
        var (message, data) = Helper.Process("hello {a} and {b}", new object?[] { "x" });

        message.Is("hello x and {b}");
        data.Has(1);
        data.At("a").As<string>().Is("x");
        data.ContainsKey("b").IsFalse();
    }

    /// <summary>
    /// A null value renders as nothing — the behaviour of appending a null object to a string builder —
    /// while the name is still bound, to null, in the data.
    /// </summary>
    [Fact]
    public void NullDataItem_RendersAsNothing_ButIsBound()
    {
        var (message, data) = Helper.Process("value: {v}!", new object?[] { null });

        message.Is("value: !");
        data.Has(1);
        data.At("v").IsDefault();
    }

    /// <summary>
    /// With no values to bind there is nothing to render, so the template comes back verbatim — braces
    /// and all. This is a deliberate change from the character-by-character scan, which dropped a
    /// <c>}</c> that closed nothing and swallowed the tail after a <c>{</c> that never closed. At a real
    /// call site the <c>LOG0003</c> analyzer rejects both shapes; the text that reaches here in this
    /// shape comes from a bridge that already rendered its own message, and for that, verbatim is the
    /// only defensible answer.
    /// </summary>
    /// <param name="template">A template whose braces do not balance.</param>
    [Theory]
    [InlineData("a {b")]
    [InlineData("a}b")]
    [InlineData("{unclosed")]
    [InlineData("plain text with no braces")]
    [InlineData("{\"json\": \"looking\"}")]
    public void NoDataItems_ReturnsTemplateVerbatim(string template)
    {
        var (message, data) = Helper.Process(template, Array.Empty<object>());

        message.Is(template);
        data.IsEmpty();
    }
}
