using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Views;
using XRest.Core.Models;

namespace XRest.Clients.Csharp.Components;

internal interface IProcessor
{
    ClientContainerView Process(Namespace ns, ApiModel api);
}