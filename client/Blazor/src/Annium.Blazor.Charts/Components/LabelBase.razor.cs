using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using NodaTime;
using OneOf;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Base class for chart labels that can display text at specific positions on the chart
/// </summary>
/// <typeparam name="T">The type of data associated with the label</typeparam>
public abstract partial class LabelBase<T> : IAsyncDisposable
{
    /// <summary>
    /// The text to display on the label. Can be a static string, a function of the data item, or a function of the time and data item
    /// </summary>
    [Parameter, EditorRequired]
    public OneOf<string, Func<T, string>, Func<Instant, T, string>> Text { get; set; }

    /// <summary>
    /// The lookup match strategy to use when finding data items
    /// </summary>
    [Parameter]
    public LookupMatch Match { get; set; } = LookupMatch.Exact;

    /// <summary>
    /// The left position of the label in pixels. Can be a static value or a function of the data item, time, or pane context
    /// </summary>
    [Parameter]
    public OneOf<
        int,
        Func<T, int>,
        Func<IPaneContext, Instant, int>,
        Func<IPaneContext, Instant, T, int>
    >? Left { get; set; }

    /// <summary>
    /// The right position of the label in pixels. Can be a static value or a function of the data item, time, or pane context
    /// </summary>
    [Parameter]
    public OneOf<
        int,
        Func<T, int>,
        Func<IPaneContext, Instant, int>,
        Func<IPaneContext, Instant, T, int>
    >? Right { get; set; }

    /// <summary>
    /// The top position of the label in pixels. Can be a static value or a function of the data item or pane context
    /// </summary>
    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? Top { get; set; }

    /// <summary>
    /// The bottom position of the label in pixels. Can be a static value or a function of the data item or pane context
    /// </summary>
    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? Bottom { get; set; }

    /// <summary>
    /// The font family to use for the label text
    /// </summary>
    [Parameter]
    public string FontFamily { get; set; } = SeriesLabelFontFamily;

    /// <summary>
    /// The font size to use for the label text in pixels
    /// </summary>
    [Parameter]
    public int FontSize { get; set; } = SeriesLabelFontSize;

    /// <summary>
    /// The color of the label text. Can be a static color or a function of the data item
    /// </summary>
    [Parameter]
    public OneOf<string, Func<T, string>> Color { get; set; } = SeriesLabelStyle;

    /// <summary>
    /// The chart context providing access to chart-level data and operations
    /// </summary>
    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// The pane context providing access to pane-level data and operations
    /// </summary>
    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// The series context providing access to series-level data and operations
    /// </summary>
    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = null!;

    /// <summary>
    /// Container for managing disposable resources
    /// </summary>
    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called when component parameters are set, validates that required position parameters are provided
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Left is null && Right is null)
            throw new ArgumentException($"Either {nameof(Left)} or {nameof(Right)} must be specified");

        if (Top is null && Bottom is null)
            throw new ArgumentException($"Either {nameof(Top)} or {nameof(Bottom)} must be specified");
    }

    /// <summary>
    /// Renders the label items for the given moment and data items
    /// </summary>
    /// <param name="moment">The time moment for which to render labels</param>
    /// <param name="items">The data items to render labels for</param>
    protected void RenderItems(Instant moment, IReadOnlyCollection<T> items)
    {
        if (items.Count == 0)
            return;

        var ctx = SeriesContext.Overlay;
        var rect = SeriesContext.Rect;

        ctx.Save();

        foreach (var item in items)
        {
            var x = GetX(Left, moment, item) ?? rect.Width.FloorInt32() - GetX(Right, moment, item)!.Value;
            var y = GetY(Top, item) ?? rect.Height.FloorInt32() - GetY(Bottom, item)!.Value;

            var text = Text.Match(value => value, get => get(item), get => get(moment, item));

            ctx.Font = $"{FontSize}px {FontFamily}";
            ctx.TextBaseline = CanvasTextBaseline.middle;

            var width = ctx.MeasureTextWidth(text);
            var height = ctx.MeasureTextHeight(text);
            ctx.FillStyle = "white";
            ctx.FillRect(x, y - (height / 2f).RoundInt32(), width, height);

            ctx.FillStyle = Color.Match(value => value, get => get(item));
            ctx.FillText(text, x, y);
        }

        ctx.Restore();
    }

    /// <summary>
    /// Gets the X position for a label based on the provided getter function
    /// </summary>
    /// <param name="getter">The function to calculate the X position</param>
    /// <param name="moment">The time moment</param>
    /// <param name="item">The data item</param>
    /// <returns>The X position in pixels, or null if no getter is provided</returns>
    private int? GetX(
        OneOf<int, Func<T, int>, Func<IPaneContext, Instant, int>, Func<IPaneContext, Instant, T, int>>? getter,
        Instant moment,
        T item
    )
    {
        if (!getter.HasValue)
            return null;

        return getter.Value.Match(
            value => value,
            get => get(item),
            get => get(PaneContext, moment),
            get => get(PaneContext, moment, item)
        );
    }

    /// <summary>
    /// Gets the Y position for a label based on the provided getter function
    /// </summary>
    /// <param name="getter">The function to calculate the Y position</param>
    /// <param name="item">The data item</param>
    /// <returns>The Y position in pixels, or null if no getter is provided</returns>
    private int? GetY(OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? getter, T item)
    {
        if (!getter.HasValue)
            return null;

        return getter.Value.Match(value => value, get => get(item), get => get(PaneContext, item));
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}
