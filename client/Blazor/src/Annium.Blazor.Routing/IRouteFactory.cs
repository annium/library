namespace Annium.Blazor.Routing;

public interface IRouteFactory
{
    IRoute<TData> Create<TPage, TData>(string template)
        where TPage : notnull where TData : notnull, new();

    IRoute Create<TPage>(string template)
        where TPage : notnull;
}