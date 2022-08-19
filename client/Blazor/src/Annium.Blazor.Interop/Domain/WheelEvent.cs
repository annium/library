namespace Annium.Blazor.Interop.Domain;

public readonly record struct WheelEvent(
    bool CtrlKey,
    decimal DeltaX,
    decimal DeltaY
);