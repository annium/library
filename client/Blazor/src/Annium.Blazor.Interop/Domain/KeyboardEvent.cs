// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a keyboard event with key information and modifier states.
/// </summary>
/// <param name="Key">The key value of the pressed key.</param>
/// <param name="Code">The physical key code of the pressed key.</param>
/// <param name="MetaKey">Indicates whether the meta key (Command/Windows key) was pressed.</param>
/// <param name="CtrlKey">Indicates whether the control key was pressed.</param>
/// <param name="AltKey">Indicates whether the alt key was pressed.</param>
/// <param name="ShiftKey">Indicates whether the shift key was pressed.</param>
public readonly record struct KeyboardEvent(
    string Key,
    string Code,
    bool MetaKey,
    bool CtrlKey,
    bool AltKey,
    bool ShiftKey
);
