// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a resize event containing the new dimensions.
/// </summary>
/// <param name="Width">The new width in pixels.</param>
/// <param name="Height">The new height in pixels.</param>
public readonly record struct ResizeEvent(int Width, int Height);
