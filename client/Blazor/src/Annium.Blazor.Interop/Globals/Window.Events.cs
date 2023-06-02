using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public static partial class Window
{
    private static readonly IInteropEvent<KeyboardEvent> KeyboardEvent =
        InteropEvent<KeyboardEvent>.Static("window", "window");

    private static readonly IInteropEvent<ResizeEvent> ResizeEvent =
        InteropEvent<ResizeEvent>.Static("window", "window");

    public static Action OnKeyDown(Action<KeyboardEvent> handle, bool preventDefault) =>
        KeyboardEvent.Register(KeyboardEventEnum.keydown, handle, preventDefault);

    public static Action OnKeyUp(Action<KeyboardEvent> handle, bool preventDefault) =>
        KeyboardEvent.Register(KeyboardEventEnum.keyup, handle, preventDefault);

    public static Action OnResize(Action<ResizeEvent> handle) => ResizeEvent.Register("resize", handle);
}