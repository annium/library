using System.Linq;
using System.Net.Http;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Clients.Csharp.Views.Client;
using Annium.XRest.Core.Models;
using Xunit;

namespace Annium.XRest.Clients.Csharp.Tests.Components.Processors;

// The names the generator reserves are only known here, where the whole API is in view: a model
// sharing one of them has to be written in full, since a type declared in the file's own namespace
// wins over anything a `using` brings in — silently, with no compiler complaint.
public class ProcessorTests
{
    [Fact]
    public void Process_ModelNamedLikeTheLeafClient_IsWrittenInFull()
    {
        // arrange — regression: a model named `RootClient` returned by RootController bound to the
        // generated client wrapper, so the call deserialized into the wrong type at runtime
        var api = Api(
            "Root",
            new StructRef("Demo.Models", "RootClient"),
            new StructModel("Demo.Models".ToNamespace(), false, "RootClient")
        );

        // act
        var view = Processor.Process("Demo".ToNamespace(), api);

        // assert
        Response(view).StartsWith("global::").IsTrue();
    }

    [Fact]
    public void Process_ModelNamedLikeNothingGenerated_StaysShort()
    {
        // arrange — qualifying unconditionally would bury every generated signature in namespaces
        var api = Api(
            "Root",
            new StructRef("Demo.Models", "Item"),
            new StructModel("Demo.Models".ToNamespace(), false, "Item")
        );

        // act
        var view = Processor.Process("Demo".ToNamespace(), api);

        // assert
        Response(view).Is("Item");
    }

    [Fact]
    public void Process_ModelNamedLikeTheRootContainer_IsWrittenInFull()
    {
        // arrange — the root container is always called `Root`, whether or not any controller sits at
        // the root, and a model of that name in scope wins over the `using`, silently. Both
        // controllers are nested, so the name can only come from the root container itself
        var api = new ApiModel(
            [
                Controller("Admin", "Probe", new StructRef("Demo.Models", "Root")),
                Controller("Public", "Other", new BaseTypeRef(BaseType.String)),
            ],
            [new StructModel("Demo.Models".ToNamespace(), false, "Root")]
        );

        // act
        var view = Processor.Process("Demo".ToNamespace(), api);

        // assert
        var admin = ((ClientContainerView)view.Client)
            .Clients.OfType<ClientContainerView>()
            .Single(x => x.Type == "AdminRoot");
        ((ClientView)admin.Clients.Single()).Actions.Single().Response.StartsWith("global::").IsTrue();
    }

    [Fact]
    public void Process_ModelNamedLikeABranchContainer_IsWrittenInFull()
    {
        // arrange — a controller under `Admin` puts an `AdminRoot` container in scope
        var api = new ApiModel(
            [
                new ControllerModel(
                    "Admin".ToNamespace(),
                    "Probe",
                    [
                        new ActionModel(
                            HttpMethod.Get,
                            "probe",
                            "Get",
                            [],
                            null,
                            new StructRef("Demo.Models", "AdminRoot")
                        ),
                    ]
                ),
                new ControllerModel(
                    Namespace.New([]),
                    "Index",
                    [new ActionModel(HttpMethod.Get, "", "Index", [], null, new BaseTypeRef(BaseType.String))]
                ),
            ],
            [new StructModel("Demo.Models".ToNamespace(), false, "AdminRoot")]
        );

        // act
        var view = Processor.Process("Demo".ToNamespace(), api);

        // assert
        var admin = ((ClientContainerView)view.Client).Clients.OfType<ClientContainerView>().Single();
        ((ClientView)admin.Clients.Single()).Actions.Single().Response.StartsWith("global::").IsTrue();
    }

    private static string Response(ApiView view) =>
        ((ClientView)((ClientContainerView)view.Client).Clients.Single()).Actions.Single().Response;

    private static ControllerModel Controller(string @namespace, string name, IRef response) =>
        new(@namespace.ToNamespace(), name, [new ActionModel(HttpMethod.Get, "probe", "Get", [], null, response)]);

    private static ApiModel Api(string controller, IRef response, params IModel[] models) =>
        new(
            [
                new ControllerModel(
                    Namespace.New([]),
                    controller,
                    [new ActionModel(HttpMethod.Get, "probe", "Get", [], null, response)]
                ),
            ],
            models
        );
}
