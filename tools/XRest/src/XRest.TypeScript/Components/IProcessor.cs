using XRest.Core.Models;
using XRest.TypeScript.Views;

namespace XRest.TypeScript.Components
{
    internal interface IProcessor
    {
        ApiView Process(ApiModel api);
    }
}