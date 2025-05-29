using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.XRest.Clients.Csharp.Views.Client;
using Annium.XRest.Clients.Csharp.Views.Models;

namespace Annium.XRest.Clients.Csharp.Views.Api;

internal sealed record ApiView(Namespace Namespace, IClientView Client, IReadOnlyCollection<IModelView> Models);
