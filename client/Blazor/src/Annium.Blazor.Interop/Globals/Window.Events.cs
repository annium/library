using System;
using Annium.Blazor.Interop.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public static partial class Window
{
    private static readonly IInteropEvent<KeyboardEvent> _keyboardEvent = InteropEvent<KeyboardEvent>.Static(
        "window",
        "window"
    );

    private static readonly IInteropEvent<ResizeEvent> _resizeEvent = InteropEvent<ResizeEvent>.Static(
        "window",
        "window"
    );

    public static Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);

    public static Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) =>
        _keyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);

    public static Action OnResize(Action<ResizeEvent> handle) => _resizeEvent.Register("resize", handle);
}
