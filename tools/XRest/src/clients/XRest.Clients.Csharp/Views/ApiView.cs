using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Views;

internal sealed record ApiView(
    Namespace Namespace, 
    IClientView Client,
    IReadOnlyCollection<IModelView> Models
);