using System;
using Annium;
using Annium.Blazor.Interop;
using Annium.Blazor.Interop.Domain;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Element;

public partial class Page : ILogSubject<Canvas.Page>, IDisposable
{
    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger<Canvas.Page> Logger { get; set; } = default!;

    private Div _container = default!;
    private Div _block = default!;
    private Div _eventsBlock = default!;
    private Div _resizedBlock = default!;
    private Input _input = default!;
    private bool IsResized;
    private DisposableBox _disposable = Disposable.Box();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

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

    private void ToggleResizedBlockSize() => IsResized = !IsResized;
}