using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;
using Annium.Net.Types;
using Annium.Testing;
using Annium.XRest.Core.Models;
using Annium.XRest.Sources.AspNetCore.Internal.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.XRest.Sources.AspNetCore.Tests.Internal.Components;

public class ActionModelBuilderTests
{
    /// <summary>
    /// Both cases below bail out before any type mapping happens, so the context is never touched —
    /// passing nulls keeps these regression tests free of a DI-built IModelMapper.
    /// </summary>
    private static readonly MappingContext _unusedContext = new(null!, null!);

    [Fact]
    public void Build_ActionWithoutHttpMethodConstraint_YieldsNothing()
    {
        // arrange — regression: `ActionConstraints` is null (not empty) for a bare [Route] action,
        // and dereferencing it threw ArgumentNullException, failing the whole `.xrest` endpoint
        // with a 500 rather than just skipping the action
        var action = CreateAction(template: "probe/anymethod", constraints: null);

        // act
        var models = ActionModelBuilder.Build(action, _unusedContext).ToArray();

        // assert
        models.IsEmpty();
    }

    [Fact]
    public void Build_ConventionallyRoutedAction_YieldsNothing()
    {
        // arrange — no attribute route to describe
        var action = CreateAction(template: null, constraints: [new HttpMethodActionConstraint(["GET"])]);

        // act
        var models = ActionModelBuilder.Build(action, _unusedContext).ToArray();

        // assert
        models.IsEmpty();
    }

    [Fact]
    public void Build_RouteAndQueryParameters_ClassifiesEachByTheRouteTemplate()
    {
        // arrange — this is the model both client generators consume: a parameter placed in the query
        // instead of the path generates a client that calls the wrong URL
        var action = CreateAction(
            template: "probe/{id}",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("id", typeof(int)), Parameter("page", typeof(int))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Parameters.Has(2);
        model.Parameters.At(0).Location.Is(ParameterLocationEnum.Path);
        model.Parameters.At(0).Name.Is("id");
        model.Parameters.At(1).Location.Is(ParameterLocationEnum.Query);
        model.Parameters.At(1).Name.Is("page");
    }

    [Fact]
    public void Build_RouteParameterCasedDifferently_IsStillAPathParameter()
    {
        // arrange — route matching is case-insensitive, so `{Id}` binds a parameter declared as `id`;
        // an ordinal comparison here silently demotes it to a query parameter
        var action = CreateAction(
            template: "probe/{Id}",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("id", typeof(int))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Parameters.Has(1).At(0).Location.Is(ParameterLocationEnum.Path);
    }

    [Fact]
    public void Build_ComplexQueryParameter_IsFlattenedIntoItsProperties()
    {
        // arrange — a bound object is not a query value itself; the client needs one query parameter
        // per readable property, camel-cased
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("filter", typeof(ProbeFilter))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Parameters.Has(2);
        model.Parameters.At(0).Name.Is("term");
        model.Parameters.At(1).Name.Is("take");
        model.Parameters.At(0).Location.Is(ParameterLocationEnum.Query);
    }

    [Fact]
    public void Build_ActionWithSeveralHttpMethods_YieldsOneModelPerMethod()
    {
        // arrange — every other case has a single verb, so the loop could emit just the first
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["GET", "HEAD"])],
            parameters: []
        );

        // act
        var models = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray();

        // assert
        models.Has(2);
        models.At(0).Method.Method.Is("GET");
        models.At(1).Method.Method.Is("HEAD");
    }

    [Fact]
    public void Build_BodyBoundParameter_BecomesTheBodyAndNotAParameter()
    {
        // arrange — a body parameter leaking into Parameters generates a client that also puts the
        // payload in the query string
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["POST"])],
            parameters: [BodyParameter("payload", typeof(ProbeFilter)), Parameter("page", typeof(int))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Body.IsNotDefault();
        model.Parameters.Has(1).At(0).Name.Is("page");
    }

    [Fact]
    public void Build_NoBodyBoundParameter_LeavesTheBodyUnset()
    {
        // arrange
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("page", typeof(int))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Body.IsDefault();
        model.Response.IsNotDefault();
    }

    [Fact]
    public void Build_CancellationTokenParameter_IsNotDescribedAtAll()
    {
        // arrange — the token is an ASP.NET binding detail; describing it would flatten it into a
        // fistful of query parameters on every generated call
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("ct", typeof(CancellationToken)), Parameter("page", typeof(int))]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Parameters.Has(1).At(0).Name.Is("page");
    }

    [Theory]
    // an enum and an array of a base type are query values in their own right; treating either as a
    // complex type would flatten it into its members instead
    [InlineData(typeof(ProbeMode))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(ProbeMode[]))]
    public void Build_EnumAndArrayParameters_StayASingleQueryParameter(System.Type type)
    {
        // arrange
        var action = CreateAction(
            template: "probe",
            constraints: [new HttpMethodActionConstraint(["GET"])],
            parameters: [Parameter("value", type)]
        );

        // act
        var model = ActionModelBuilder.Build(action, MappingContexts.Create()).ToArray().Has(1).At(0);

        // assert
        model.Parameters.Has(1);
        model.Parameters.At(0).Name.Is("value");
        model.Parameters.At(0).Location.Is(ParameterLocationEnum.Query);
    }

    private static ParameterDescriptor Parameter(string name, System.Type type) =>
        new()
        {
            Name = name,
            ParameterType = type,
            BindingInfo = new BindingInfo(),
        };

    private static ParameterDescriptor BodyParameter(string name, System.Type type) =>
        new()
        {
            Name = name,
            ParameterType = type,
            BindingInfo = new BindingInfo { BindingSource = BindingSource.Body },
        };

    private sealed record ProbeFilter(string Term, int Take);

    private enum ProbeMode
    {
        One,
        Two,
    }

    private static ControllerActionDescriptor CreateAction(
        string? template,
        IList<IActionConstraintMetadata>? constraints,
        IList<ParameterDescriptor>? parameters = null
    ) =>
        new()
        {
            ActionName = "Probe",
            ControllerName = "Probe",
            MethodInfo = typeof(ActionModelBuilderTests).GetMethod(
                nameof(Probe),
                BindingFlags.NonPublic | BindingFlags.Static
            )!,
            ControllerTypeInfo = typeof(ActionModelBuilderTests).GetTypeInfo(),
            AttributeRouteInfo = template is null ? null : new AttributeRouteInfo { Template = template },
            ActionConstraints = constraints,
            Parameters = parameters ?? [],
        };

    private static string Probe() => string.Empty;
}
