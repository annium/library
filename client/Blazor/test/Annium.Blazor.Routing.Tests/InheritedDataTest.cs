using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Tests that a route data type may inherit route parameters from a base type — properties resolved with a base
/// <c>DeclaringType</c> must be accepted by the data model, not rejected.
/// </summary>
public class InheritedDataTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the InheritedDataTest class.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public InheritedDataTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a route whose data type inherits its path parameter from a base record builds and resolves
    /// correctly (the inherited property is accepted, links generate, and params round-trip).
    /// </summary>
    [Fact]
    public void Route_WithInheritedParam_LinksAndRoundTrips()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{id}", Id inherited from BaseData

        // act
        var link = route.Link(new ItemData { Id = 7, Name = "abc" });

        // assert: inherited path param + own query param both render
        link.Is("items/7?name=abc");

        // round-trip
        NavigationManager.NavigateTo(link);
        var p = route.GetParams();
        p.Id.Is(7);
        p.Name.Is("abc");
    }

    /// <summary>
    /// Tests that an empty-STRING query parameter is still emitted (<c>name=</c>) — distinct from an empty ARRAY,
    /// which is omitted. Pins the `not string` exclusion in the empty-enumerable skip (a string is technically an
    /// IEnumerable&lt;char&gt;, but an empty string is a real scalar value, not an omittable default).
    /// </summary>
    [Fact]
    public void Link_EmptyStringQueryParam_StillEmitted()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{id}", Name is a string query param

        // act: Name left at its "" default
        var link = route.Link(new ItemData { Id = 7 });

        // assert: the empty string is emitted as an empty query value, not dropped
        link.Is("items/7?name=");
    }

    /// <summary>
    /// Tests that a path parameter whose raw value fails type conversion (a non-numeric id) yields no match —
    /// the conversion exception is swallowed to a non-match rather than propagated (pins ParamLocationSegment's
    /// generic catch → null path).
    /// </summary>
    [Fact]
    public void IsAt_UnconvertiblePathParam_DoesNotMatch()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{id}", Id is int

        // act
        NavigationManager.NavigateTo("items/notanumber");

        // assert: the FormatException on int.Parse becomes a non-match, not an exception
        route.IsAt().IsFalse();
        route.TryGetParams(out _).IsFalse();
    }

    /// <summary>
    /// Test routing configuration whose data type inherits a parameter from a base type.
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route keyed by an inherited id path parameter.
        /// </summary>
        public IRoute<ItemData> Item { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class.
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes.</param>
        public Routing(IRouteFactory routeFactory)
        {
            Item = routeFactory.Create<ItemPage, ItemData>("/items/{id}");
        }
    }

    /// <summary>
    /// Test page component for the item route.
    /// </summary>
    public class ItemPage { }

    /// <summary>
    /// Base data model contributing an inherited id parameter.
    /// </summary>
    public abstract record BaseData
    {
        /// <summary>
        /// Gets the item id (inherited path parameter).
        /// </summary>
        public int Id { get; init; }
    }

    /// <summary>
    /// Derived data model adding its own query parameter.
    /// </summary>
    public sealed record ItemData : BaseData
    {
        /// <summary>
        /// Gets the item name (own query parameter).
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
