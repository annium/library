using NodaTime;

namespace Annium.Blazor.Charts.Internal;

internal static class Constants
{
    public const int AnimationFrameMs = 40;
    public const decimal ScrollMultiplier = 0.5m;
    public static int GridLine => (int)(GridHalfLine * 2);
    public const float GridHalfLine = 0.5f;
    public const string GridStyle = "#eee";
    public const string SeriesLabelFontFamily = "sans-serif";
    public const int SeriesLabelFontSize = 12;
    public const string SeriesLabelStyle = "black";
    public const string AxisLabelFontFamily = "sans-serif";
    public const int AxisLabelFontSize = 12;
    public const string AxisLabelStyle = "black";
    public const string CrosshairLineStyle = "#555";
    public const string CrosshairLabelBackground = "#333";
    public const string CrosshairLabelFontFamily = "sans-serif";
    public const int CrosshairLabelFontSize = 12;
    public const string CrosshairLabelStyle = "white";
    public static readonly Instant PastBound = NodaConstants.UnixEpoch - Duration.FromDays(100000);
    public static readonly Instant FutureBound = NodaConstants.UnixEpoch + Duration.FromDays(100000);
}