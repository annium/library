using Annium.Blazor.Interop.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Interop.Tests;

/// <summary>
/// Tests for <see cref="Helper"/> — the string contract every JS lookup depends on.
/// </summary>
public class HelperTest
{
    /// <summary>
    /// Tests that Call prefixes the identifier with the interop namespace verbatim.
    /// </summary>
    [Fact]
    public void Call_PrefixesInteropNamespace()
    {
        Helper.Call("window.onKeyboardEvent").Is("Annium.interop.window.onKeyboardEvent");
        Helper.Call("element.getBoundingClientRect").Is("Annium.interop.element.getBoundingClientRect");
    }
}
