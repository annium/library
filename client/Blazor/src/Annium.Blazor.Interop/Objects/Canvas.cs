using System.Linq;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;

// ReSharper disable InconsistentNaming

namespace Annium.Blazor.Interop;

public sealed record Canvas : ReferenceElement
{
    public Canvas(ElementReference reference) : base(reference)
    {
    }

    public int Width
    {
        get => Ctx.UInvoke<string, int>("canvas.getWidth", Id);
        set => Ctx.UInvokeVoid("canvas.setWidth", Id, value);
    }

    public int Height
    {
        get => Ctx.UInvoke<string, int>("canvas.getHeight", Id);
        set => Ctx.UInvokeVoid("canvas.setHeight", Id, value);
    }

    public string FillStyle
    {
        get => Ctx.UInvoke<string, string>("canvas.getFillStyle", Id);
        set => Ctx.UInvokeVoid("canvas.setFillStyle", Id, value);
    }

    public string StrokeStyle
    {
        get => Ctx.UInvoke<string, string>("canvas.getStrokeStyle", Id);
        set => Ctx.UInvokeVoid("canvas.setStrokeStyle", Id, value);
    }

    public CanvasLineCap LineCap
    {
        get => Ctx.UInvoke<string, string>("canvas.getLineCap", Id).ParseEnum<CanvasLineCap>();
        set => Ctx.UInvokeVoid("canvas.setLineCap", Id, value.ToString());
    }

    public CanvasLineJoin LineJoin
    {
        get => Ctx.UInvoke<string, string>("canvas.getLineJoin", Id).ParseEnum<CanvasLineJoin>();
        set => Ctx.UInvokeVoid("canvas.setLineJoin", Id, value.ToString());
    }

    public int LineWidth
    {
        get => Ctx.UInvoke<string, int>("canvas.getLineWidth", Id);
        set => Ctx.UInvokeVoid("canvas.setLineWidth", Id, value);
    }

    public int LineDashOffset
    {
        get => Ctx.UInvoke<string, int>("canvas.getLineDashOffset", Id);
        set => Ctx.UInvokeVoid("canvas.setLineDashOffset", Id, value);
    }

    public int MiterLimit
    {
        get => Ctx.UInvoke<string, int>("canvas.getMiterLimit", Id);
        set => Ctx.UInvokeVoid("canvas.setMiterLimit", Id, value);
    }

    public int[] LineDash
    {
        get => Ctx.UInvoke<string, string>("canvas.getLineDash", Id).Split(',').Select(int.Parse).ToArray();
        set => Ctx.UInvokeVoid("canvas.setLineDash", Id, string.Join(',', value));
    }

    public string Font
    {
        get => Ctx.UInvoke<string, string>("canvas.getFont", Id);
        set => Ctx.UInvokeVoid("canvas.setFont", Id, value);
    }

    public CanvasTextAlign TextAlign
    {
        get => Ctx.UInvoke<string, string>("canvas.getTextAlign", Id).ParseEnum<CanvasTextAlign>();
        set => Ctx.UInvokeVoid("canvas.setTextAlign", Id, value.ToString());
    }

    public CanvasTextBaseline TextBaseline
    {
        get => Ctx.UInvoke<string, string>("canvas.getTextBaseline", Id).ParseEnum<CanvasTextBaseline>();
        set => Ctx.UInvokeVoid("canvas.setTextBaseline", Id, value.ToString());
    }

    public void ClearRect(int x, int y, int width, int height) =>
        Ctx.UInvokeVoid("canvas.clearRect", Id, x, y, width, height);

    public void FillRect(int x, int y, int width, int height) =>
        Ctx.UInvokeVoid("canvas.fillRect", Id, x, y, width, height);

    public void BeginPath() =>
        Ctx.UInvokeVoid("canvas.beginPath", Id);

    public void ClosePath() =>
        Ctx.UInvokeVoid("canvas.closePath", Id);

    public void MoveTo(float x, float y) =>
        Ctx.UInvokeVoid("canvas.moveTo", Id, x, y);

    public void LineTo(float x, float y) =>
        Ctx.UInvokeVoid("canvas.lineTo", Id, x, y);

    public void Stroke() =>
        Ctx.UInvokeVoid("canvas.stroke", Id);

    public void FillText(string text, int x, int y, int maxWidth = 0) =>
        Ctx.UInvokeVoid("canvas.fillText", Id, text, x, y, maxWidth);

    public void StrokeText(string text, int x, int y, int maxWidth = 0) =>
        Ctx.UInvokeVoid("canvas.strokeText", Id, text, x, y, maxWidth);

    public int MeasureText(string text) =>
        Ctx.UInvoke<string, string, int>("canvas.measureText", Id, text);

    public void Save() =>
        Ctx.UInvokeVoid("canvas.save", Id);

    public void Restore() =>
        Ctx.UInvokeVoid("canvas.restore", Id);

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