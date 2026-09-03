using System.Collections.Generic;
using System.Reflection;
using Annium.Testing;
using Annium.XRest.Sources.AspNetCore.Internal.Components;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace Annium.XRest.Sources.AspNetCore.Tests.Internal.Components;

// The namespace this builds is what both client generators emit their files into, so a segment landing
// in the wrong place renames every generated type's namespace.
public class ControllerModelBuilderTests
{
    // no action here survives to a model — the descriptors carry no attribute route — so the mapper is
    // never touched and only the controller-level naming is under test
    private static readonly MappingContext _unusedContext = new(null!, null!);

    [Fact]
    public void Build_NamespaceUnderControllers_KeepsOnlyWhatFollowsIt()
    {
        // arrange — the controller sits in Sample.Controllers.Deep
        var action = CreateAction();

        // act
        var model = ControllerModelBuilder.Build([action], _unusedContext);

        // assert
        model.Name.Is("probe");
        model.Namespace.ToString().Is("Deep");
    }

    [Fact]
    public void Build_AreaRoute_PrependsTheAreaToTheNamespace()
    {
        // arrange — an area names the outermost namespace segment; appending it instead would bury it
        // under the controller's own path
        var action = CreateAction(area: "admin");

        // act
        var model = ControllerModelBuilder.Build([action], _unusedContext);

        // assert — appending instead would read Deep.Admin
        model.Namespace.ToString().Is("Admin.Deep");
    }

    [Fact]
    public void Build_DynamicKeyRoute_AppendsItToTheNamespace()
    {
        // arrange — the dynamic key qualifies the controller, so it goes last
        var action = CreateAction(dynamicKey: "tenant");

        // act
        var model = ControllerModelBuilder.Build([action], _unusedContext);

        // assert — prepending instead would read Tenant.Deep
        model.Namespace.ToString().Is("Deep.Tenant");
    }

    [Fact]
    public void Build_AreaAndDynamicKey_WrapTheNamespaceOnBothSides()
    {
        // arrange
        var action = CreateAction(area: "admin", dynamicKey: "tenant");

        // act
        var model = ControllerModelBuilder.Build([action], _unusedContext);

        // assert
        model.Namespace.ToString().Is("Admin.Deep.Tenant");
    }

    private static ControllerActionDescriptor CreateAction(string? area = null, string? dynamicKey = null)
    {
        var routeValues = new Dictionary<string, string?> { ["controller"] = "probe" };
        if (area is not null)
            routeValues["area"] = area;
        if (dynamicKey is not null)
            routeValues["dynamicKey"] = dynamicKey;

        return new ControllerActionDescriptor
        {
            ActionName = "Probe",
            ControllerName = "Probe",
            MethodInfo = typeof(ControllerModelBuilderTests).GetMethod(
                nameof(Probe),
                BindingFlags.NonPublic | BindingFlags.Static
            )!,
            ControllerTypeInfo = typeof(Sample.Controllers.Deep.ProbeController).GetTypeInfo(),
            RouteValues = routeValues,
            Parameters = [],
        };
    }

    private static string Probe() => string.Empty;
}
