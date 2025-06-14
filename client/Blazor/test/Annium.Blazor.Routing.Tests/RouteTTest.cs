using System;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Contains tests for parameterized route functionality
/// </summary>
public class RouteTTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the RouteTTest class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public RouteTTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that parameterized route Link method generates correct URL with parameters
    /// </summary>
    [Fact]
    public void Link_Works()
    {
        // arrange
        var route = GetRouting<Routing>().Search;

        // assert
        route
            .Link(
                new SearchData
                {
                    Sex = Sex.Male,
                    Name = ["alex", "anna"],
                    Age = 30,
                }
            )
            .Is("pages/search/Male?name=alex&name=anna&age=30");
    }

    /// <summary>
    /// Tests that parameterized route Go method navigates to correct URL with parameters
    /// </summary>
    [Fact]
    public void Go_Works()
    {
        // arrange
        var route = GetRouting<Routing>().Search;

        // act
        route.Go(
            new SearchData
            {
                Sex = Sex.Male,
                Name = ["alex", "anna"],
                Age = 30,
            }
        );

        // assert
        NavigationManager.Locations.At(0).Is("pages/search/Male?name=alex&name=anna&age=30");
    }

    /// <summary>
    /// Tests that parameterized route IsAt method correctly identifies current location with parameters
    /// </summary>
    [Fact]
    public void IsAt_Works()
    {
        // arrange
        var route = GetRouting<Routing>().Search;

        // assert
        NavigationManager.NavigateTo("pages/search/Male?name=alex&name=anna&age=30");
        route.IsAt().IsTrue();
        route.IsAt(new SearchData { Sex = Sex.Male }).IsTrue();
        route.IsAt(new SearchData { Sex = Sex.Female }).IsFalse();
        route
            .IsAt(
                new SearchData
                {
                    Sex = Sex.Male,
                    Name = ["alex", "anna"],
                    Age = 30,
                }
            )
            .IsTrue();
        route.IsAt(new SearchData { Sex = Sex.Male, Name = ["alex", "ann"] }).IsFalse();
        NavigationManager.NavigateTo("pages");
        route.IsAt().IsFalse();
        route.IsAt(null, PathMatch.Start).IsTrue();
    }

    /// <summary>
    /// Tests that parameterized route GetParams method correctly extracts parameters from current URL
    /// </summary>
    [Fact]
    public void GetParams_Works()
    {
        // arrange
        var route = GetRouting<Routing>().Search;

        // assert
        NavigationManager.NavigateTo("pages/search/Male?name=alex&name=anna&age=30");
        route
            .GetParams()
            .IsEqual(
                new SearchData
                {
                    Sex = Sex.Male,
                    Name = ["alex", "anna"],
                    Age = 30,
                }
            );
        NavigationManager.NavigateTo("profile/Male?name=alex&name=anna&age=30");
        route.TryGetParams(out _).IsFalse();
    }

    /// <summary>
    /// Test routing configuration with parameterized routes
    /// </summary>
    public class Routing : IRouting
    {
        /// <summary>
        /// Route for the Search page with parameters
        /// </summary>
        public IRoute<SearchData> Search { get; }

        /// <summary>
        /// Initializes a new instance of the Routing class
        /// </summary>
        /// <param name="routeFactory">Factory for creating routes</param>
        public Routing(IRouteFactory routeFactory)
        {
            Search = routeFactory.Create<SearchPage, SearchData>("/pages/search/{sex}");
        }
    }

    /// <summary>
    /// Test page component for parameterized routing
    /// </summary>
    public class SearchPage { }

    /// <summary>
    /// Test data model for search parameters
    /// </summary>
    public sealed record SearchData
    {
        /// <summary>
        /// Gets or sets the sex filter parameter
        /// </summary>
        public Sex Sex { get; init; }

        /// <summary>
        /// Gets or sets the name filter parameters
        /// </summary>
        public string[] Name { get; init; } = [];

        /// <summary>
        /// Gets or sets the age filter parameter
        /// </summary>
        public int Age { get; init; }

        /// <summary>
        /// Determines whether the specified SearchData is equal to the current SearchData
        /// </summary>
        /// <param name="other">The SearchData to compare with the current SearchData</param>
        /// <returns>true if the specified SearchData is equal to the current SearchData; otherwise, false</returns>
        public bool Equals(SearchData? other) => other is not null && GetHashCode() == other.GetHashCode();

        /// <summary>
        /// Returns a hash code for this instance
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table</returns>
        public override int GetHashCode() => HashCode.Combine((int)Sex, HashCodeSeq.Combine(Name), Age);
    }

    /// <summary>
    /// Enumeration for sex values used in search parameters
    /// </summary>
    [AutoMapped]
    public enum Sex
    {
        /// <summary>
        /// Male sex value
        /// </summary>
        Male,

        /// <summary>
        /// Female sex value
        /// </summary>
        Female,
    }
}
