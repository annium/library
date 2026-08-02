using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Testing;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Clients.Csharp.Views.Client;
using Xunit;

namespace Annium.XRest.Clients.Csharp.Tests.Components.Processors;

public class ClientBuilderTests
{
    [Fact]
    public void BuildClient_NestedNamespace_NamesContainerApartFromItsNamespace()
    {
        // arrange — regression: the container for "…Clients.Admin" was typed `Admin`, so referencing
        // it from the parent resolved to the namespace and the generated client failed to compile
        // with CS0118: 'Admin' is a namespace but is used like a type
        var clients = "Demo.Clients".ToNamespace();
        var controllers = new[]
        {
            Controller(clients, "Index"),
            Controller("Demo.Clients.Admin".ToNamespace(), "Users"),
        };

        // act
        var root = (ClientContainerView)ClientBuilder.BuildClient(clients, "Root", "Root", controllers);

        // assert
        var admin = (ClientContainerView)root.Clients.Single(x => x.ToString() == "AdminRoot");
        admin.Namespace.Is("Demo.Clients.Admin");
        admin.Type.Is("AdminRoot");
        admin.Type.IsNotEqual("Admin");
    }

    [Fact]
    public void BuildClient_SingleController_BuildsFlatContainer()
    {
        // arrange
        var clients = "Demo.Clients".ToNamespace();

        // act
        var root = (ClientContainerView)
            ClientBuilder.BuildClient(clients, "Root", "Root", [Controller(clients, "Index")]);

        // assert
        root.Type.Is("Root");
        root.Clients.Has(1).At(0).ToString().Is("Index");
    }

    [Fact]
    public void BuildClient_NoControllers_Throws()
    {
        // arrange
        var clients = "Demo.Clients".ToNamespace();

        // act
        var build = Wrap.It(() => ClientBuilder.BuildClient(clients, "Root", "Root", []));

        // assert
        build.Throws<System.ArgumentException>();
    }

    private static ControllerView Controller(Annium.Net.Types.Models.Namespace @namespace, string name) =>
        new([], @namespace, name, []);
}
