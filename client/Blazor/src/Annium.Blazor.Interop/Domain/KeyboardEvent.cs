// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

public readonly record struct KeyboardEvent(
    string Key,
    string Code,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);