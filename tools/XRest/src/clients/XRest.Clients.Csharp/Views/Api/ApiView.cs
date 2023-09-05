using System.Collections.Generic;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views.Client;
using XRest.Clients.Csharp.Views.Models;

namespace XRest.Clients.Csharp.Views.Api;

internal sealed record ApiView(
    Namespace Namespace,
    IClientView Client,
    IReadOnlyCollection<IModelView> Models
);