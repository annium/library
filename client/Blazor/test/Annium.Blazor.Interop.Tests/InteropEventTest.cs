using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Annium.Blazor.Interop.Internal;
using Annium.Testing;
using Microsoft.JSInterop;
using Xunit;

namespace Annium.Blazor.Interop.Tests;

/// <summary>
/// Tests for <see cref="InteropEvent{T}"/> — the highest-risk unit: it binds a hand-written JS argument array
/// positionally into a record's primary constructor via reflection, with no compile-time safety net. These tests pin
/// that positional contract (a reordered record field would silently corrupt data in production) plus the callback
/// registration / disposal bookkeeping, all through a fake JS runtime (no DOM).
/// </summary>
public class InteropEventTest : TestBase
{
    /// <summary>
    /// Parses a JSON array literal into the <see cref="JsonElement"/>[] shape that JS hands to Handle.
    /// </summary>
    /// <param name="json">A JSON array literal.</param>
    /// <returns>The parsed elements.</returns>
    private static JsonElement[] Args(string json) => JsonSerializer.Deserialize<JsonElement[]>(json)!;

    /// <summary>
    /// Registers a handler on a fresh static event and returns the concrete event plus a slot capturing the next
    /// dispatched value.
    /// </summary>
    /// <typeparam name="T">The event payload type.</typeparam>
    /// <param name="eventKey">The event key passed to Register.</param>
    /// <param name="captured">Receives a getter for the last value dispatched to the handler.</param>
    /// <returns>The concrete event instance whose Handle can be invoked directly.</returns>
    private InteropEvent<T> Register<T>(object eventKey, out Func<T> captured)
        where T : notnull
    {
        var box = new T[1];
        var hasValue = false;
        captured = () => hasValue ? box[0] : throw new InvalidOperationException("handler was not invoked");

        var evt = (InteropEvent<T>)InteropEvent<T>.Static("window", "window");
        evt.Register(
            eventKey,
            v =>
            {
                box[0] = v;
                hasValue = true;
            }
        );

        return evt;
    }

    /// <summary>
    /// Tests that each keyboard payload array index binds to its specific named KeyboardEvent property (asserting
    /// per-property, not via the record's own constructor, so a field reorder would be caught).
    /// </summary>
    [Fact]
    public void Handle_BindsKeyboardEventPositionally()
    {
        var evt = Register<KeyboardEvent>(KeyboardEventEnum.keydown, out var captured);

        evt.Handle(0, Args("""["Enter","Digit1",true,false,true,false]"""));

        var e = captured();
        e.Key.Is("Enter");
        e.Code.Is("Digit1");
        e.MetaKey.IsTrue();
        e.CtrlKey.IsFalse();
        e.AltKey.IsTrue();
        e.ShiftKey.IsFalse();
    }

    /// <summary>
    /// Tests that each mouse payload array index binds to its specific named MouseEvent property.
    /// </summary>
    [Fact]
    public void Handle_BindsMouseEventPositionally()
    {
        var evt = Register<MouseEvent>(MouseEventEnum.mousemove, out var captured);

        evt.Handle(0, Args("[10,20,false,true,false,true]"));

        var e = captured();
        e.X.Is(10);
        e.Y.Is(20);
        e.MetaKey.IsFalse();
        e.CtrlKey.IsTrue();
        e.AltKey.IsFalse();
        e.ShiftKey.IsTrue();
    }

    /// <summary>
    /// Tests that each wheel payload array index (decimal deltas + modifier flags) binds to its named WheelEvent
    /// property.
    /// </summary>
    [Fact]
    public void Handle_BindsWheelEventPositionally()
    {
        var evt = Register<WheelEvent>("wheel", out var captured);

        evt.Handle(0, Args("[1.5,-2.5,false,false,true,false]"));

        var e = captured();
        e.DeltaX.Is(1.5m);
        e.DeltaY.Is(-2.5m);
        e.MetaKey.IsFalse();
        e.CtrlKey.IsFalse();
        e.AltKey.IsTrue();
        e.ShiftKey.IsFalse();
    }

    /// <summary>
    /// Tests that each resize payload array index binds to its named ResizeEvent property (width vs height not
    /// inverted).
    /// </summary>
    [Fact]
    public void Handle_BindsResizeEventPositionally()
    {
        var evt = Register<ResizeEvent>("resize", out var captured);

        evt.Handle(0, Args("[800,600]"));

        var e = captured();
        e.Width.Is(800);
        e.Height.Is(600);
    }

    /// <summary>
    /// Tests that Handle throws when the callback id is not registered (pins the missing-handler guard).
    /// </summary>
    [Fact]
    public void Handle_UnknownCallbackId_Throws()
    {
        var evt = Register<ResizeEvent>("resize", out _);

        Wrap.It(() => evt.Handle(999, Args("[1,1]"))).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that Register issues the conventionally named binder call with the event key as its first shared arg.
    /// </summary>
    [Fact]
    public void Register_CallsConventionallyNamedBinder()
    {
        Register<KeyboardEvent>(KeyboardEventEnum.keydown, out _);

        var call = Fake.Runtime.Calls.Single();
        call.Identifier.Is("Annium.interop.window.onKeyboardEvent");
        // positional JS-wiring contract: [eventKey, netRef, handleMethod]
        call.Args[0].Is("keydown");
        (call.Args[1] is DotNetObjectReference<InteropEvent<KeyboardEvent>>).IsTrue();
        (call.Args[2] is string).IsTrue();
    }

    /// <summary>
    /// Tests that the unbind call passes the event key then the callback id, in that order (pins the unbinder's
    /// positional JS contract).
    /// </summary>
    [Fact]
    public void Unregister_PassesEventKeyThenCallbackId()
    {
        var evt = (InteropEvent<KeyboardEvent>)InteropEvent<KeyboardEvent>.Static("window", "window");
        var unregister = evt.Register(KeyboardEventEnum.keydown, _ => { });

        unregister();

        var off = Fake.Runtime.Calls.Single(c => c.Identifier == "Annium.interop.window.offKeyboardEvent");
        off.Args[0].Is("keydown");
        off.Args[1].Is(0);
    }

    /// <summary>
    /// Tests that Register forwards its trailing extra args (e.g. the preventDefault flag OnKeyDown/OnKeyUp pass) to
    /// the JS binder call as the final argument — the only channel for those parameters.
    /// </summary>
    [Fact]
    public void Register_ForwardsExtraArgsToBinder()
    {
        var evt = (InteropEvent<KeyboardEvent>)InteropEvent<KeyboardEvent>.Static("window", "window");

        evt.Register(KeyboardEventEnum.keydown, _ => { }, true);

        Fake.Runtime.Calls.Single().Args[^1].Is(true);
    }

    /// <summary>
    /// Tests that invoking the unregister action twice unbinds the handler exactly once (the disposer is removed on
    /// the first call, so the second is a no-op) — pins the double-dispose guard.
    /// </summary>
    [Fact]
    public void Unregister_IsIdempotent()
    {
        var evt = (InteropEvent<KeyboardEvent>)InteropEvent<KeyboardEvent>.Static("window", "window");
        var unregister = evt.Register(KeyboardEventEnum.keydown, _ => { });

        unregister();
        unregister();

        Fake.Runtime.Calls.Count(c => c.Identifier == "Annium.interop.window.offKeyboardEvent").Is(1);
    }

    /// <summary>
    /// Tests that disposing the event unbinds every still-registered handler (pins Dispose draining the disposer set).
    /// </summary>
    [Fact]
    public void Dispose_UnbindsRegisteredHandlers()
    {
        var evt = (InteropEvent<KeyboardEvent>)InteropEvent<KeyboardEvent>.Static("window", "window");
        evt.Register(KeyboardEventEnum.keydown, _ => { });
        evt.Register(KeyboardEventEnum.keyup, _ => { });

        evt.Dispose();

        Fake.Runtime.Calls.Count(c => c.Identifier == "Annium.interop.window.offKeyboardEvent").Is(2);
    }

    /// <summary>
    /// Tests that disposing the event disposes its DotNetObjectReference, unpinning it from the JS runtime's
    /// reference table (otherwise the reference — and the event graph it captures — leaks for the app's lifetime).
    /// </summary>
    [Fact]
    public void Dispose_DisposesDotNetObjectReference()
    {
        var evt = (InteropEvent<KeyboardEvent>)InteropEvent<KeyboardEvent>.Static("window", "window");
        evt.Register(KeyboardEventEnum.keydown, _ => { });

        evt.Dispose();

        var netRef =
            (DotNetObjectReference<InteropEvent<KeyboardEvent>>)
                typeof(InteropEvent<KeyboardEvent>)
                    .GetField("_netRef", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(evt)!;
        Wrap.It(() => _ = netRef.Value).Throws<ObjectDisposedException>();
    }
}
