namespace Annium.Blazor.Interop.Domain;

public readonly record struct KeyboardEvent(
    char Key,
    char Code,
    bool MetaKey,
    bool ShiftKey,
    bool AltKey
);