using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Testing;
using Annium.XRest.Sources.AspNetCore.Internal.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
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

    private static ControllerActionDescriptor CreateAction(
        string? template,
        IList<IActionConstraintMetadata>? constraints
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
            Parameters = [],
        };

    private static string Probe() => string.Empty;
}
