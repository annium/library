using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Resolution selector component that allows users to change the time resolution of the chart
/// </summary>
public partial class Resolution : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Additional CSS class to apply to the component
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// The chart context providing access to chart-level data and operations
    /// </summary>
    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// The CSS styles for the component
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Logger for the component
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// The computed CSS class string for the component
    /// </summary>
    private string Class => ClassBuilder.With(Styles.Container).With(CssClass).Build();

    /// <summary>
    /// Container for managing disposable resources
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called after the component has been rendered
    /// </summary>
    /// <param name="firstRender">True if this is the first render</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += ChartContext.OnUpdate(StateHasChanged);
    }

    /// <summary>
    /// Creates an action handler for selecting a specific resolution
    /// </summary>
    /// <param name="resolution">The resolution to select</param>
    /// <returns>An action that sets the chart resolution</returns>
    private Action HandleResolutionSelect(Duration resolution) => () => ChartContext.SetResolution(resolution);

    /// <summary>
    /// Converts a duration to a human-readable string format
    /// </summary>
    /// <param name="resolution">The duration to humanize</param>
    /// <returns>A formatted string representing the duration</returns>
    private string Humanize(Duration resolution) =>
        resolution.TotalMinutes switch
        {
            < 60 => $"{resolution.TotalMinutes}m",
            < 1440 => $"{resolution.TotalHours}h",
            _ => $"{resolution.TotalDays}d",
        };

    /// <summary>
    /// Disposes of the component's resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    /// <summary>
    /// CSS styles for the Resolution component
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the main container element
        /// </summary>
        public readonly CssRule Container = Rule.Class()
            .PositionAbsolute()
            .FlexRow(AlignItems.Stretch, JustifyContent.Stretch);

        /// <summary>
        /// CSS rule for individual resolution option elements
        /// </summary>
        public readonly CssRule Resolution = Rule.Class()
            .FlexRow(AlignItems.Center, JustifyContent.Center)
            .MarginPx(0, 5)
            .Set("cursor", "pointer");

        /// <summary>
        /// CSS rule for the currently active resolution option
        /// </summary>
        public readonly CssRule ActiveResolution = Rule.Class().FontWeightBold();
    }
}
