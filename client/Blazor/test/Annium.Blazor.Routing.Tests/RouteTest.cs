using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Contains tests for basic route functionality without parameters
/// </summary>
public class RouteTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the RouteTest class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public RouteTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that route Link method generates correct URL
    /// </summary>
    [Fact]
    public void Link_Works()
    {
        // arrange
        var route = GetRouting<Routing>().About;

        // assert
        route.Link().Is("statics/about");
    }

    /// <summary>
    /// Tests that route Go method navigates to correct URL
    /// </summary>
    [Fact]
    public void Go_Works()
    {
        // arrange
        var route = GetRouting<Routing>().About;

        // act
        route.Go();

        // assert
        NavigationManager.Locations.At(0).Is("statics/about");
    }

    /// <summary>
    /// Tests that route IsAt method correctly identifies current location
    /// </summary>
    [Fact]
    public void IsAt_Works()
    {
        // arrange
        var route = GetRouting<Routing>().About;

        // assert
        NavigationManager.NavigateTo("statics/about");
        route.IsAt().IsTrue();
        NavigationManager.NavigateTo("statics");
        route.IsAt().IsFalse();
        route.IsAt(PathMatch.Start).IsTrue();
    }

    /// <summary>
    /// Test routing configuration with static routes
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route for the About page
        /// </summary>
        public IRoute About { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes</param>
        public Routing(IRouteFactory routeFactory)
        {
            About = routeFactory.Create<StaticPage>("/statics/about");
        }
    }

    /// <summary>
    /// Test page component for static routing
    /// </summary>
    public class StaticPage { }
}
