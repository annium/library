using XRest.Clients.TypeScript.Views;
using XRest.Core.Models;

namespace XRest.Clients.TypeScript.Components
{
    internal interface IProcessor
    {
        ApiView Process(ApiModel api);
    }
}