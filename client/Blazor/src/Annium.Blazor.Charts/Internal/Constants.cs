using NodaTime;

namespace Annium.Blazor.Charts.Internal;

/// <summary>
/// Internal constants used throughout the Blazor Charts library
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Animation frame duration in milliseconds
    /// </summary>
    public const int AnimationFrameMs = 40;

    /// <summary>
    /// Multiplier applied to scroll wheel events for chart navigation
    /// </summary>
    public const decimal ScrollMultiplier = 0.5m;

    /// <summary>
    /// Full grid line width in pixels
    /// </summary>
    public static int GridLine => (int)(GridHalfLine * 2);

    /// <summary>
    /// Half grid line width in pixels
    /// </summary>
    public const float GridHalfLine = 0.5f;

    /// <summary>
    /// Color style for grid lines
    /// </summary>
    public const string GridStyle = "#eee";

    /// <summary>
    /// Font family for series labels
    /// </summary>
    public const string SeriesLabelFontFamily = "sans-serif";

    /// <summary>
    /// Font size for series labels in pixels
    /// </summary>
    public const int SeriesLabelFontSize = 12;

    /// <summary>
    /// Color style for series labels
    /// </summary>
    public const string SeriesLabelStyle = "black";

    /// <summary>
    /// Font family for axis labels
    /// </summary>
    public const string AxisLabelFontFamily = "sans-serif";

    /// <summary>
    /// Font size for axis labels in pixels
    /// </summary>
    public const int AxisLabelFontSize = 12;

    /// <summary>
    /// Color style for axis labels
    /// </summary>
    public const string AxisLabelStyle = "black";

    /// <summary>
    /// Color style for crosshair lines
    /// </summary>
    public const string CrosshairLineStyle = "#555";

    /// <summary>
    /// Background color for crosshair labels
    /// </summary>
    public const string CrosshairLabelBackground = "#333";

    /// <summary>
    /// Font family for crosshair labels
    /// </summary>
    public const string CrosshairLabelFontFamily = "sans-serif";

    /// <summary>
    /// Font size for crosshair labels in pixels
    /// </summary>
    public const int CrosshairLabelFontSize = 12;

    /// <summary>
    /// Color style for crosshair labels
    /// </summary>
    public const string CrosshairLabelStyle = "white";

    /// <summary>
    /// Past boundary instant for time-based charts, set to 100,000 days before Unix epoch
    /// </summary>
    public static readonly Instant PastBound = NodaConstants.UnixEpoch - Duration.FromDays(100000);

    /// <summary>
    /// Future boundary instant for time-based charts, set to 100,000 days after Unix epoch
    /// </summary>
    public static readonly Instant FutureBound = NodaConstants.UnixEpoch + Duration.FromDays(100000);
}
