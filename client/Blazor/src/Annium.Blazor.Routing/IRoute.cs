namespace Annium.Blazor.Routing
{
    public interface IRoute
    {
        string Link();
        void Go();
        bool IsAt(PathMatch match = PathMatch.Exact);
    }
}
