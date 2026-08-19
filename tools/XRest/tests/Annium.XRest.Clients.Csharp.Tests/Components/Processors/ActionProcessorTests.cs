using System.Linq;
using System.Net.Http;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Annium.XRest.Core.Models;
using Xunit;

namespace Annium.XRest.Clients.Csharp.Tests.Components.Processors;

// What the client template renders comes from here: which arguments are interpolated into the path,
// which are appended as query values, and what the call returns.
public class ActionProcessorTests
{
    [Fact]
    public void Process_Parameters_AreSplitByLocation()
    {
        // arrange — a query parameter rendered into the path (or the reverse) calls the wrong URL
        var action = new ActionModel(
            HttpMethod.Get,
            "items/{id}",
            "Items",
            [
                new ParameterModel(ParameterLocationEnum.Path, new BaseTypeRef(BaseType.Int), "id"),
                new ParameterModel(ParameterLocationEnum.Query, new BaseTypeRef(BaseType.String), "term"),
            ],
            null,
            new BaseTypeRef(BaseType.String)
        );

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert
        view.PathParameters.Has(1).At(0).Name.Is("id");
        view.QueryParameters.Has(1).At(0).Name.Is("term");
        view.Parameters.Has(2);
    }

    [Fact]
    public void Process_Body_BecomesATrailingParameter()
    {
        // arrange
        var action = new ActionModel(
            HttpMethod.Post,
            "items",
            "Create",
            [],
            new StructRef("Demo.Models", "Item"),
            new BaseTypeRef(BaseType.String)
        );

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert
        view.HasBody.IsTrue();
        view.Body.Is("Item");
        view.Parameters.Has(1).At(0).Name.Is("body");
    }

    [Fact]
    public void Process_PromiseResponse_IsUnwrappedToWhatItResolvesTo()
    {
        // arrange — the client already awaits the call, so the model's Task<T> must render as T
        var action = new ActionModel(
            HttpMethod.Get,
            "items",
            "Items",
            [],
            null,
            new PromiseRef(new BaseTypeRef(BaseType.String))
        );

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert
        view.HasResponse.IsTrue();
        view.Response.Is("string");
    }

    [Fact]
    public void Process_ValuelessPromiseResponse_LeavesTheCallWithoutOne()
    {
        // arrange — a `Task`-returning action has nothing to hand back
        var action = new ActionModel(HttpMethod.Post, "items", "Create", [], null, new PromiseRef(null));

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert
        view.HasResponse.IsFalse();
    }

    [Fact]
    public void Process_VoidResponse_LeavesTheCallWithoutOne()
    {
        // arrange — a synchronous `void` action arrives as a plain void base type, not a promise, and
        // rendering it gave `Task<Void>` plus a `Void defaultValue` parameter
        var action = new ActionModel(HttpMethod.Post, "items", "Create", [], null, new BaseTypeRef(BaseType.Void));

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert
        view.HasResponse.IsFalse();
    }

    [Fact]
    public void Process_NameArgument_IsWhatTheMethodIsCalled()
    {
        // arrange — the caller passes a disambiguated name when a controller has colliding actions
        var action = new ActionModel(HttpMethod.Get, "items", "Items", [], null, new BaseTypeRef(BaseType.String));

        // act
        var view = ActionProcessor.Process(action, "GetItems", Context());

        // assert
        view.Name.Is("GetItems");
    }

    [Fact]
    public void Process_ParameterNamedLikeTheBody_KeepsBothDeclarable()
    {
        // arrange — regression: the body wrapper is always declared as `body`, so a route parameter of
        // that name gave the generated method two parameters called `body` (CS0100)
        var action = new ActionModel(
            HttpMethod.Post,
            "items/{body}",
            "Create",
            [new ParameterModel(ParameterLocationEnum.Path, new BaseTypeRef(BaseType.String), "body")],
            new StructRef("Demo.Models", "Item"),
            new BaseTypeRef(BaseType.String)
        );

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert — the path parameter keeps its identifier, since the route interpolates it
        view.PathParameters.Has(1).At(0).Argument.Is("body");
        view.BodyArgument.IsNotEqual("body");
        view.Parameters.Select(x => x.Argument).Distinct().ToArray().Has(2);
    }

    [Fact]
    public void Process_QueryParametersSharingAName_AreDeclaredApartButSentAsThemselves()
    {
        // arrange — a complex query object flattens into its properties, which can repeat a name a
        // sibling parameter already uses
        var action = new ActionModel(
            HttpMethod.Get,
            "items",
            "Items",
            [
                new ParameterModel(ParameterLocationEnum.Query, new BaseTypeRef(BaseType.String), "id"),
                new ParameterModel(ParameterLocationEnum.Query, new BaseTypeRef(BaseType.String), "id"),
            ],
            null,
            new BaseTypeRef(BaseType.String)
        );

        // act
        var view = ActionProcessor.Process(action, action.Name, Context());

        // assert — distinct identifiers, but both still sent under the name the server knows
        view.QueryParameters.Select(x => x.Argument).Distinct().ToArray().Has(2);
        view.QueryParameters.At(0).Name.Is("id");
        view.QueryParameters.At(1).Name.Is("id");
    }

    private static ProcessingContext Context() => new("Demo.Models".ToNamespace(), []);
}
