using Annium.Testing;
using Annium.XRest.Sources.AspNetCore.Internal.Helpers;
using Xunit;

namespace Annium.XRest.Sources.AspNetCore.Tests.Internal.Helpers;

public class RouteHelperTests
{
    [Theory]
    [InlineData("users/{id}", "users/{id}")]
    [InlineData("users/{id:int}", "users/{id}")]
    [InlineData("users/{id?}", "users/{id}")]
    [InlineData("users/{id:int:min(1)}", "users/{id}")]
    [InlineData("users/{user_id}", "users/{user_id}")]
    [InlineData("users/{id}/posts/{postId}", "users/{id}/posts/{postId}")]
    [InlineData("users", "users")]
    [InlineData("", "")]
    public void NormalizeRoute_Placeholders_AreStrippedToBareNames(string route, string expected)
    {
        // assert
        RouteHelper.NormalizeRoute(route, []).Is(expected);
    }

    [Theory]
    [InlineData("files/{*slug}", "files/{slug}")]
    [InlineData("files/{**slug}", "files/{slug}")]
    public void NormalizeRoute_CatchAll_IsRecognized(string route, string expected)
    {
        // assert — regression: the pattern required an alphanumeric right after `{`, so `{*slug}`
        // was left verbatim in the path and its parameter was classified as query
        RouteHelper.NormalizeRoute(route, []).Is(expected);
    }

    [Fact]
    public void NormalizeRoute_NameCasingDiffersFromParameter_UsesParameterCasing()
    {
        // arrange — regression: routes match case-insensitively, but the generated client
        // interpolates the path against its own parameters, which is case-sensitive
        // act
        var route = RouteHelper.NormalizeRoute("users/{Id}", ["id"]);

        // assert
        route.Is("users/{id}");
    }

    [Fact]
    public void NormalizeRoute_NoMatchingParameter_KeepsRouteSpelling()
    {
        // assert
        RouteHelper.NormalizeRoute("users/{Id}", ["other"]).Is("users/{Id}");
    }

    [Theory]
    [InlineData("users/{id}", new[] { "id" })]
    [InlineData("users/{id:int}/posts/{postId}", new[] { "id", "postId" })]
    [InlineData("files/{*slug}", new[] { "slug" })]
    [InlineData("files/{**slug}", new[] { "slug" })]
    [InlineData("users", new string[0])]
    public void ParseRouteParameters_Route_YieldsPlaceholderNames(string route, string[] expected)
    {
        // act
        var parameters = RouteHelper.ParseRouteParameters(route);

        // assert
        parameters.Has(expected.Length);
        for (var i = 0; i < expected.Length; i++)
            parameters.At(i).Is(expected[i]);
    }
}
