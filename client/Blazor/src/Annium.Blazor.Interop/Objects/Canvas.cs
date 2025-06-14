using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Represents an HTML canvas element with drawing capabilities
/// </summary>
public sealed partial record Canvas : ReferenceElement
{
    /// <summary>
    /// Initializes a new instance of the Canvas class
    /// </summary>
    /// <param name="reference">The ElementReference to the HTML canvas element</param>
    public Canvas(ElementReference reference)
        : base(reference) { }

    #region Width

    /// <summary>
    /// Gets or sets the width of the canvas in pixels
    /// </summary>
    public int Width
    {
        get => GetWidth(Id);
        set => SetWidth(Id, value);
    }

    /// <summary>
    /// Gets the width of the canvas element in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The width in pixels.</returns>
    [JSImport($"{JsPath}canvas.getWidth")]
    private static partial int GetWidth(string id);

    /// <summary>
    /// Sets the width of the canvas element in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="width">The width to set in pixels.</param>
    [JSImport($"{JsPath}canvas.setWidth")]
    private static partial void SetWidth(string id, int width);

    #endregion

    #region Height

    /// <summary>
    /// Gets or sets the height of the canvas in pixels
    /// </summary>
    public int Height
    {
        get => GetHeight(Id);
        set => SetHeight(Id, value);
    }

    /// <summary>
    /// Gets the height of the canvas element in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The height in pixels.</returns>
    [JSImport($"{JsPath}canvas.getHeight")]
    private static partial int GetHeight(string id);

    /// <summary>
    /// Sets the height of the canvas element in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="height">The height to set in pixels.</param>
    [JSImport($"{JsPath}canvas.setHeight")]
    private static partial void SetHeight(string id, int height);

    #endregion

    #region FillStyle

    /// <summary>
    /// Gets or sets the fill style used for drawing operations (color, gradient, or pattern)
    /// </summary>
    public string FillStyle
    {
        get => GetFillStyle(Id);
        set => SetFillStyle(Id, value);
    }

    /// <summary>
    /// Gets the fill style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current fill style.</returns>
    [JSImport($"{JsPath}canvas.getFillStyle")]
    private static partial string GetFillStyle(string id);

    /// <summary>
    /// Sets the fill style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="style">The fill style to set.</param>
    [JSImport($"{JsPath}canvas.setFillStyle")]
    private static partial void SetFillStyle(string id, string style);

    #endregion

    #region StrokeStyle

    /// <summary>
    /// Gets or sets the stroke style used for drawing outlines (color, gradient, or pattern)
    /// </summary>
    public string StrokeStyle
    {
        get => GetStrokeStyle(Id);
        set => SetStrokeStyle(Id, value);
    }

    /// <summary>
    /// Gets the stroke style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current stroke style.</returns>
    [JSImport($"{JsPath}canvas.getStrokeStyle")]
    private static partial string GetStrokeStyle(string id);

    /// <summary>
    /// Sets the stroke style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="style">The stroke style to set.</param>
    [JSImport($"{JsPath}canvas.setStrokeStyle")]
    private static partial void SetStrokeStyle(string id, string style);

    #endregion

    #region LineCap

    /// <summary>
    /// Gets or sets the line cap style for the end of lines
    /// </summary>
    public CanvasLineCap LineCap
    {
        get => GetLineCap(Id).ParseEnum<CanvasLineCap>();
        set => SetLineCap(Id, value.ToString());
    }

    /// <summary>
    /// Gets the line cap style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current line cap style.</returns>
    [JSImport($"{JsPath}canvas.getLineCap")]
    private static partial string GetLineCap(string id);

    /// <summary>
    /// Sets the line cap style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="style">The line cap style to set.</param>
    [JSImport($"{JsPath}canvas.setLineCap")]
    private static partial void SetLineCap(string id, string style);

    #endregion

    #region LineJoin

    /// <summary>
    /// Gets or sets the line join style for connecting lines
    /// </summary>
    public CanvasLineJoin LineJoin
    {
        get => GetLineJoin(Id).ParseEnum<CanvasLineJoin>();
        set => SetLineJoin(Id, value.ToString());
    }

    /// <summary>
    /// Gets the line join style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current line join style.</returns>
    [JSImport($"{JsPath}canvas.getLineJoin")]
    private static partial string GetLineJoin(string id);

    /// <summary>
    /// Sets the line join style of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="style">The line join style to set.</param>
    [JSImport($"{JsPath}canvas.setLineJoin")]
    private static partial void SetLineJoin(string id, string style);

    #endregion

    #region LineWidth

    /// <summary>
    /// Gets or sets the width of lines in pixels
    /// </summary>
    public int LineWidth
    {
        get => GetLineWidth(Id);
        set => SetLineWidth(Id, value);
    }

    /// <summary>
    /// Gets the line width of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current line width in pixels.</returns>
    [JSImport($"{JsPath}canvas.getLineWidth")]
    private static partial int GetLineWidth(string id);

    /// <summary>
    /// Sets the line width of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="width">The line width to set in pixels.</param>
    [JSImport($"{JsPath}canvas.setLineWidth")]
    private static partial void SetLineWidth(string id, int width);

    #endregion

    #region LineDashOffset

    /// <summary>
    /// Gets or sets the offset for the line dash pattern
    /// </summary>
    public int LineDashOffset
    {
        get => GetLineDashOffset(Id);
        set => SetLineDashOffset(Id, value);
    }

    /// <summary>
    /// Gets the line dash offset of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current line dash offset.</returns>
    [JSImport($"{JsPath}canvas.getLineDashOffset")]
    private static partial int GetLineDashOffset(string id);

    /// <summary>
    /// Sets the line dash offset of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="offset">The line dash offset to set.</param>
    [JSImport($"{JsPath}canvas.setLineDashOffset")]
    private static partial void SetLineDashOffset(string id, int offset);

    #endregion

    #region MiterLimit

    /// <summary>
    /// Gets or sets the miter limit for line joins
    /// </summary>
    public int MiterLimit
    {
        get => GetMiterLimit(Id);
        set => SetMiterLimit(Id, value);
    }

    /// <summary>
    /// Gets the miter limit of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current miter limit.</returns>
    [JSImport($"{JsPath}canvas.getMiterLimit")]
    private static partial int GetMiterLimit(string id);

    /// <summary>
    /// Sets the miter limit of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="limit">The miter limit to set.</param>
    [JSImport($"{JsPath}canvas.setMiterLimit")]
    private static partial void SetMiterLimit(string id, int limit);

    #endregion

    #region LineDash

    /// <summary>
    /// Gets or sets the line dash pattern as an array of dash lengths
    /// </summary>
    public int[] LineDash
    {
        get => GetLineDash(Id).Split(',').Select(int.Parse).ToArray();
        set => SetLineDash(Id, string.Join(',', value));
    }

    /// <summary>
    /// Gets the line dash pattern of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current line dash pattern as a comma-separated string.</returns>
    [JSImport($"{JsPath}canvas.getLineDash")]
    private static partial string GetLineDash(string id);

    /// <summary>
    /// Sets the line dash pattern of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="dash">The line dash pattern as a comma-separated string.</param>
    [JSImport($"{JsPath}canvas.setLineDash")]
    private static partial void SetLineDash(string id, string dash);

    #endregion

    #region Font

    /// <summary>
    /// Gets or sets the font used for text rendering
    /// </summary>
    public string Font
    {
        get => GetFont(Id);
        set => SetFont(Id, value);
    }

    /// <summary>
    /// Gets the font of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current font setting.</returns>
    [JSImport($"{JsPath}canvas.getFont")]
    private static partial string GetFont(string id);

    /// <summary>
    /// Sets the font of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="font">The font to set.</param>
    [JSImport($"{JsPath}canvas.setFont")]
    private static partial void SetFont(string id, string font);

    #endregion

    #region TextAlign

    /// <summary>
    /// Gets or sets the horizontal text alignment
    /// </summary>
    public CanvasTextAlign TextAlign
    {
        get => GetTextAlign(Id).ParseEnum<CanvasTextAlign>();
        set => SetTextAlign(Id, value.ToString());
    }

    /// <summary>
    /// Gets the text alignment of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current text alignment.</returns>
    [JSImport($"{JsPath}canvas.getTextAlign")]
    private static partial string GetTextAlign(string id);

    /// <summary>
    /// Sets the text alignment of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="align">The text alignment to set.</param>
    [JSImport($"{JsPath}canvas.setTextAlign")]
    private static partial void SetTextAlign(string id, string align);

    #endregion

    #region TextAlign

    /// <summary>
    /// Gets or sets the vertical text baseline alignment
    /// </summary>
    public CanvasTextBaseline TextBaseline
    {
        get => GetTextBaseline(Id).ParseEnum<CanvasTextBaseline>();
        set => SetTextBaseline(Id, value.ToString());
    }

    /// <summary>
    /// Gets the text baseline of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <returns>The current text baseline.</returns>
    [JSImport($"{JsPath}canvas.getTextBaseline")]
    private static partial string GetTextBaseline(string id);

    /// <summary>
    /// Sets the text baseline of the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="baseline">The text baseline to set.</param>
    [JSImport($"{JsPath}canvas.setTextBaseline")]
    private static partial void SetTextBaseline(string id, string baseline);

    #endregion

    #region ClearRect

    /// <summary>
    /// Clears a rectangular area on the canvas
    /// </summary>
    /// <param name="x">The x-coordinate of the rectangle's top-left corner</param>
    /// <param name="y">The y-coordinate of the rectangle's top-left corner</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    public void ClearRect(int x, int y, int width, int height) => ClearRect(Id, x, y, width, height);

    /// <summary>
    /// Clears a rectangular area on the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="x">The x-coordinate of the rectangle's top-left corner.</param>
    /// <param name="y">The y-coordinate of the rectangle's top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    [JSImport($"{JsPath}canvas.clearRect")]
    private static partial void ClearRect(string id, int x, int y, int width, int height);

    #endregion

    #region FillRect

    /// <summary>
    /// Fills a rectangular area on the canvas
    /// </summary>
    /// <param name="x">The x-coordinate of the rectangle's top-left corner</param>
    /// <param name="y">The y-coordinate of the rectangle's top-left corner</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    public void FillRect(int x, int y, int width, int height) => FillRect(Id, x, y, width, height);

    /// <summary>
    /// Fills a rectangular area on the canvas.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="x">The x-coordinate of the rectangle's top-left corner.</param>
    /// <param name="y">The y-coordinate of the rectangle's top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    [JSImport($"{JsPath}canvas.fillRect")]
    private static partial void FillRect(string id, int x, int y, int width, int height);

    #endregion

    #region BeginPath

    /// <summary>
    /// Begins a new drawing path
    /// </summary>
    public void BeginPath() => BeginPath(Id);

    /// <summary>
    /// Begins a new drawing path.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.beginPath")]
    private static partial void BeginPath(string id);

    #endregion

    #region ClosePath

    /// <summary>
    /// Closes the current drawing path by drawing a line to the starting point
    /// </summary>
    public void ClosePath() => ClosePath(Id);

    /// <summary>
    /// Closes the current drawing path.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.closePath")]
    private static partial void ClosePath(string id);

    #endregion

    #region MoveTo

    /// <summary>
    /// Moves the drawing cursor to the specified coordinates without drawing
    /// </summary>
    /// <param name="x">The x-coordinate to move to</param>
    /// <param name="y">The y-coordinate to move to</param>
    public void MoveTo(float x, float y) => MoveTo(Id, x, y);

    /// <summary>
    /// Moves the drawing cursor to the specified coordinates.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="x">The x-coordinate to move to.</param>
    /// <param name="y">The y-coordinate to move to.</param>
    [JSImport($"{JsPath}canvas.moveTo")]
    private static partial void MoveTo(string id, float x, float y);

    #endregion

    #region LineTo

    /// <summary>
    /// Draws a line from the current cursor position to the specified coordinates
    /// </summary>
    /// <param name="x">The x-coordinate to draw to</param>
    /// <param name="y">The y-coordinate to draw to</param>
    public void LineTo(float x, float y) => LineTo(Id, x, y);

    /// <summary>
    /// Draws a line from the current cursor position to the specified coordinates.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="x">The x-coordinate to draw to.</param>
    /// <param name="y">The y-coordinate to draw to.</param>
    [JSImport($"{JsPath}canvas.lineTo")]
    private static partial void LineTo(string id, float x, float y);

    #endregion

    #region Arc

    /// <summary>
    /// Draws an arc or circle
    /// </summary>
    /// <param name="x">The x-coordinate of the arc's center</param>
    /// <param name="y">The y-coordinate of the arc's center</param>
    /// <param name="radius">The radius of the arc</param>
    /// <param name="startAngle">The starting angle in radians</param>
    /// <param name="endAngle">The ending angle in radians</param>
    /// <param name="antiClockwise">Whether to draw the arc counter-clockwise</param>
    public void Arc(float x, float y, float radius, float startAngle, float endAngle, bool antiClockwise) =>
        Arc(Id, x, y, radius, startAngle, endAngle, antiClockwise);

    /// <summary>
    /// Draws an arc or circle.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="x">The x-coordinate of the arc's center.</param>
    /// <param name="y">The y-coordinate of the arc's center.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="startAngle">The starting angle in radians.</param>
    /// <param name="endAngle">The ending angle in radians.</param>
    /// <param name="antiClockwise">Whether to draw the arc counter-clockwise.</param>
    [JSImport($"{JsPath}canvas.arc")]
    private static partial void Arc(
        string id,
        float x,
        float y,
        float radius,
        float startAngle,
        float endAngle,
        bool antiClockwise
    );

    #endregion

    #region Stroke

    /// <summary>
    /// Strokes (outlines) the current path
    /// </summary>
    public void Stroke() => Stroke(Id);

    /// <summary>
    /// Strokes (outlines) the current path.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.stroke")]
    private static partial void Stroke(string id);

    #endregion

    #region Fill

    /// <summary>
    /// Fills the current path
    /// </summary>
    public void Fill() => Fill(Id);

    /// <summary>
    /// Fills the current path.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.fill")]
    private static partial void Fill(string id);

    #endregion

    #region FillText

    /// <summary>
    /// Fills text at the specified position
    /// </summary>
    /// <param name="text">The text to draw</param>
    /// <param name="x">The x-coordinate of the text</param>
    /// <param name="y">The y-coordinate of the text</param>
    /// <param name="maxWidth">The maximum width of the text (optional)</param>
    public void FillText(string text, int x, int y, int maxWidth = 0) => FillText(Id, text, x, y, maxWidth);

    /// <summary>
    /// Fills text at the specified position.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="x">The x-coordinate of the text.</param>
    /// <param name="y">The y-coordinate of the text.</param>
    /// <param name="maxWidth">The maximum width of the text.</param>
    [JSImport($"{JsPath}canvas.fillText")]
    private static partial void FillText(string id, string text, int x, int y, int maxWidth);

    #endregion

    #region StrokeText

    /// <summary>
    /// Strokes (outlines) text at the specified position
    /// </summary>
    /// <param name="text">The text to draw</param>
    /// <param name="x">The x-coordinate of the text</param>
    /// <param name="y">The y-coordinate of the text</param>
    /// <param name="maxWidth">The maximum width of the text (optional)</param>
    public void StrokeText(string text, int x, int y, int maxWidth = 0) => StrokeText(Id, text, x, y, maxWidth);

    /// <summary>
    /// Strokes (outlines) text at the specified position.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="x">The x-coordinate of the text.</param>
    /// <param name="y">The y-coordinate of the text.</param>
    /// <param name="maxWidth">The maximum width of the text.</param>
    [JSImport($"{JsPath}canvas.strokeText")]
    private static partial void StrokeText(string id, string text, int x, int y, int maxWidth);

    #endregion

    #region MeasureTextWidth

    /// <summary>
    /// Measures the width of the specified text in pixels
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <returns>The width of the text in pixels</returns>
    public int MeasureTextWidth(string text) => MeasureTextWidth(Id, text);

    /// <summary>
    /// Measures the width of the specified text in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="text">The text to measure.</param>
    /// <returns>The width of the text in pixels.</returns>
    [JSImport($"{JsPath}canvas.measureTextWidth")]
    private static partial int MeasureTextWidth(string id, string text);

    #endregion

    #region MeasureTextHeight

    /// <summary>
    /// Measures the height of the specified text in pixels
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <returns>The height of the text in pixels</returns>
    public int MeasureTextHeight(string text) => MeasureTextHeight(Id, text);

    /// <summary>
    /// Measures the height of the specified text in pixels.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    /// <param name="text">The text to measure.</param>
    /// <returns>The height of the text in pixels.</returns>
    [JSImport($"{JsPath}canvas.measureTextHeight")]
    private static partial int MeasureTextHeight(string id, string text);

    #endregion

    #region Save

    /// <summary>
    /// Saves the current drawing state to a stack
    /// </summary>
    public void Save() => Save(Id);

    /// <summary>
    /// Saves the current drawing state to a stack.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.save")]
    private static partial void Save(string id);

    #endregion

    #region Restore

    /// <summary>
    /// Restores the most recently saved drawing state from the stack
    /// </summary>
    public void Restore() => Restore(Id);

    /// <summary>
    /// Restores the most recently saved drawing state from the stack.
    /// </summary>
    /// <param name="id">The canvas element identifier.</param>
    [JSImport($"{JsPath}canvas.restore")]
    private static partial void Restore(string id);

    #endregion

    /// <summary>
    /// Implicitly converts an ElementReference to a Canvas
    /// </summary>
    /// <param name="reference">The ElementReference to convert</param>
    /// <returns>A new Canvas instance</returns>
    public static implicit operator Canvas(ElementReference reference) => new(reference);
}

/// <summary>
/// Defines the line cap styles for canvas drawing
/// </summary>
public enum CanvasLineCap
{
    /// <summary>
    /// The line cap is a flat edge
    /// </summary>
    butt,

    /// <summary>
    /// The line cap is a rounded edge
    /// </summary>
    round,

    /// <summary>
    /// The line cap is a square edge
    /// </summary>
    square,
}

/// <summary>
/// Defines the line join styles for canvas drawing
/// </summary>
public enum CanvasLineJoin
{
    /// <summary>
    /// The line join is a beveled edge
    /// </summary>
    bevel,

    /// <summary>
    /// The line join is a mitered edge
    /// </summary>
    miter,

    /// <summary>
    /// The line join is a rounded edge
    /// </summary>
    round,
}

/// <summary>
/// Defines the text alignment options for canvas text rendering
/// </summary>
public enum CanvasTextAlign
{
    /// <summary>
    /// Text is aligned at the start of the text direction
    /// </summary>
    start,

    /// <summary>
    /// Text is aligned at the end of the text direction
    /// </summary>
    end,

    /// <summary>
    /// Text is aligned to the left
    /// </summary>
    left,

    /// <summary>
    /// Text is aligned to the right
    /// </summary>
    right,

    /// <summary>
    /// Text is centered
    /// </summary>
    center,
}

/// <summary>
/// Defines the text baseline alignment options for canvas text rendering
/// </summary>
public enum CanvasTextBaseline
{
    /// <summary>
    /// Text baseline is at the top of the em square
    /// </summary>
    top,

    /// <summary>
    /// Text baseline is at the hanging baseline
    /// </summary>
    hanging,

    /// <summary>
    /// Text baseline is at the middle of the em square
    /// </summary>
    middle,

    /// <summary>
    /// Text baseline is at the alphabetic baseline (default)
    /// </summary>
    alphabetic,

    /// <summary>
    /// Text baseline is at the ideographic baseline
    /// </summary>
    ideographic,

    /// <summary>
    /// Text baseline is at the bottom of the em square
    /// </summary>
    bottom,
}
