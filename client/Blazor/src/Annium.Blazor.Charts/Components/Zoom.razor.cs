using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Chart zoom control component that provides zoom in and zoom out functionality
/// </summary>
public partial class Zoom : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Optional CSS class to apply to the zoom component
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Chart context providing access to chart operations and state
    /// </summary>
    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Injected CSS styles for the zoom component
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Logger instance for the zoom component
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Combined CSS class string for the zoom component
    /// </summary>
    private string Class => ClassBuilder.With(Styles.Container).With(CssClass).Build();

    /// <summary>
    /// Disposable resource container for cleanup
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Lifecycle method called after component rendering
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += ChartContext.OnUpdate(StateHasChanged);
    }

    /// <summary>
    /// Handles zoom in button click by calling chart context zoom in method
    /// </summary>
    private void HandleZoomIn() => ChartContext.ZoomIn();

    /// <summary>
    /// Handles zoom out button click by calling chart context zoom out method
    /// </summary>
    private void HandleZoomOut() => ChartContext.ZoomOut();

    /// <summary>
    /// Disposes of async resources used by the zoom component
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    /// <summary>
    /// CSS styles for the zoom component
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the zoom component container
        /// </summary>
        public readonly CssRule Container = Rule.Class()
            .PositionAbsolute()
            .FlexColumn(AlignItems.Stretch, JustifyContent.Stretch)
            .Set("user-select", "none");

        /// <summary>
        /// CSS rule for zoom buttons
        /// </summary>
        public readonly CssRule Button = Rule.Class()
            .FlexRow(AlignItems.Center, JustifyContent.Center)
            .WidthRem(2)
            .HeightRem(2)
            .FontSizeRem(1.5)
            .Border("1px solid black")
            .Set("cursor", "pointer");
    }
}
