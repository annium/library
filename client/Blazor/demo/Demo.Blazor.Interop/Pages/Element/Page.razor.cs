using System;
using Annium;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Demo.Blazor.Interop.Pages.Element;

/// <summary>
/// Demonstrates element interactions and event handling in Blazor.
/// </summary>
public partial class Page : ILogSubject, IDisposable
{
    /// <summary>
    /// Gets or sets the CSS styles for the page.
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// The div element that handles mouse events.
    /// </summary>
    private Div _eventsBlock = null!;

    /// <summary>
    /// The div element that handles resize events.
    /// </summary>
    private Div _resizedBlock = null!;

    /// <summary>
    /// The input element that handles keyboard events.
    /// </summary>
    private Input _input = null!;

    /// <summary>
    /// Indicates whether the resized block is currently in its enlarged state.
    /// </summary>
    private bool _isResized;

    /// <summary>
    /// Container for disposable event subscriptions.
    /// </summary>
    private DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    /// <summary>
    /// Sets up event handlers after the component is rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += Window.OnResize(Handler<ResizeEvent>("resize", "window"));
        _disposable += _input;
        _input.OnKeyDown(Handler<KeyboardEvent>("keydown", "input"), false);
        _input.OnKeyUp(Handler<KeyboardEvent>("keyup", "input"), false);

        _disposable += _eventsBlock.OnMouseDown(Handler<MouseEvent>("mousedown", "block"));
        _disposable += _eventsBlock.OnMouseUp(Handler<MouseEvent>("mouseup", "block"));
        _disposable += _eventsBlock.OnMouseEnter(Handler<MouseEvent>("mouseenter", "block"));
        _disposable += _eventsBlock.OnMouseLeave(Handler<MouseEvent>("mouseleave", "block"));
        _disposable += _eventsBlock.OnMouseOver(Handler<MouseEvent>("mouseover", "block"));
        _disposable += _eventsBlock.OnMouseOut(Handler<MouseEvent>("mouseout", "block"));
        _disposable += _eventsBlock.OnMouseMove(Handler<MouseEvent>("mousemove", "block"));
        _disposable += _eventsBlock.OnWheel(Handler<WheelEvent>("wheel", "block"));

        _disposable += _resizedBlock.OnResize(Handler<ResizeEvent>("resize", "block"));
    }

    /// <summary>
    /// Disposes of event subscriptions and resources.
    /// </summary>
    public void Dispose()
    {
        _disposable.Dispose();
    }

    /// <summary>
    /// Creates a generic event handler that logs event information to the console.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="ev">The event name.</param>
    /// <param name="code">The element identifier.</param>
    /// <returns>An action that handles the event by logging it.</returns>
    private Action<T> Handler<T>(string ev, string code) => e => Console.WriteLine($"{code}.{ev}: {e}");

    /// <summary>
    /// Toggles the size of the resized block between normal and enlarged states.
    /// </summary>
    /// <param name="_">The event.</param>
    private void ToggleResizedBlockSize(MouseEventArgs _) => _isResized = !_isResized;
}
