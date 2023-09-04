using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Annium.NodaTime.Extensions;
using Microsoft.AspNetCore.Components;

using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class XAxis : ILogSubject, IAsyncDisposable
{
    [Parameter]
    public string LabelFontFamily { get; set; } = AxisLabelFontFamily;

    [Parameter]
    public int LabelFontSize { get; set; } = AxisLabelFontSize;

    [Parameter]
    public string LabelStyle { get; set; } = AxisLabelStyle;

    [CascadingParameter]
    public IChartContext Chart { get; set; } = default!;

    [CascadingParameter]
    public IHorizontalSideContext SideContext { get; set; } = default!;

    [Inject]
    public ILogger Logger { get; set; } = default!;

    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += Chart.OnUpdate(Draw);
    }

    private void Draw()
    {
        var ctx = SideContext.Canvas;

        ctx.Save();

        ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
        ctx.FillStyle = LabelStyle;
        ctx.TextBaseline = CanvasTextBaseline.middle;

        var baseline = (SideContext.Rect.Height / 2).FloorInt32();
        foreach (var (x, m) in Chart.GetVerticalLines())
        {
            if (m.IsMidnight())
            {
                var text = m.ToString("dd.MM", null);
                var offset = ((double) ctx.MeasureTextWidth(text) / 2).CeilInt32();

                ctx.Font = $"bold {LabelFontSize}px {LabelFontFamily}";
                ctx.FillText(text, x - offset, baseline);
                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
            }
            else
            {
                var text = m.ToString("HH:mm", null);
                var offset = ((double) ctx.MeasureTextWidth(text) / 2).CeilInt32();

                ctx.FillText(text, x - offset, baseline);
            }
        }

        ctx.Restore();
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}