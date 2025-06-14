using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Annium.Blazor.Routing.Tests;

/// <summary>
/// Base class for routing tests providing common test infrastructure
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Gets the fake navigation manager for testing navigation operations
    /// </summary>
    protected FakeNavigationManager NavigationManager { get; }

    /// <summary>
    /// Initializes a new instance of the TestBase class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddMapper();
            container.AddRouting();
            container.Add<FakeNavigationManager>().AsSelf().As<NavigationManager>().Singleton();
        });

        NavigationManager = Get<FakeNavigationManager>();
    }

    /// <summary>
    /// Gets a routing instance of the specified type from the service container
    /// </summary>
    /// <typeparam name="T">The type of routing to retrieve</typeparam>
    /// <returns>An instance of the specified routing type</returns>
    public T GetRouting<T>()
        where T : IRouting
    {
        return Get<T>();
    }

    /// <summary>
    /// Fake navigation manager for testing navigation operations
    /// </summary>
    protected class FakeNavigationManager : NavigationManager
    {
        /// <summary>
        /// Gets the list of locations that have been navigated to
        /// </summary>
        public IReadOnlyList<string> Locations => _locations;

        /// <summary>
        /// Private list to store navigation locations
        /// </summary>
        private readonly List<string> _locations = new();

        /// <summary>
        /// Initializes a new instance of the FakeNavigationManager class
        /// </summary>
        public FakeNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        /// <summary>
        /// Core navigation method that records navigation attempts
        /// </summary>
        /// <param name="uri">The URI to navigate to</param>
        /// <param name="forceLoad">Whether to force a page load</param>
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            _locations.Add(uri);
            Uri = new Uri(new Uri(BaseUri), uri).ToString();
        }
    }
}
