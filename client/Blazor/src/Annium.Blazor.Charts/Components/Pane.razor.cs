using System;
using System.Drawing;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class Pane
{
    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [CascadingParameter]
    internal IChartContext ChartContext { get; set; } = default!;

    [Inject]
    private IManagedPaneContext PaneContext { get; set; } = default!;

    private string Class => ClassBuilder.With(_style.Block).With(CssClass).Build();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        PaneContext.Init(ChartContext);
        ChartContext.Register(PaneContext);
    }

    public class Style : RuleSet
    {
        public readonly CssRule Block = Rule.Class()
            .WidthPercent(100)
            .Set("display", "grid")
            .Set("grid-template-rows", "1fr min-content")
            .Set("grid-template-columns", "1fr min-content");
    }
}