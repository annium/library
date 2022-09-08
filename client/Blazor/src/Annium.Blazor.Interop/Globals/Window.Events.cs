using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;

namespace Annium.Blazor.Interop;

public static partial class Window
{
    private static readonly WindowInteropEvent<ResizeEvent> ResizeEvent = new();

    public static Action OnResize(Action<ResizeEvent> handle) => ResizeEvent.Register("resize", handle);
}