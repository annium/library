using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal interface IClientView
{
    IReadOnlyCollection<string> Usages { get; }
    string Namespace { get; }
    string Name { get; }
    string Type { get; }
}