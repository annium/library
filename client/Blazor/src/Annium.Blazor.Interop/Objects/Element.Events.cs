using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public partial record Element
{
    private readonly InteropEvent<KeyboardEvent> _keyboardEvent;
    private readonly InteropEvent<MouseEvent> _mouseEvent;
    private readonly InteropEvent<WheelEvent> _wheelEvent;

    public Action OnMouseDown(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mousedown, handle);
    public Action OnMouseUp(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseup, handle);
    public Action OnMouseEnter(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseenter, handle);
    public Action OnMouseLeave(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseleave, handle);
    public Action OnMouseOver(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseover, handle);
    public Action OnMouseOut(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseout, handle);
    public Action OnMouseMove(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mousemove, handle);
    public Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) => _keyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);
    public Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) => _keyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);
    public Action OnWheel(Action<WheelEvent> handle) => _wheelEvent.Register("wheel", handle);

    [JSInvokable($"{nameof(Element)}.{nameof(HandleKeyboardEvent)}")]
    public void HandleKeyboardEvent(
        int callbackId,
        string key,
        string code,
        bool metaKey,
        bool ctrlKey,
        bool altKey,
        bool shiftKey
    )
    {
        _keyboardEvent.Handle(callbackId, new KeyboardEvent(key, code, metaKey, ctrlKey, altKey, shiftKey));
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleMouseEvent)}")]
    public void HandleMouseEvent(int callbackId, int x, int y)
    {
        _mouseEvent.Handle(callbackId, new MouseEvent(x, y));
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleWheelEvent)}")]
    public void HandleWheelEvent(
        int callbackId,
        bool ctrlKey,
        decimal deltaX,
        decimal deltaY
    )
    {
        _wheelEvent.Handle(callbackId, new WheelEvent(ctrlKey, deltaX, deltaY));
    }
}