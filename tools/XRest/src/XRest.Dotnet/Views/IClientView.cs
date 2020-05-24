namespace XRest.Dotnet.Views
{
    internal interface IClientView
    {
        string Namespace { get; }
        string Name { get; }
        string Type { get; }
    }
}