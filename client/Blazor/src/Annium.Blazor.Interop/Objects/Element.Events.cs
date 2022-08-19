using System;
using System.Collections.Generic;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public partial record Element
{
    private readonly InteropEvent<WheelEvent> _wheelEvent = new();
    private readonly Dictionary<MouseEventEnum, InteropEvent<MouseEvent>> _mouseEvents = new();
    private readonly Dictionary<KeyboardEventEnum, InteropEvent<KeyboardEvent>> _keyboardEvents = new();

    public Action OnMouseDown(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mousedown, handle);
    public Action OnMouseUp(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseup, handle);
    public Action OnMouseEnter(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseenter, handle);
    public Action OnMouseLeave(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseleave, handle);
    public Action OnMouseOver(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseover, handle);
    public Action OnMouseOut(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseout, handle);
    public Action OnMouseMove(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mousemove, handle);
    public Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) => OnKeyboardEvent(KeyboardEventEnum.keydown, handle, preventDefault);
    public Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) => OnKeyboardEvent(KeyboardEventEnum.keyup, handle, preventDefault);

    public Action OnWheel(Action<WheelEvent> handle)
    {
        if (!_wheelEvent.HasListeners)
            _wheelEvent.SetCallbackId(Ctx.Invoke<int>("element.onWheelEvent", Id, _ref, $"{nameof(Element)}.{nameof(HandleWheelEvent)}"));

        _wheelEvent.Event += handle;

        return () =>
        {
            _wheelEvent.Event -= handle;

            if (_wheelEvent.HasListeners)
                return;

            Ctx.InvokeVoid("element.offWheelEvent", Id, _wheelEvent.CallbackId);
            _wheelEvent.ResetCallbackId();
        };
    }

    private Action OnMouseEvent(MouseEventEnum type, Action<MouseEvent> handle)
    {
        if (!_mouseEvents.TryGetValue(type, out var e))
            e = _mouseEvents[type] = new InteropEvent<MouseEvent>();

        if (!e.HasListeners)
            e.SetCallbackId(Ctx.Invoke<int>("element.onMouseEvent", Id, type.ToString(), _ref, $"{nameof(Element)}.{nameof(HandleMouseEvent)}"));

        e.Event += handle;

        return () =>
        {
            e.Event -= handle;

            if (e.HasListeners)
                return;

            _mouseEvents.Remove(type);
            Ctx.InvokeVoid("element.offMouseEvent", Id, type.ToString(), e.CallbackId);
            e.ResetCallbackId();
        };
    }

    private Action OnKeyboardEvent(KeyboardEventEnum type, Action<KeyboardEvent> handle, bool preventDefault)
    {
        if (!_keyboardEvents.TryGetValue(type, out var e))
            e = _keyboardEvents[type] = new InteropEvent<KeyboardEvent>();

        if (!e.HasListeners)
            e.SetCallbackId(Ctx.Invoke<int>(
                "element.onKeyboardEvent",
                Id,
                type.ToString(),
                _ref,
                $"{nameof(Element)}.{nameof(HandleKeyboardEvent)}",
                preventDefault
            ));

        e.Event += handle;

        return () =>
        {
            e.Event -= handle;

            if (e.HasListeners)
                return;

            _keyboardEvents.Remove(type);
            Ctx.InvokeVoid("element.offKeyboardEvent", Id, type.ToString(), e.CallbackId);
            e.ResetCallbackId();
        };
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleMouseEvent)}")]
    public void HandleMouseEvent(string typeName, int x, int y)
    {
        var type = typeName.ParseEnum<MouseEventEnum>();
        if (!_mouseEvents.TryGetValue(type, out var e))
            throw new InvalidOperationException($"No {type} event handlers registered");

        e.Handle(new MouseEvent(x, y));
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleKeyboardEvent)}")]
    public void HandleKeyboardEvent(string typeName, string key, string code, bool metaKey, bool shiftKey, bool altKey)
    {
        var type = typeName.ParseEnum<KeyboardEventEnum>();
        if (!_keyboardEvents.TryGetValue(type, out var e))
            throw new InvalidOperationException($"No {type} event handlers registered");

        e.Handle(new KeyboardEvent(key, code, metaKey, shiftKey, altKey));
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleWheelEvent)}")]
    public void HandleWheelEvent(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        _wheelEvent.Handle(new WheelEvent(ctrlKey, deltaX, deltaY));
    }
}