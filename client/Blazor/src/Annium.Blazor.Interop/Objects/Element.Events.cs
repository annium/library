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
    private readonly InteropEvent<KeyboardEvent> _keyboardEvent;
    private readonly InteropEvent<MouseEvent> _mouseEvent;
    private readonly InteropEvent<WheelEvent> _wheelEvent;
    private readonly InteropOldEvent<WheelEvent> _wheelOldEvent = new();
    private readonly Dictionary<MouseEventEnum, InteropOldEvent<MouseEvent>> _mouseEvents = new();
    private readonly Dictionary<KeyboardEventEnum, InteropOldEvent<KeyboardEvent>> _keyboardEvents = new();

    public Action OnMouseDown(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mousedown, handle);
    public Action OnMouseUp(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseup, handle);
    public Action OnMouseEnter(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseenter, handle);
    public Action OnMouseLeave(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseleave, handle);
    public Action OnMouseOver(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseover, handle);
    public Action OnMouseOut(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mouseout, handle);
    public Action OnMouseMove(Action<MouseEvent> handle) => OnMouseEvent(MouseEventEnum.mousemove, handle);
    public Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) => _keyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);
    public Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) => _keyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);

    public Action OnWheel(Action<WheelEvent> handle)
    {
        if (!_wheelOldEvent.HasListeners)
            _wheelOldEvent.SetCallbackId(Ctx.Invoke<int>("element.onWheelEvent", Id, _ref, $"{nameof(Element)}.{nameof(HandleWheelEvent)}"));

        _wheelOldEvent.Event += handle;

        return () =>
        {
            _wheelOldEvent.Event -= handle;

            if (_wheelOldEvent.HasListeners)
                return;

            Ctx.InvokeVoid("element.offWheelEvent", Id, _wheelOldEvent.CallbackId);
            _wheelOldEvent.ResetCallbackId();
        };
    }

    private Action OnMouseEvent(MouseEventEnum type, Action<MouseEvent> handle)
    {
        if (!_mouseEvents.TryGetValue(type, out var e))
            e = _mouseEvents[type] = new InteropOldEvent<MouseEvent>();

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
            e = _keyboardEvents[type] = new InteropOldEvent<KeyboardEvent>();

        if (!e.HasListeners)
            e.SetCallbackId(Ctx.Invoke<int>(
                    "element.onKeyboardEvent",
                    Id,
                    type.ToString(),
                    _ref,
                    $"{nameof(Element)}.{nameof(HandleKeyboardEvent)}",
                    preventDefault
                )
            );

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
    public void HandleMouseEvent(string typeName, int x, int y)
    {
        var type = typeName.ParseEnum<MouseEventEnum>();
        if (!_mouseEvents.TryGetValue(type, out var e))
            throw new InvalidOperationException($"No {type} event handlers registered");

        e.Handle(new MouseEvent(x, y));
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleWheelEvent)}")]
    public void HandleWheelEvent(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        _wheelOldEvent.Handle(new WheelEvent(ctrlKey, deltaX, deltaY));
    }
}