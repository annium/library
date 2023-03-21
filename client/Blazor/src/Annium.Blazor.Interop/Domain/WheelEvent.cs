namespace Annium.Blazor.Interop.Domain;

public readonly record struct WheelEvent(
    decimal DeltaX,
    decimal DeltaY,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);