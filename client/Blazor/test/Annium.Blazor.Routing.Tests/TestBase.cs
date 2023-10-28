using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Xunit.Abstractions;

namespace Annium.Blazor.Routing.Tests;

public abstract class TestBase : Testing.TestBase
{
    protected FakeNavigationManager NavigationManager { get; }

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

    public T GetRouting<T>()
        where T : IRouting
    {
        return Get<T>();
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
