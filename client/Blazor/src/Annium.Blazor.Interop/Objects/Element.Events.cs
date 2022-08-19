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
    private readonly InteropEvent<bool, decimal, decimal> _wheelEvent = new();
    private readonly Dictionary<MouseEventEnum, InteropEvent<int, int>> _mouseEvents = new();

    public Action OnMouseDown(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mousedown, handle);
    public Action OnMouseUp(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mouseup, handle);
    public Action OnMouseEnter(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mouseenter, handle);
    public Action OnMouseLeave(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mouseleave, handle);
    public Action OnMouseOver(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mouseover, handle);
    public Action OnMouseOut(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mouseout, handle);
    public Action OnMouseMove(Action<int, int> handle) => OnMouseEvent(MouseEventEnum.mousemove, handle);

    public Action OnWheel(Action<bool, decimal, decimal> handle)
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

    private Action OnMouseEvent(MouseEventEnum type, Action<int, int> handle)
    {
        if (!_mouseEvents.TryGetValue(type, out var e))
            e = _mouseEvents[type] = new InteropEvent<int, int>();

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

    [JSInvokable($"{nameof(Element)}.{nameof(HandleMouseEvent)}")]
    public void HandleMouseEvent(string typeName, int x, int y)
    {
        var type = typeName.ParseEnum<MouseEventEnum>();
        if (!_mouseEvents.TryGetValue(type, out var e))
            throw new InvalidOperationException($"No {type} event handlers registered");

        e.Handle(x, y);
    }

    [JSInvokable($"{nameof(Element)}.{nameof(HandleWheelEvent)}")]
    public void HandleWheelEvent(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        _wheelEvent.Handle(ctrlKey, deltaX, deltaY);
    }
}