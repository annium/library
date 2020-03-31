using XRest.Core.Models;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Components
{
    internal interface IProcessor
    {
        ApiView Process(ApiModel api);
    }
}