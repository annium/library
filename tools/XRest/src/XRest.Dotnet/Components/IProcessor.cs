using XRest.Core.Models;
using XRest.Dotnet.Views;

namespace XRest.Dotnet.Components
{
    internal interface IProcessor
    {
        ClientContainerView Process(string ns, ApiModel api);
    }
}