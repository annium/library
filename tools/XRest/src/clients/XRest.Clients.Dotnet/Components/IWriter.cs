using XRest.Clients.Dotnet.Views;

namespace XRest.Clients.Dotnet.Components;

internal interface IWriter
{
    void Write(string output, ClientContainerView client, bool generateTestClient);
}