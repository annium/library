namespace Annium.Blazor.Interop.Domain;

public readonly record struct MouseEvent(
    int X,
    int Y,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);