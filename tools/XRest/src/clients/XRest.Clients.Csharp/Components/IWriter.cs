using XRest.Clients.Csharp.ViewsSave;

namespace XRest.Clients.Csharp.Components;

internal interface IWriter
{
    void Write(string output, ClientContainerView client, bool generateTestClient);
}