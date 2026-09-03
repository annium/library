using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.State.Tests.Fakes;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Microsoft.JSInterop;
using Xunit;

namespace Annium.Blazor.State.Tests;

/// <summary>
/// Tests for <c>StorageBase</c> — the shared browser-storage logic behind <see cref="ILocalStorage"/> and
/// <see cref="ISessionStorage"/> — exercised through the public interfaces via the real
/// <see cref="ServiceContainerExtensions.AddStates"/> registration, with the JS interop layer faked and a real
/// JSON string serializer wired for the typed <c>Get</c>/<c>Set</c>/<c>TryGet</c> round trip.
/// </summary>
public class StorageBaseTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageBaseTest"/> class, wiring <c>AddStates</c> against a
    /// fake in-process JS runtime and a real default JSON string serializer.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public StorageBaseTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            container.AddStates();
            container.Add<FakeInProcessJsRuntime>().AsSelf().As<IJSRuntime>().Singleton();
        });
    }

    /// <summary>
    /// An empty storage reports no keys — pins the length-0 early return in <c>GetKeys</c>.
    /// </summary>
    [Fact]
    public void GetKeys_Empty_ReturnsEmpty()
    {
        var storage = Get<ILocalStorage>();

        storage.GetKeys().IsEmpty();
    }

    /// <summary>
    /// A storage with several entries reports every key — pins the length-driven range/select loop in
    /// <c>GetKeys</c>.
    /// </summary>
    [Fact]
    public void GetKeys_WithKeys_ReturnsAllKeys()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("a", "1");
        storage.SetString("b", "2");
        storage.SetString("c", "3");

        var keys = storage.GetKeys();

        keys.Has(3);
        var sortedKeys = keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        sortedKeys[0].Is("a");
        sortedKeys[1].Is("b");
        sortedKeys[2].Is("c");
    }

    /// <summary>
    /// <c>HasKey</c> reports true for a key that was set.
    /// </summary>
    [Fact]
    public void HasKey_KeyExists_ReturnsTrue()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "value");

        storage.HasKey("key").IsTrue();
    }

    /// <summary>
    /// <c>HasKey</c> reports false for a key that was never set.
    /// </summary>
    [Fact]
    public void HasKey_KeyAbsent_ReturnsFalse()
    {
        var storage = Get<ILocalStorage>();

        storage.HasKey("missing").IsFalse();
    }

    /// <summary>
    /// Setting a previously-absent key returns true — pins the <c>!hasKey</c> new-vs-updated semantics of
    /// <c>SetString</c>.
    /// </summary>
    [Fact]
    public void SetString_NewKey_ReturnsTrue()
    {
        var storage = Get<ILocalStorage>();

        storage.SetString("key", "value").IsTrue();
    }

    /// <summary>
    /// Setting an already-present key returns false — pins the <c>!hasKey</c> new-vs-updated semantics of
    /// <c>SetString</c> on the update path.
    /// </summary>
    [Fact]
    public void SetString_ExistingKey_ReturnsFalse()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "value");

        storage.SetString("key", "other").IsFalse();
    }

    /// <summary>
    /// <c>TryGetString</c> returns true and the stored value when the key is present with a non-blank value.
    /// </summary>
    [Fact]
    public void TryGetString_Present_ReturnsTrueAndValue()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "value");

        var found = storage.TryGetString("key", out var value);

        found.IsTrue();
        value!.Is("value");
    }

    /// <summary>
    /// <c>TryGetString</c> returns false and a null value when the key was never set.
    /// </summary>
    [Fact]
    public void TryGetString_Absent_ReturnsFalseAndNull()
    {
        var storage = Get<ILocalStorage>();

        var found = storage.TryGetString("missing", out var value);

        found.IsFalse();
        value.IsDefault();
    }

    /// <summary>
    /// <c>TryGetString</c> returns false for a stored whitespace-only value — pins the
    /// <c>IsNullOrWhiteSpace</c> check rather than a plain null/empty check — and nulls the out value on the
    /// false path, per the documented "null otherwise" contract.
    /// </summary>
    [Fact]
    public void TryGetString_WhitespaceValue_ReturnsFalseAndNull()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "   ");

        var found = storage.TryGetString("key", out var value);

        found.IsFalse();
        value.IsDefault();
    }

    /// <summary>
    /// <c>GetString</c> returns the stored value when the key is present.
    /// </summary>
    [Fact]
    public void GetString_Present_ReturnsValue()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "value");

        storage.GetString("key").Is("value");
    }

    /// <summary>
    /// <c>GetString</c> throws <see cref="KeyNotFoundException"/> when the key is absent.
    /// </summary>
    [Fact]
    public void GetString_Absent_ThrowsKeyNotFoundException()
    {
        var storage = Get<ILocalStorage>();

        Wrap.It(() => storage.GetString("missing")).Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// <c>Set</c> followed by <c>Get</c> round-trips a serializable value through the real JSON string
    /// serializer.
    /// </summary>
    [Fact]
    public void Set_Get_RoundTripsValue()
    {
        var storage = Get<ILocalStorage>();
        var payload = new TestPayload("Annium", 42);

        storage.Set("payload", payload);

        storage.Get<TestPayload>("payload").Is(payload);
    }

    /// <summary>
    /// <c>TryGet</c> round-trips a serializable value that was stored via <c>Set</c>.
    /// </summary>
    [Fact]
    public void TryGet_Present_ReturnsTrueAndValue()
    {
        var storage = Get<ILocalStorage>();
        var payload = new TestPayload("Annium", 42);
        storage.Set("payload", payload);

        var found = storage.TryGet<TestPayload>("payload", out var value);

        found.IsTrue();
        value!.Is(payload);
    }

    /// <summary>
    /// <c>TryGet</c> returns false and a default value when the key is absent.
    /// </summary>
    [Fact]
    public void TryGet_Absent_ReturnsFalseAndDefault()
    {
        var storage = Get<ILocalStorage>();

        var found = storage.TryGet<TestPayload>("missing", out var value);

        found.IsFalse();
        value.IsDefault();
    }

    /// <summary>
    /// <c>TryGet</c> returns false (rather than throwing) when the stored value cannot be deserialized to the
    /// requested type — pins the exception-free <c>Try</c> contract against malformed stored content.
    /// </summary>
    [Fact]
    public void TryGet_MalformedValue_ReturnsFalseAndDefault()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("payload", "not-json");

        var found = storage.TryGet<TestPayload>("payload", out var value);

        found.IsFalse();
        value.IsDefault();
    }

    /// <summary>
    /// Removing a present key returns true, and the key is gone afterwards.
    /// </summary>
    [Fact]
    public void Remove_KeyExists_ReturnsTrueAndRemovesKey()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("key", "value");

        storage.Remove("key").IsTrue();

        storage.HasKey("key").IsFalse();
    }

    /// <summary>
    /// Removing an absent key returns false.
    /// </summary>
    [Fact]
    public void Remove_KeyAbsent_ReturnsFalse()
    {
        var storage = Get<ILocalStorage>();

        storage.Remove("missing").IsFalse();
    }

    /// <summary>
    /// <c>Clear</c> empties the storage: <c>GetKeys</c> reports nothing afterwards.
    /// </summary>
    [Fact]
    public void Clear_EmptiesStorage()
    {
        var storage = Get<ILocalStorage>();
        storage.SetString("a", "1");
        storage.SetString("b", "2");

        storage.Clear();

        storage.GetKeys().IsEmpty();
    }

    /// <summary>
    /// <see cref="ILocalStorage"/> operates against the <c>localStorage</c> JS object and leaves
    /// <c>sessionStorage</c> untouched.
    /// </summary>
    [Fact]
    public void LocalStorage_SetString_TargetsLocalStorageObject()
    {
        Get<ILocalStorage>().SetString("key", "value");

        var js = Get<FakeInProcessJsRuntime>();
        js["localStorage"]["key"].Is("value");
        js["sessionStorage"].ContainsKey("key").IsFalse();
    }

    /// <summary>
    /// <see cref="ISessionStorage"/> operates against the <c>sessionStorage</c> JS object and leaves
    /// <c>localStorage</c> untouched.
    /// </summary>
    [Fact]
    public void SessionStorage_SetString_TargetsSessionStorageObject()
    {
        Get<ISessionStorage>().SetString("key", "value");

        var js = Get<FakeInProcessJsRuntime>();
        js["sessionStorage"]["key"].Is("value");
        js["localStorage"].ContainsKey("key").IsFalse();
    }

    /// <summary>
    /// A minimal serializable payload used to exercise the typed <c>Get</c>/<c>Set</c>/<c>TryGet</c> round trip
    /// through the real JSON string serializer.
    /// </summary>
    /// <param name="Name">A string field.</param>
    /// <param name="Value">A numeric field.</param>
    private sealed record TestPayload(string Name, int Value);
}
