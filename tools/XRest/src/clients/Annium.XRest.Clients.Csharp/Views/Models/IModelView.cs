using System.Collections.Generic;

namespace Annium.XRest.Clients.Csharp.Views.Models;

internal interface IModelView
{
    IReadOnlyList<string> Usages { get; }
    string Namespace { get; }
    string Name { get; }
}
