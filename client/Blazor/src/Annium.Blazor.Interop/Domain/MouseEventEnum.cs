// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Enumeration of mouse event types for JavaScript interop.
/// </summary>
public enum MouseEventEnum
{
    /// <summary>
    /// Fired when a mouse button is pressed down on an element.
    /// </summary>
    mousedown,

    /// <summary>
    /// Fired when a mouse button is released over an element.
    /// </summary>
    mouseup,

    /// <summary>
    /// Fired when a mouse pointer enters an element.
    /// </summary>
    mouseenter,

    /// <summary>
    /// Fired when a mouse pointer leaves an element.
    /// </summary>
    mouseleave,

    /// <summary>
    /// Fired when a mouse pointer moves over an element.
    /// </summary>
    mouseover,

    /// <summary>
    /// Fired when a mouse pointer moves out of an element.
    /// </summary>
    mouseout,

    /// <summary>
    /// Fired when a mouse pointer moves within an element.
    /// </summary>
    mousemove,
}
