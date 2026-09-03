// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a mouse event with position coordinates and modifier states.
/// </summary>
/// <param name="X">The X coordinate of the mouse event.</param>
/// <param name="Y">The Y coordinate of the mouse event.</param>
/// <param name="MetaKey">Indicates whether the meta key (Command/Windows key) was pressed.</param>
/// <param name="CtrlKey">Indicates whether the control key was pressed.</param>
/// <param name="AltKey">Indicates whether the alt key was pressed.</param>
/// <param name="ShiftKey">Indicates whether the shift key was pressed.</param>
public readonly record struct MouseEvent(int X, int Y, bool MetaKey, bool CtrlKey, bool AltKey, bool ShiftKey);
