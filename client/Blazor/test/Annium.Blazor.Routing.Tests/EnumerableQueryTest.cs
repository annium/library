using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Tests conversion handling for enumerable (array) query parameters — specifically that a malformed element in
/// an array-typed query value is swallowed (the whole parameter drops to its default) rather than propagating.
/// </summary>
public class EnumerableQueryTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the EnumerableQueryTest class.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public EnumerableQueryTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that when an array-typed query parameter has an element that fails conversion (a non-numeric entry
    /// for an int[] property), the exception is swallowed and the property keeps its default empty array (pins the
    /// enumerable branch of LocationQuery's try/catch).
    /// </summary>
    [Fact]
    public void GetParams_MalformedArrayElement_Ignored()
    {
        // arrange
        var route = GetRouting<Routing>().Search;

        // act: second element is not a number
        NavigationManager.NavigateTo("search?tags=1&tags=notanumber");

        // assert: the whole array param is dropped to its default, no exception surfaces
        route.GetParams().Tags.IsEmpty();
    }

    /// <summary>
    /// Test routing configuration with an int[] query parameter.
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route carrying an int-array query parameter.
        /// </summary>
        public IRoute<TagData> Search { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class.
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes.</param>
        public Routing(IRouteFactory routeFactory)
        {
            Search = routeFactory.Create<SearchPage, TagData>("/search");
        }
    }

    /// <summary>
    /// Test page component for the search route.
    /// </summary>
    public class SearchPage { }

    /// <summary>
    /// Test data model with an int-array query parameter.
    /// </summary>
    public sealed record TagData
    {
        /// <summary>
        /// Gets the tag ids (array query parameter).
        /// </summary>
        public int[] Tags { get; init; } = [];
    }
}
