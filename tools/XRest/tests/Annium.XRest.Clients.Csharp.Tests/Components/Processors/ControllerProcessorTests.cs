using System.Linq;
using System.Net.Http;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Annium.XRest.Core.Models;
using Xunit;

namespace Annium.XRest.Clients.Csharp.Tests.Components.Processors;

// The generated client declares one method per action, named after the action, so two actions of a
// controller that share a name produce a client that does not compile.
public class ControllerProcessorTests
{
    [Fact]
    public void Process_ActionsSharingAName_AreDisambiguatedByVerb()
    {
        // arrange — regression: `[HttpGet("items/{id}")]` and `[HttpDelete("items/{id}")]` on one
        // method arrive as two models named after that method, and the generated client failed with
        // CS0111: already defines a member with the same parameter types
        var controller = Controller(Action(HttpMethod.Get, "Items"), Action(HttpMethod.Delete, "Items"));

        // act
        var view = ControllerProcessor.Process(controller, Context());

        // assert
        view.Actions.Select(x => x.Name).OrderBy(x => x).ToArray().At(0).Is("DeleteItems");
        view.Actions.Select(x => x.Name).OrderBy(x => x).ToArray().At(1).Is("GetItems");
    }

    [Fact]
    public void Process_ActionsWithDistinctNames_KeepThem()
    {
        // arrange — disambiguating unconditionally would rename every method of every client
        var controller = Controller(Action(HttpMethod.Get, "Items"), Action(HttpMethod.Post, "Create"));

        // act
        var view = ControllerProcessor.Process(controller, Context());

        // assert
        view.Actions.Select(x => x.Name).OrderBy(x => x).ToArray().At(0).Is("Create");
        view.Actions.Select(x => x.Name).OrderBy(x => x).ToArray().At(1).Is("Items");
    }

    [Fact]
    public void Process_VerbNameCollidingWithAnotherAction_IsNumbered()
    {
        // arrange — regression: `Items` on GET+DELETE renames to `GetItems`, which a controller can
        // already have as an action of its own, giving two identical signatures (CS0111)
        var controller = Controller(
            Action(HttpMethod.Get, "Items"),
            Action(HttpMethod.Delete, "Items"),
            Action(HttpMethod.Get, "GetItems")
        );

        // act
        var view = ControllerProcessor.Process(controller, Context());

        // assert
        view.Actions.Select(x => x.Name).Distinct().ToArray().Has(3);
    }

    private static ApiContext Context() => new("Demo.Clients".ToNamespace(), "Demo.Models".ToNamespace(), []);

    private static ControllerModel Controller(params ActionModel[] actions) =>
        new("Demo".ToNamespace(), "Probe", actions);

    private static ActionModel Action(HttpMethod method, string name) =>
        new(method, "items/{id}", name, [], null, new BaseTypeRef(BaseType.String));
}
