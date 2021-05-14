namespace Annium.Blazor.Routing
{
    public interface IRouteFactory
    {
        IRoute<TData> Create<TPage, TData>(string template, TData data = default) where TPage : notnull where TData : notnull, new();
        IRoute Create<TPage>(string template) where TPage : notnull;
    }
}