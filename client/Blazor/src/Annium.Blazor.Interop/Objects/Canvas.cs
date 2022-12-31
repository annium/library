using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable InconsistentNaming

namespace Annium.Blazor.Interop;

public sealed partial record Canvas : ReferenceElement
{
    public Canvas(ElementReference reference) : base(reference)
    {
    }

    public int Width
    {
        get => Ctx.Invoke<string, int>("canvas.getWidth", Id);
        set => Ctx.Invoke("canvas.setWidth", Id, value);
    }

    public int Height
    {
        get => Ctx.Invoke<string, int>("canvas.getHeight", Id);
        set => Ctx.Invoke("canvas.setHeight", Id, value);
    }

    public string FillStyle
    {
        get => Ctx.Invoke<string, string>("canvas.getFillStyle", Id);
        set => Ctx.Invoke("canvas.setFillStyle", Id, value);
    }

    public string StrokeStyle
    {
        get => Ctx.Invoke<string, string>("canvas.getStrokeStyle", Id);
        set => Ctx.Invoke("canvas.setStrokeStyle", Id, value);
    }

    public CanvasLineCap LineCap
    {
        get => Ctx.Invoke<string, string>("canvas.getLineCap", Id).ParseEnum<CanvasLineCap>();
        set => Ctx.Invoke("canvas.setLineCap", Id, value.ToString());
    }

    public CanvasLineJoin LineJoin
    {
        get => Ctx.Invoke<string, string>("canvas.getLineJoin", Id).ParseEnum<CanvasLineJoin>();
        set => Ctx.Invoke("canvas.setLineJoin", Id, value.ToString());
    }

    public int LineWidth
    {
        get => Ctx.Invoke<string, int>("canvas.getLineWidth", Id);
        set => Ctx.Invoke("canvas.setLineWidth", Id, value);
    }

    public int LineDashOffset
    {
        get => Ctx.Invoke<string, int>("canvas.getLineDashOffset", Id);
        set => Ctx.Invoke("canvas.setLineDashOffset", Id, value);
    }

    public int MiterLimit
    {
        get => Ctx.Invoke<string, int>("canvas.getMiterLimit", Id);
        set => Ctx.Invoke("canvas.setMiterLimit", Id, value);
    }

    public int[] LineDash
    {
        get => Ctx.Invoke<string, string>("canvas.getLineDash", Id).Split(',').Select(int.Parse).ToArray();
        set => Ctx.Invoke("canvas.setLineDash", Id, string.Join(',', value));
    }

    public string Font
    {
        get => Ctx.Invoke<string, string>("canvas.getFont", Id);
        set => Ctx.Invoke("canvas.setFont", Id, value);
    }

    public CanvasTextAlign TextAlign
    {
        get => Ctx.Invoke<string, string>("canvas.getTextAlign", Id).ParseEnum<CanvasTextAlign>();
        set => Ctx.Invoke("canvas.setTextAlign", Id, value.ToString());
    }

    public CanvasTextBaseline TextBaseline
    {
        get => Ctx.Invoke<string, string>("canvas.getTextBaseline", Id).ParseEnum<CanvasTextBaseline>();
        set => Ctx.Invoke("canvas.setTextBaseline", Id, value.ToString());
    }

    public void ClearRect(int x, int y, int width, int height) =>
        Ctx.Invoke("canvas.clearRect", Id, x, y, width, height);

    public void FillRect(int x, int y, int width, int height) =>
        Ctx.Invoke("canvas.fillRect", Id, x, y, width, height);

    public void BeginPath() =>
        Ctx.Invoke("canvas.beginPath", Id);

    public void ClosePath() =>
        Ctx.Invoke("canvas.closePath", Id);

    public void MoveTo(float x, float y) =>
        Ctx.Invoke("canvas.moveTo", Id, x, y);

    public void LineTo(float x, float y) =>
        Ctx.Invoke("canvas.lineTo", Id, x, y);

    public void Arc(
        float x,
        float y,
        float radius,
        float startAngle,
        float endAngle,
        bool antiClockwise
    )
    {
        Ctx.Invoke("canvas.arc", Id, x, y, radius, startAngle, endAngle, antiClockwise ? 1 : 0);
    }

    public void Stroke() =>
        Ctx.Invoke("canvas.stroke", Id);

    public void Fill() =>
        Ctx.Invoke("canvas.fill", Id);

    public void FillText(string text, int x, int y, int maxWidth = 0) =>
        Ctx.Invoke("canvas.fillText", Id, text, x, y, maxWidth);

    public void StrokeText(string text, int x, int y, int maxWidth = 0) =>
        Ctx.Invoke("canvas.strokeText", Id, text, x, y, maxWidth);

    public int MeasureTextWidth(string text) =>
        Ctx.Invoke<string, string, int>("canvas.measureTextWidth", Id, text);

    public int MeasureTextHeight(string text) =>
        Ctx.Invoke<string, string, int>("canvas.measureTextHeight", Id, text);

    public void Save() => Save(Id);

    [JSImport($"{JsPath}canvas.save")]
    private static partial void Save(string id);

    public void Restore() =>
        Ctx.Invoke("canvas.restore", Id);

    public static implicit operator Canvas(ElementReference reference) => new(reference);
}

public enum CanvasLineCap
{
    butt,
    round,
    square
}

public enum CanvasLineJoin
{
    bevel,
    miter,
    round
}

public enum CanvasTextAlign
{
    start,
    end,
    left,
    right,
    center
}

public enum CanvasTextBaseline
{
    top,
    hanging,
    middle,
    alphabetic,
    ideographic,
    bottom
}