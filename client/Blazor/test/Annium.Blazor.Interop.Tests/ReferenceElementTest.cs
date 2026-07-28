using System.Linq;
using Annium.Testing;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Annium.Blazor.Interop.Tests;

/// <summary>
/// Tests for <see cref="ReferenceElement"/> lifecycle: an element registered in the JS objectTracker on first Id
/// access must be released on dispose, or its detached DOM node leaks in the JS-side map for the session's lifetime.
/// </summary>
public class ReferenceElementTest : TestBase
{
    /// <summary>
    /// Tests that disposing a reference element whose Id was resolved releases it from the JS objectTracker (paired
    /// with the track issued on first Id access) — otherwise the tracked HTMLElement leaks.
    /// </summary>
    [Fact]
    public void Dispose_ReleasesTrackedElement()
    {
        // arrange: resolving Id triggers objectTracker.track(id, reference)
        var div = new Div(default);
        var id = div.Id;
        var track = Fake.Runtime.Calls.Single(c => c.Identifier == "Annium.interop.objectTracker.track");
        track.Args[0].Is(id);
        (track.Args[1] is ElementReference).IsTrue();

        // act
        div.Dispose();

        // assert: a matching release for the same id is issued
        var release = Fake.Runtime.Calls.Single(c => c.Identifier == "Annium.interop.objectTracker.release");
        release.Args[0].Is(id);
    }

    /// <summary>
    /// Tests that dispose unbinds DOM listeners (off*) BEFORE releasing the element from the objectTracker: the JS
    /// off* functions resolve the element via objectTracker.get(id), which throws once the entry is released, so
    /// releasing first would abort the remaining unbinds and leak listeners.
    /// </summary>
    [Fact]
    public void Dispose_UnbindsListenersBeforeReleasingTracker()
    {
        // arrange: an active DOM subscription (also resolves Id → track)
        var div = new Div(default);
        div.OnMouseDown(_ => { });

        // act
        div.Dispose();

        // assert: the off* call precedes the tracker release
        var calls = Fake.Runtime.Calls;
        var offIndex = calls.FindIndex(c => c.Identifier == "Annium.interop.element.offMouseEvent");
        var releaseIndex = calls.FindIndex(c => c.Identifier == "Annium.interop.objectTracker.release");
        (offIndex >= 0).IsTrue();
        (releaseIndex >= 0).IsTrue();
        (offIndex < releaseIndex).IsTrue();
    }

    /// <summary>
    /// Tests that Dispose is idempotent: calling it twice releases the tracked element only once (a second release
    /// of an already-untracked id throws on the JS side).
    /// </summary>
    [Fact]
    public void Dispose_IsIdempotent()
    {
        // arrange: resolve Id so the element is tracked
        var div = new Div(default);
        _ = div.Id;

        // act
        div.Dispose();
        div.Dispose();

        // assert: exactly one release despite two Dispose calls
        Fake.Runtime.Calls.Count(c => c.Identifier == "Annium.interop.objectTracker.release").Is(1);
    }

    /// <summary>
    /// Tests that the element-scoped bind and unbind JS calls both carry the element's own Id as their first argument
    /// (the shared bind arg), so listeners are attached to and detached from the correct element.
    /// </summary>
    [Fact]
    public void ElementBindAndUnbind_CarryElementId()
    {
        // arrange
        var div = new Div(default);
        var unregister = div.OnMouseDown(_ => { });
        var id = div.Id;

        // assert: the on* call targets this element's id
        Fake.Runtime.Calls.Single(c => c.Identifier == "Annium.interop.element.onMouseEvent").Args[0].Is(id);

        // act + assert: the off* call targets the same id
        unregister();
        Fake.Runtime.Calls.Single(c => c.Identifier == "Annium.interop.element.offMouseEvent").Args[0].Is(id);
    }

    /// <summary>
    /// Tests that each public element event method binds to its specific JS event with the matching event key — a
    /// one-line wrapper wired to the wrong enum or the wrong event field would otherwise go unnoticed.
    /// </summary>
    [Fact]
    public void ElementEventMethods_RegisterExpectedJsEvents()
    {
        // arrange
        var div = new Div(default);
        var calls = Fake.Runtime.Calls;

        // assert the binder call each specific method just produced (its own last recorded call), so a wrong enum
        // swapped between two siblings sharing a binder (e.g. mousedown ↔ mouseup) is caught — Args[0] is the
        // element id, Args[1] the event key
        void AssertLastCall(string identifier, string key)
        {
            var call = calls[^1];
            call.Identifier.Is(identifier);
            call.Args[1].Is(key);
        }

        div.OnMouseDown(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mousedown");
        div.OnMouseUp(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mouseup");
        div.OnMouseEnter(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mouseenter");
        div.OnMouseLeave(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mouseleave");
        div.OnMouseOver(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mouseover");
        div.OnMouseOut(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mouseout");
        div.OnMouseMove(_ => { });
        AssertLastCall("Annium.interop.element.onMouseEvent", "mousemove");
        div.OnKeyDown(_ => { }, false);
        AssertLastCall("Annium.interop.element.onKeyboardEvent", "keydown");
        div.OnKeyUp(_ => { }, false);
        AssertLastCall("Annium.interop.element.onKeyboardEvent", "keyup");
        div.OnWheel(_ => { });
        AssertLastCall("Annium.interop.element.onWheelEvent", "wheel");
        div.OnResize(_ => { });
        AssertLastCall("Annium.interop.element.onResizeEvent", "resize");
    }

    /// <summary>
    /// Tests that disposing a reference element whose Id was never resolved issues no release (the element was never
    /// tracked, and releasing an untracked id throws on the JS side).
    /// </summary>
    [Fact]
    public void Dispose_WithoutIdAccess_DoesNotRelease()
    {
        // arrange: never touch Id, so track is never issued
        var div = new Div(default);

        // act
        div.Dispose();

        // assert: no release for an element that was never tracked
        Fake.Runtime.Calls.Any(c => c.Identifier == "Annium.interop.objectTracker.release").IsFalse();
    }
}
