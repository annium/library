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
    /// Tests that a URL with MORE segments than the template never matches — neither Exact nor Start
    /// (pins LocationPath's `segments.Count > _segments.Count` guard).
    /// </summary>
    [Fact]
    public void IsAt_UrlLongerThanTemplate_DoesNotMatch()
    {
        // arrange
        var route = GetRouting<Routing>().About; // "/statics/about"

        // act
        NavigationManager.NavigateTo("statics/about/extra");

        // assert
        route.IsAt().IsFalse();
        route.IsAt(PathMatch.Start).IsFalse();
    }

    /// <summary>
    /// Tests that a fixed segment matches by EXACT equality, not prefix — `statics-x` must not match the `statics`
    /// segment even under PathMatch.Start (pins FixedLocationSegment's `Part == segment`).
    /// </summary>
    [Fact]
    public void IsAt_PartialFixedSegment_DoesNotMatch()
    {
        // arrange
        var route = GetRouting<Routing>().About; // "/statics/about"

        // act
        NavigationManager.NavigateTo("statics-x/about");

        // assert
        route.IsAt().IsFalse();
        route.IsAt(PathMatch.Start).IsFalse();
    }

    /// <summary>
    /// Tests that a trailing slash on the current URL is tolerated (empty segments are ignored on live URLs) and
    /// still matches the route — a bare `statics/about/` must not crash routing.
    /// </summary>
    [Fact]
    public void IsAt_TrailingSlash_Matches()
    {
        // arrange
        var route = GetRouting<Routing>().About; // "/statics/about"

        // act
        NavigationManager.NavigateTo("statics/about/");

        // assert
        route.IsAt().IsTrue();
    }

    /// <summary>
    /// Tests that a URL fragment (#...) is stripped before matching — an on-page anchor must not fold into the
    /// last path segment and break the route match.
    /// </summary>
    [Fact]
    public void IsAt_UrlWithFragment_IgnoresFragment()
    {
        // arrange
        var route = GetRouting<Routing>().About; // "/statics/about"

        // act
        NavigationManager.NavigateTo("statics/about#section");

        // assert
        route.IsAt().IsTrue();
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
