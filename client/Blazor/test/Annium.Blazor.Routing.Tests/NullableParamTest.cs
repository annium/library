using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Tests that IsAt tolerates a nullable reference-type route-data property left at its null default — the
/// parameter comparison must skip a null value rather than dereference it.
/// </summary>
public class NullableParamTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the NullableParamTest class.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public NullableParamTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that comparing against data whose nullable reference-type property is null does not throw — the null
    /// property is skipped in the comparison (pins the null-guard in the non-default-parameter filter), while a
    /// differing path parameter still drives the match result.
    /// </summary>
    [Fact]
    public void IsAt_NullableRefParamLeftNull_DoesNotThrow()
    {
        // arrange
        var route = GetRouting<Routing>().Item; // "/items/{id}", Label is a nullable query param

        // act
        NavigationManager.NavigateTo("items/7");

        // assert: Label left null is skipped (no crash); the id drives the result
        route.IsAt(new Data { Id = 7, Label = null }).IsTrue();
        route.IsAt(new Data { Id = 9, Label = null }).IsFalse();
    }

    /// <summary>
    /// Test routing configuration whose data type has a nullable reference-type parameter.
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route keyed by an id path parameter with an optional label query parameter.
        /// </summary>
        public IRoute<Data> Item { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class.
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes.</param>
        public Routing(IRouteFactory routeFactory)
        {
            Item = routeFactory.Create<ItemPage, Data>("/items/{id}");
        }
    }

    /// <summary>
    /// Test page component for the item route.
    /// </summary>
    public class ItemPage { }

    /// <summary>
    /// Test data model with an id path parameter and a nullable label query parameter.
    /// </summary>
    public sealed record Data
    {
        /// <summary>
        /// Gets the item id (path parameter).
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the optional label (nullable query parameter).
        /// </summary>
        public string? Label { get; init; }
    }
}
