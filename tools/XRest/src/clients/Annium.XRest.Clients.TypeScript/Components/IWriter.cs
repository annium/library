using Annium.XRest.Clients.TypeScript.Views;

namespace Annium.XRest.Clients.TypeScript.Components;

internal interface IWriter
{
    void Write(string output, ApiView api);
}
