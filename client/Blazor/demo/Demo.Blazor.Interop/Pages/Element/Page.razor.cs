using System;
using Annium.Blazor.Interop;
using Annium.Blazor.Interop.Domain;
using Annium.Core.Primitives;
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
    private Input _input = default!;
    private DisposableBox _disposable = Disposable.Box();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += _input;
        _input.OnKeyDown(Handler<KeyboardEvent>("keydown", "input"), true);
        _input.OnKeyUp(Handler<KeyboardEvent>("keyup", "input"), true);

        _disposable += _eventsBlock.OnMouseDown(Handler<MouseEvent>("mousedown", "block"));
        _disposable += _eventsBlock.OnMouseUp(Handler<MouseEvent>("mouseup", "block"));
        _disposable += _eventsBlock.OnMouseEnter(Handler<MouseEvent>("mouseenter", "block"));
        _disposable += _eventsBlock.OnMouseLeave(Handler<MouseEvent>("mouseleave", "block"));
        _disposable += _eventsBlock.OnMouseOver(Handler<MouseEvent>("mouseover", "block"));
        _disposable += _eventsBlock.OnMouseOut(Handler<MouseEvent>("mouseout", "block"));
        _disposable += _eventsBlock.OnMouseMove(Handler<MouseEvent>("mousemove", "block"));
        _disposable += _eventsBlock.OnWheel(Handler<WheelEvent>("wheel", "block"));
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

    private Action<T> Handler<T>(string ev, string code) => e => Console.WriteLine($"{code}.{ev}: {e}");
}