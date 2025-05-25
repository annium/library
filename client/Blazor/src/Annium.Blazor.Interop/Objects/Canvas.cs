using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public sealed partial record Canvas : ReferenceElement
{
    public Canvas(ElementReference reference)
        : base(reference) { }

    #region Width

    public int Width
    {
        get => GetWidth(Id);
        set => SetWidth(Id, value);
    }

    [JSImport($"{JsPath}canvas.getWidth")]
    private static partial int GetWidth(string id);

    [JSImport($"{JsPath}canvas.setWidth")]
    private static partial void SetWidth(string id, int width);

    #endregion

    #region Height

    public int Height
    {
        get => GetHeight(Id);
        set => SetHeight(Id, value);
    }

    [JSImport($"{JsPath}canvas.getHeight")]
    private static partial int GetHeight(string id);

    [JSImport($"{JsPath}canvas.setHeight")]
    private static partial void SetHeight(string id, int height);

    #endregion

    #region FillStyle

    public string FillStyle
    {
        get => GetFillStyle(Id);
        set => SetFillStyle(Id, value);
    }

    [JSImport($"{JsPath}canvas.getFillStyle")]
    private static partial string GetFillStyle(string id);

    [JSImport($"{JsPath}canvas.setFillStyle")]
    private static partial void SetFillStyle(string id, string style);

    #endregion

    #region StrokeStyle

    public string StrokeStyle
    {
        get => GetStrokeStyle(Id);
        set => SetStrokeStyle(Id, value);
    }

    [JSImport($"{JsPath}canvas.getStrokeStyle")]
    private static partial string GetStrokeStyle(string id);

    [JSImport($"{JsPath}canvas.setStrokeStyle")]
    private static partial void SetStrokeStyle(string id, string style);

    #endregion

    #region LineCap

    public CanvasLineCap LineCap
    {
        get => GetLineCap(Id).ParseEnum<CanvasLineCap>();
        set => SetLineCap(Id, value.ToString());
    }

    [JSImport($"{JsPath}canvas.getLineCap")]
    private static partial string GetLineCap(string id);

    [JSImport($"{JsPath}canvas.setLineCap")]
    private static partial void SetLineCap(string id, string style);

    #endregion

    #region LineJoin

    public CanvasLineJoin LineJoin
    {
        get => GetLineJoin(Id).ParseEnum<CanvasLineJoin>();
        set => SetLineJoin(Id, value.ToString());
    }

    [JSImport($"{JsPath}canvas.getLineJoin")]
    private static partial string GetLineJoin(string id);

    [JSImport($"{JsPath}canvas.setLineJoin")]
    private static partial void SetLineJoin(string id, string style);

    #endregion

    #region LineWidth

    public int LineWidth
    {
        get => GetLineWidth(Id);
        set => SetLineWidth(Id, value);
    }

    [JSImport($"{JsPath}canvas.getLineWidth")]
    private static partial int GetLineWidth(string id);

    [JSImport($"{JsPath}canvas.setLineWidth")]
    private static partial void SetLineWidth(string id, int width);

    #endregion

    #region LineDashOffset

    public int LineDashOffset
    {
        get => GetLineDashOffset(Id);
        set => SetLineDashOffset(Id, value);
    }

    [JSImport($"{JsPath}canvas.getLineDashOffset")]
    private static partial int GetLineDashOffset(string id);

    [JSImport($"{JsPath}canvas.setLineDashOffset")]
    private static partial void SetLineDashOffset(string id, int offset);

    #endregion

    #region MiterLimit

    public int MiterLimit
    {
        get => GetMiterLimit(Id);
        set => SetMiterLimit(Id, value);
    }

    [JSImport($"{JsPath}canvas.getMiterLimit")]
    private static partial int GetMiterLimit(string id);

    [JSImport($"{JsPath}canvas.setMiterLimit")]
    private static partial void SetMiterLimit(string id, int limit);

    #endregion

    #region LineDash

    public int[] LineDash
    {
        get => GetLineDash(Id).Split(',').Select(int.Parse).ToArray();
        set => SetLineDash(Id, string.Join(',', value));
    }

    [JSImport($"{JsPath}canvas.getLineDash")]
    private static partial string GetLineDash(string id);

    [JSImport($"{JsPath}canvas.setLineDash")]
    private static partial void SetLineDash(string id, string dash);

    #endregion

    #region Font

    public string Font
    {
        get => GetFont(Id);
        set => SetFont(Id, value);
    }

    [JSImport($"{JsPath}canvas.getFont")]
    private static partial string GetFont(string id);

    [JSImport($"{JsPath}canvas.setFont")]
    private static partial void SetFont(string id, string font);

    #endregion

    #region TextAlign

    public CanvasTextAlign TextAlign
    {
        get => GetTextAlign(Id).ParseEnum<CanvasTextAlign>();
        set => SetTextAlign(Id, value.ToString());
    }

    [JSImport($"{JsPath}canvas.getTextAlign")]
    private static partial string GetTextAlign(string id);

    [JSImport($"{JsPath}canvas.setTextAlign")]
    private static partial void SetTextAlign(string id, string align);

    #endregion

    #region TextAlign

    public CanvasTextBaseline TextBaseline
    {
        get => GetTextBaseline(Id).ParseEnum<CanvasTextBaseline>();
        set => SetTextBaseline(Id, value.ToString());
    }

    [JSImport($"{JsPath}canvas.getTextBaseline")]
    private static partial string GetTextBaseline(string id);

    [JSImport($"{JsPath}canvas.setTextBaseline")]
    private static partial void SetTextBaseline(string id, string baseline);

    #endregion

    #region ClearRect

    public void ClearRect(int x, int y, int width, int height) => ClearRect(Id, x, y, width, height);

    [JSImport($"{JsPath}canvas.clearRect")]
    private static partial void ClearRect(string id, int x, int y, int width, int height);

    #endregion

    #region FillRect

    public void FillRect(int x, int y, int width, int height) => FillRect(Id, x, y, width, height);

    [JSImport($"{JsPath}canvas.fillRect")]
    private static partial void FillRect(string id, int x, int y, int width, int height);

    #endregion

    #region BeginPath

    public void BeginPath() => BeginPath(Id);

    [JSImport($"{JsPath}canvas.beginPath")]
    private static partial void BeginPath(string id);

    #endregion

    #region ClosePath

    public void ClosePath() => ClosePath(Id);

    [JSImport($"{JsPath}canvas.closePath")]
    private static partial void ClosePath(string id);

    #endregion

    #region MoveTo

    public void MoveTo(float x, float y) => MoveTo(Id, x, y);

    [JSImport($"{JsPath}canvas.moveTo")]
    private static partial void MoveTo(string id, float x, float y);

    #endregion

    #region LineTo

    public void LineTo(float x, float y) => LineTo(Id, x, y);

    [JSImport($"{JsPath}canvas.lineTo")]
    private static partial void LineTo(string id, float x, float y);

    #endregion

    #region Arc

    public void Arc(float x, float y, float radius, float startAngle, float endAngle, bool antiClockwise) =>
        Arc(Id, x, y, radius, startAngle, endAngle, antiClockwise);

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

    public void Stroke() => Stroke(Id);

    [JSImport($"{JsPath}canvas.stroke")]
    private static partial void Stroke(string id);

    #endregion

    #region Fill

    public void Fill() => Fill(Id);

    [JSImport($"{JsPath}canvas.fill")]
    private static partial void Fill(string id);

    #endregion

    #region FillText

    public void FillText(string text, int x, int y, int maxWidth = 0) => FillText(Id, text, x, y, maxWidth);

    [JSImport($"{JsPath}canvas.fillText")]
    private static partial void FillText(string id, string text, int x, int y, int maxWidth);

    #endregion

    #region StrokeText

    public void StrokeText(string text, int x, int y, int maxWidth = 0) => StrokeText(Id, text, x, y, maxWidth);

    [JSImport($"{JsPath}canvas.strokeText")]
    private static partial void StrokeText(string id, string text, int x, int y, int maxWidth);

    #endregion

    #region MeasureTextWidth

    public int MeasureTextWidth(string text) => MeasureTextWidth(Id, text);

    [JSImport($"{JsPath}canvas.measureTextWidth")]
    private static partial int MeasureTextWidth(string id, string text);

    #endregion

    #region MeasureTextHeight

    public int MeasureTextHeight(string text) => MeasureTextHeight(Id, text);

    [JSImport($"{JsPath}canvas.measureTextHeight")]
    private static partial int MeasureTextHeight(string id, string text);

    #endregion

    #region Save

    public void Save() => Save(Id);

    [JSImport($"{JsPath}canvas.save")]
    private static partial void Save(string id);

    #endregion

    #region Restore

    public void Restore() => Restore(Id);

    [JSImport($"{JsPath}canvas.restore")]
    private static partial void Restore(string id);

    #endregion

    public static implicit operator Canvas(ElementReference reference) => new(reference);
}

public enum CanvasLineCap
{
    butt,
    round,
    square,
}

public enum CanvasLineJoin
{
    bevel,
    miter,
    round,
}

public enum CanvasTextAlign
{
    start,
    end,
    left,
    right,
    center,
}

public enum CanvasTextBaseline
{
    top,
    hanging,
    middle,
    alphabetic,
    ideographic,
    bottom,
}
