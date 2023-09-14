// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public readonly record struct WheelEvent(
    decimal DeltaX,
    decimal DeltaY,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);