using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Tests that string path parameters are percent-encoded on Link and decoded on match, so values containing URL
/// separators or spaces round-trip correctly (Link → GetParams).
/// </summary>
public class PathEncodingTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the PathEncodingTest class.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public PathEncodingTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a path parameter containing a '/' and a space is percent-encoded in the generated link and
    /// decoded back to its original value when the route params are extracted.
    /// </summary>
    [Fact]
    public void Link_And_GetParams_RoundTripSpecialCharsInPathParam()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{name}"
        var data = new ItemData { Name = "john/doe x" };

        // act
        var link = route.Link(data);

        // assert: special chars are percent-encoded in the path segment (no raw '/' leaking a new segment)
        link.Contains("john%2Fdoe").IsTrue();
        link.Contains("john/doe").IsFalse();

        // round-trip: navigating that link and reading params restores the original value
        NavigationManager.NavigateTo(link);
        route.GetParams().Name.Is("john/doe x");
    }

    /// <summary>
    /// Tests that Bind substitutes the data into the template, producing a concrete (parameter-free) route whose
    /// Link is the resolved path and which matches at that location.
    /// </summary>
    [Fact]
    public void Bind_ProducesConcreteRoute()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{name}"

        // act
        var bound = route.Bind(new ItemData { Name = "abc" });

        // assert: the bound route's link is the substituted path
        bound.Link().Is("items/abc");

        // and it resolves at that location
        NavigationManager.NavigateTo(bound.Link());
        bound.IsAt().IsTrue();
    }

    /// <summary>
    /// Test routing configuration with a string path parameter.
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route for an item page keyed by a free-form name path parameter.
        /// </summary>
        public IRoute<ItemData> Item { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class.
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes.</param>
        public Routing(IRouteFactory routeFactory)
        {
            Item = routeFactory.Create<ItemPage, ItemData>("/items/{name}");
        }
    }

    /// <summary>
    /// Test page component for the item route.
    /// </summary>
    public class ItemPage { }

    /// <summary>
    /// Test data model carrying a single free-form name path parameter.
    /// </summary>
    public sealed record ItemData
    {
        /// <summary>
        /// Gets the item name (path parameter).
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
