using XRest.Clients.Dotnet.Views;
using XRest.Core.Models;

namespace XRest.Clients.Dotnet.Components
{
    internal interface IProcessor
    {
        ClientContainerView Process(Namespace ns, ApiModel api);
    }
}