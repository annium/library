using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Testing;
using Annium.XRest.Sources.AspNetCore.Internal.Components;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace Annium.XRest.Sources.AspNetCore.Tests.Internal.Components;

// One controller per type, whatever order the descriptions arrive in: a controller split in two
// emits two files that overwrite each other in the generated client.
public class ApiModelBuilderTests
{
    // no descriptor here carries an attribute route, so no action survives to a model — but the
    // builder still asks the mapper for the models it collected, so this needs a real one
    private static readonly MappingContext _context = MappingContexts.Create();

    [Fact]
    public void Build_ActionsOfTheSameController_AreGroupedIntoOne()
    {
        // arrange — interleaved, so a grouping that relied on adjacency would split them
        var descriptions = new[]
        {
            Description<Sample.Controllers.Deep.ProbeController>("first"),
            Description<Sample.Controllers.Deep.OtherController>("other"),
            Description<Sample.Controllers.Deep.ProbeController>("second"),
        };

        // act
        var model = ApiModelBuilder.Build(descriptions, _context);

        // assert
        model.Controllers.Has(2);
        model.Controllers.Select(x => x.Name).OrderBy(x => x).ToArray().At(0).Is("other");
    }

    [Fact]
    public void Build_NonControllerActions_AreIgnored()
    {
        // arrange — minimal APIs and other endpoint sources share the ApiDescription list
        var descriptions = new[]
        {
            new ApiDescription { ActionDescriptor = new ActionDescriptor() },
            Description<Sample.Controllers.Deep.ProbeController>("first"),
        };

        // act
        var model = ApiModelBuilder.Build(descriptions, _context);

        // assert
        model.Controllers.Has(1).At(0).Name.Is("probe");
    }

    [Fact]
    public void Build_TwoControllersSharingAClassName_StayApart()
    {
        // arrange — ASP.NET derives ControllerName from the class name alone, so a versioned
        // V1.ProbeController and V2.ProbeController share it; grouping by name would merge their
        // actions into one generated file
        var descriptions = new[]
        {
            Description<Sample.Controllers.Deep.ProbeController>("first"),
            Description<Sample.Controllers.Other.ProbeController>("second"),
        };

        // act
        var model = ApiModelBuilder.Build(descriptions, _context);

        // assert
        model.Controllers.Has(2);
    }

    private static ApiDescription Description<TController>(string action) =>
        new()
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ActionName = action,
                ControllerName = typeof(TController).Name,
                MethodInfo = typeof(ApiModelBuilderTests).GetMethod(
                    nameof(Probe),
                    BindingFlags.NonPublic | BindingFlags.Static
                )!,
                ControllerTypeInfo = typeof(TController).GetTypeInfo(),
                RouteValues = new Dictionary<string, string?>
                {
                    ["controller"] =
                        typeof(TController) == typeof(Sample.Controllers.Deep.ProbeController) ? "probe" : "other",
                },
                Parameters = [],
            },
        };

    private static string Probe() => string.Empty;
}
