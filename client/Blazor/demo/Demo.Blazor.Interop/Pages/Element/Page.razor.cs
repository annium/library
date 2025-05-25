using System;
using Annium;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Element;

public partial class Page : ILogSubject, IDisposable
{
    [Inject]
    private Style Styles { get; set; } = null!;

    [Inject]
    public ILogger Logger { get; set; } = null!;

    private Div _eventsBlock = null!;
    private Div _resizedBlock = null!;
    private Input _input = null!;
    private bool _isResized;
    private DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += Window.OnResize(Handler<ResizeEvent>("resize", "window"));
        _disposable += _input;
        _input.OnKeyDown(Handler<KeyboardEvent>("keydown", "input"), false);
        _input.OnKeyUp(Handler<KeyboardEvent>("keyup", "input"), false);

        _disposable += _eventsBlock.OnMouseDown(Handler<MouseEvent>("mousedown", "block"));
        _disposable += _eventsBlock.OnMouseUp(Handler<MouseEvent>("mouseup", "block"));
        _disposable += _eventsBlock.OnMouseEnter(Handler<MouseEvent>("mouseenter", "block"));
        _disposable += _eventsBlock.OnMouseLeave(Handler<MouseEvent>("mouseleave", "block"));
        _disposable += _eventsBlock.OnMouseOver(Handler<MouseEvent>("mouseover", "block"));
        _disposable += _eventsBlock.OnMouseOut(Handler<MouseEvent>("mouseout", "block"));
        _disposable += _eventsBlock.OnMouseMove(Handler<MouseEvent>("mousemove", "block"));
        _disposable += _eventsBlock.OnWheel(Handler<WheelEvent>("wheel", "block"));

        _disposable += _resizedBlock.OnResize(Handler<ResizeEvent>("resize", "block"));
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

    private Action<T> Handler<T>(string ev, string code) => e => Console.WriteLine($"{code}.{ev}: {e}");

    private void ToggleResizedBlockSize() => _isResized = !_isResized;
}
