using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal interface IModelView
{
    IReadOnlyList<string> Usages { get; }
    string Namespace { get; }
    string Name { get; }
}