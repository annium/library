using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

public class RouteTest : TestBase
{
    [Fact]
    public void Link_Works()
    {
        // arrange
        var route = GetRouting<Routing>().About;

        // assert
        route.Link().IsEqual("statics/about");
    }

    [Fact]
    public void Go_Works()
    {
        // arrange
        var route = GetRouting<Routing>().About;

        // act
        route.Go();

        // assert
        NavigationManager.Locations.At(0).IsEqual("statics/about");
    }

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

    public class Routing : IRouting
    {
        public IRoute About { get; }

        public Routing(IRouteFactory routeFactory)
        {
            About = routeFactory.Create<StaticPage>("/statics/about");
        }
    }

    public class StaticPage
    {
    }
}