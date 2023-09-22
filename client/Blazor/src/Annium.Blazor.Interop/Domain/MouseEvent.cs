// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

public readonly record struct MouseEvent(
    int X,
    int Y,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);