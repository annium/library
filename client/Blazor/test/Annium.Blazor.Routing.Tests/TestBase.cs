using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Tests;

public abstract class TestBase
{
    protected FakeNavigationManager NavigationManager { get; }

    private readonly IServiceProvider _serviceProvider;

    protected TestBase()
    {
        var container = new ServiceContainer();
        container.AddRuntimeTools(GetType().Assembly, false);
        container.AddMapper();
        container.AddRouting();
        container.Add<FakeNavigationManager>().AsSelf().As<NavigationManager>().Singleton();
        _serviceProvider = container.BuildServiceProvider();
        NavigationManager = _serviceProvider.Resolve<FakeNavigationManager>();
    }

    public T GetRouting<T>()
        where T : IRouting
    {
        return _serviceProvider.Resolve<T>();
    }

    protected class FakeNavigationManager : NavigationManager
    {
        public IReadOnlyList<string> Locations => _locations;
        private readonly List<string> _locations = new();

        public FakeNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            _locations.Add(uri);
            Uri = new Uri(new Uri(BaseUri), uri).ToString();
        }
    }
}