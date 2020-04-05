using XRest.Core.Models;
using XRest.TypeScript.Views;

namespace XRest.TypeScript.Components
{
    internal interface IWriter
    {
        void Write(string output, ApiView api);
    }
}