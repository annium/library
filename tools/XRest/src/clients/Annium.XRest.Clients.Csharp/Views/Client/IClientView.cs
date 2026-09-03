using System.Collections.Generic;

namespace Annium.XRest.Clients.Csharp.Views.Client;

internal interface IClientView
{
    IReadOnlyCollection<string> Usages { get; }
    string Namespace { get; }
    string Name { get; }
    string Type { get; }
}
