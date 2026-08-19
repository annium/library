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
    public void BuildClient_BranchesSharingALastSegment_GetDistinctContainerNames()
    {
        // arrange — regression: containers were named after their last namespace segment alone, so
        // Admin.Items and Public.Items both became `ItemsRoot`, and the parent declared the property
        // twice — CS0102, plus CS0104 on the ambiguous type reference
        var clients = "Demo.Clients".ToNamespace();
        var controllers = new[]
        {
            Controller("Demo.Clients.Admin.Items".ToNamespace(), "Items"),
            Controller("Demo.Clients.Public.Items".ToNamespace(), "Items"),
        };

        // act
        var root = (ClientContainerView)ClientBuilder.BuildClient(clients, "Root", "Root", controllers);

        // assert
        var types = root.Clients.Select(x => x.Type).OrderBy(x => x).ToArray();
        types.Has(2);
        types.At(0).Is("AdminItemsRoot");
        types.At(1).Is("PublicItemsRoot");
    }

    [Fact]
    public void BuildClient_ControllerNamedLikeItsContainer_IsRenamed()
    {
        // arrange — regression: a RootController at the API root emitted `public RootClient Root => …`
        // inside `class Root`, and a member cannot be named after the type declaring it (CS0542)
        var clients = "Demo.Clients".ToNamespace();

        // act
        var root = (ClientContainerView)
            ClientBuilder.BuildClient(clients, "Root", "Root", [Controller(clients, "Root")]);

        // assert
        root.Clients.Has(1).At(0).Name.IsNotEqual("Root");
        root.Clients.At(0).Type.Is("RootClient");
    }

    [Fact]
    public void BuildClient_BranchesWhoseSegmentsConcatenateAlike_StayApart()
    {
        // arrange — regression: names were built by concatenating the segments below the client root,
        // so `A.BC` and `AB.C` both read `ABCRoot` and collided on the parent
        var clients = "Demo.Clients".ToNamespace();
        var controllers = new[]
        {
            Controller("Demo.Clients.A.BC".ToNamespace(), "One"),
            Controller("Demo.Clients.AB.C".ToNamespace(), "Two"),
        };

        // act
        var root = (ClientContainerView)ClientBuilder.BuildClient(clients, "Root", "Root", controllers);

        // assert
        var types = root.Clients.Select(x => x.Type).OrderBy(x => x).ToArray();
        types.Has(2);
        types.At(0).IsNotEqual(types.At(1));
    }

    [Fact]
    public void BuildClient_ControllersSharingAName_GetDistinctTypesAndProperties()
    {
        // arrange — a controller's name comes from its route, not its class, so two classes can carry
        // the same one: the container declared the property twice and both clients wrote one file
        var clients = "Demo.Clients".ToNamespace();
        var controllers = new[] { Controller(clients, "Probe"), Controller(clients, "Probe") };

        // act
        var root = (ClientContainerView)ClientBuilder.BuildClient(clients, "Root", "Root", controllers);

        // assert
        root.Clients.Select(x => x.Type).Distinct().ToArray().Has(2);
        root.Clients.Select(x => x.Name).Distinct().ToArray().Has(2);
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
