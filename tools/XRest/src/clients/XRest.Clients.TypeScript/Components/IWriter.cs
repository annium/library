using XRest.Clients.TypeScript.Views;

namespace XRest.Clients.TypeScript.Components;

internal interface IWriter
{
    void Write(string output, ApiView api);
}
