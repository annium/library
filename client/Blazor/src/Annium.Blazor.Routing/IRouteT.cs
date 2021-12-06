namespace Annium.Blazor.Routing;

public interface IRoute<TData>
    where TData : notnull, new()
{
    string Link(TData data);
    void Go(TData data);
    bool IsAt(TData? data = default, PathMatch match = PathMatch.Exact);
    bool TryGetParams(out TData data);
    TData GetParams();
    IRoute Bind(TData data);
}