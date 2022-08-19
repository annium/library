namespace Annium.Blazor.Interop.Domain;

public readonly record struct KeyboardEvent(
    string Key,
    string Code,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);