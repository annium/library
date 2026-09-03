using System;
using Annium.Blazor.Interop.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Provides event handling functionality for the browser window
/// </summary>
public static partial class Window
{
    /// <summary>
    /// Static interop event handler for keyboard events on the window
    /// </summary>
    private static readonly IInteropEvent<KeyboardEvent> _keyboardEvent = InteropEvent<KeyboardEvent>.Static(
        "window",
        "window"
    );

    /// <summary>
    /// Static interop event handler for resize events on the window
    /// </summary>
    private static readonly IInteropEvent<ResizeEvent> _resizeEvent = InteropEvent<ResizeEvent>.Static(
        "window",
        "window"
    );

    /// <summary>
    /// Registers a handler for the keydown event on the window
    /// </summary>
    /// <param name="handle">The callback to invoke when a key is pressed down</param>
    /// <param name="preventDefault">Whether to prevent the default browser behavior</param>
    /// <returns>An action that can be called to unregister the event handler</returns>
    public static Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);

    /// <summary>
    /// Registers a handler for the keyup event on the window
    /// </summary>
    /// <param name="handle">The callback to invoke when a key is released</param>
    /// <param name="preventDefault">Whether to prevent the default browser behavior</param>
    /// <returns>An action that can be called to unregister the event handler</returns>
    public static Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);

    /// <summary>
    /// Registers a handler for the resize event on the window
    /// </summary>
    /// <param name="handle">The callback to invoke when the window is resized</param>
    /// <returns>An action that can be called to unregister the event handler</returns>
    public static Action OnResize(Action<ResizeEvent> handle) => _resizeEvent.Register("resize", handle);
}
