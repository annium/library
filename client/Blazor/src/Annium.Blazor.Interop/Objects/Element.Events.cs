using System;
using Annium.Blazor.Interop.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Partial record providing event handling functionality for DOM elements
/// </summary>
public partial record Element
{
    /// <summary>
    /// Event handler for keyboard events
    /// </summary>
    private readonly IInteropEvent<KeyboardEvent> _keyboardEvent;

    /// <summary>
    /// Event handler for mouse events
    /// </summary>
    private readonly IInteropEvent<MouseEvent> _mouseEvent;

    /// <summary>
    /// Event handler for wheel events
    /// </summary>
    private readonly IInteropEvent<WheelEvent> _wheelEvent;

    /// <summary>
    /// Event handler for resize events
    /// </summary>
    private readonly IInteropEvent<ResizeEvent> _resizeEvent;

    /// <summary>
    /// Registers a handler for mouse down events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseDown(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mousedown, handle);

    /// <summary>
    /// Registers a handler for mouse up events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseUp(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseup, handle);

    /// <summary>
    /// Registers a handler for mouse enter events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseEnter(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseenter, handle);

    /// <summary>
    /// Registers a handler for mouse leave events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseLeave(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseleave, handle);

    /// <summary>
    /// Registers a handler for mouse over events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseOver(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseover, handle);

    /// <summary>
    /// Registers a handler for mouse out events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseOut(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mouseout, handle);

    /// <summary>
    /// Registers a handler for mouse move events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnMouseMove(Action<MouseEvent> handle) => _mouseEvent.Register(MouseEventEnum.mousemove, handle);

    /// <summary>
    /// Registers a handler for key down events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <param name="preventDefault">Whether to prevent the default browser behavior</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);

    /// <summary>
    /// Registers a handler for key up events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <param name="preventDefault">Whether to prevent the default browser behavior</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);

    /// <summary>
    /// Registers a handler for wheel events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnWheel(Action<WheelEvent> handle) => _wheelEvent.Register("wheel", handle);

    /// <summary>
    /// Registers a handler for resize events
    /// </summary>
    /// <param name="handle">The event handler to register</param>
    /// <returns>An action to unregister the event handler</returns>
    public Action OnResize(Action<ResizeEvent> handle) => _resizeEvent.Register("resize", handle);
}
