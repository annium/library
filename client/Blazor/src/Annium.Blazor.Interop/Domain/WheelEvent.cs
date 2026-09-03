// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a wheel event containing scroll delta values and modifier key states.
/// </summary>
/// <param name="DeltaX">The horizontal scroll delta value.</param>
/// <param name="DeltaY">The vertical scroll delta value.</param>
/// <param name="MetaKey">Whether the meta key (Cmd on Mac, Windows key on PC) was pressed.</param>
/// <param name="CtrlKey">Whether the Ctrl key was pressed.</param>
/// <param name="AltKey">Whether the Alt key was pressed.</param>
/// <param name="ShiftKey">Whether the Shift key was pressed.</param>
public readonly record struct WheelEvent(
    decimal DeltaX,
    decimal DeltaY,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);
