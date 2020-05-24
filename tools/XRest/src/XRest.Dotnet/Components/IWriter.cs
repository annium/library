using XRest.Dotnet.Views;

namespace XRest.Dotnet.Components
{
    internal interface IWriter
    {
        void Write(string output, ClientContainerView client, bool generateTestClient);
    }
}