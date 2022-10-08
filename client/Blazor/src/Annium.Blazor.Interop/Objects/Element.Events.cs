using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;

namespace Annium.Blazor.Interop;

public partial record Element
{
    private readonly IInteropEvent<KeyboardEvent> _keyboardEvent;
    private readonly IInteropEvent<MouseEvent> _mouseEvent;
    private readonly IInteropEvent<WheelEvent> _wheelEvent;
    private readonly IInteropEvent<ResizeEvent> _resizeEvent;

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
    public Action OnResize(Action<ResizeEvent> handle) => _resizeEvent.Register("resize", handle);
}