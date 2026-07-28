using System;
using Annium.Blazor.Routing.Internal;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Tests for the internal RouteManager route-resolution logic (first-match-wins and no-match → null), exercised
/// directly through the internal IRouteFactory/IRouteMatcher seam (InternalsVisibleTo) — the Router component that
/// normally drives it needs a render host and is out of scope for unit tests.
/// </summary>
public class RouteManagerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the RouteManagerTest class.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public RouteManagerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that when several routes match the same location, Match returns the FIRST tracked one (registration
    /// order wins).
    /// </summary>
    [Fact]
    public void Match_ReturnsFirstTrackedRoute()
    {
        // arrange: two routes with the same template but distinct page types, tracked in order
        var factory = Get<IRouteFactory>();
        factory.Create<PageA>("/x/y");
        factory.Create<PageB>("/x/y");
        var matcher = Get<IRouteMatcher>();

        // act
        var data = matcher.Match(RawLocation.Parse("x/y"), PathMatch.Exact);

        // assert: the first-registered route wins
        data.NotNull().PageType.Is(typeof(PageA));
    }

    /// <summary>
    /// Tests that Match returns null when no registered route matches the location.
    /// </summary>
    [Fact]
    public void Match_ReturnsNullWhenNoRouteMatches()
    {
        // arrange
        var factory = Get<IRouteFactory>();
        factory.Create<PageA>("/x/y");
        var matcher = Get<IRouteMatcher>();

        // act + assert
        matcher.Match(RawLocation.Parse("no/match"), PathMatch.Exact).IsDefault();
    }

    /// <summary>
    /// Tests that Match keeps scanning past a non-matching route to find a later matching one (pins the loop's
    /// skip-and-continue behavior, distinct from only ever checking the first tracked route).
    /// </summary>
    [Fact]
    public void Match_SkipsNonMatchingRouteToFindLater()
    {
        // arrange: first route does NOT match the target, second one does
        var factory = Get<IRouteFactory>();
        factory.Create<PageA>("/a/b");
        factory.Create<PageB>("/x/y");
        var matcher = Get<IRouteMatcher>();

        // act
        var data = matcher.Match(RawLocation.Parse("x/y"), PathMatch.Exact);

        // assert: the later matching route is returned, not null and not the first
        data.NotNull().PageType.Is(typeof(PageB));
    }

    /// <summary>
    /// Tests that creating a route with a malformed template throws: an empty/whitespace segment raises
    /// ArgumentException, and a null template raises ArgumentNullException (pins Helper's template validation).
    /// </summary>
    [Fact]
    public void Create_InvalidTemplate_Throws()
    {
        // arrange
        var factory = Get<IRouteFactory>();

        // assert
        Wrap.It(() => factory.Create<PageA>("/statics/ /about")).Throws<ArgumentException>();
        Wrap.It(() => factory.Create<PageA>(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that a template segment which merely CONTAINS a space (not a whitespace-only segment) is rejected —
    /// pins the `Contains(' ')` disjunct distinct from the `IsNullOrWhiteSpace` check.
    /// </summary>
    [Fact]
    public void Create_TemplateWithEmbeddedSpaceSegment_Throws()
    {
        var factory = Get<IRouteFactory>();
        Wrap.It(() => factory.Create<PageA>("/statics/foo bar/about")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a template with a `{param}` placeholder that maps to no property is rejected (pins
    /// LocationPath's unknown-parameter guard) — a non-generic route has no properties, so any placeholder is
    /// unknown.
    /// </summary>
    [Fact]
    public void Create_TemplateWithUnknownParam_Throws()
    {
        var factory = Get<IRouteFactory>();
        Wrap.It(() => factory.Create<PageA>("/x/{y}")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a root template ("/") is valid and matches the empty path (zero segments).
    /// </summary>
    [Fact]
    public void Create_RootTemplate_MatchesEmptyPath()
    {
        var factory = Get<IRouteFactory>();
        var route = factory.Create<PageA>("/");

        route.Link().Is(string.Empty);

        NavigationManager.NavigateTo(string.Empty);
        route.IsAt().IsTrue();
    }

    /// <summary>
    /// Tests that a bare empty-string template (distinct from "/") is also a valid root route — the
    /// whitespace-template guard specifically allows the empty string (its `Length &gt; 0` conjunct is false).
    /// </summary>
    [Fact]
    public void Create_EmptyTemplate_IsRoot()
    {
        var factory = Get<IRouteFactory>();
        var route = factory.Create<PageA>(string.Empty);

        route.Link().Is(string.Empty);

        NavigationManager.NavigateTo(string.Empty);
        route.IsAt().IsTrue();
    }

    /// <summary>
    /// First test page component.
    /// </summary>
    private class PageA { }

    /// <summary>
    /// Second test page component.
    /// </summary>
    private class PageB { }
}
