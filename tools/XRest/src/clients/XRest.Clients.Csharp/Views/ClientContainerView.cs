using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal class ClientContainerView : IClientView
{
    public IReadOnlyCollection<string> Usages { get; }
    public string Namespace { get; }
    public string Name { get; }
    public string Type { get; }
    public IReadOnlyCollection<IClientView> Clients { get; }

    public ClientContainerView(
        IReadOnlyCollection<string> usages,
        string @namespace,
        string name,
        string type,
        IReadOnlyCollection<IClientView> clients
    )
    {
        Usages = usages;
        Namespace = @namespace;
        Name = name;
        Type = type;
        Clients = clients;
    }

    public override string ToString() => Name;
}