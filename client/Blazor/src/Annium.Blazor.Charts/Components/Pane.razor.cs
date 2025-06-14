using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Chart pane component that provides a container for chart series and manages pane-specific rendering context.
/// </summary>
public partial class Pane : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the CSS class to apply to the pane container.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the pane.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    /// <summary>
    /// Gets or sets the chart context from the parent chart component.
    /// </summary>
    [CascadingParameter]
    internal IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the managed pane context for this pane.
    /// </summary>
    [Inject]
    private IManagedPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CSS styles for the pane component.
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets the computed CSS class string for the pane container.
    /// </summary>
    private string Class => ClassBuilder.With(Styles.Block).With(CssClass).Build();

    /// <summary>
    /// Container for managing disposable resources.
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called after the component has been rendered. Initializes the pane context on first render.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        PaneContext.Init(ChartContext);
        _disposable += ChartContext.RegisterPane(PaneContext);
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    /// <summary>
    /// CSS styles for the Pane component.
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the pane block element with grid layout.
        /// </summary>
        public readonly CssRule Block = Rule.Class()
            .WidthPercent(100)
            .Set("display", "grid")
            .Set("grid-template-rows", "1fr min-content")
            .Set("grid-template-columns", "1fr min-content");
    }
}
