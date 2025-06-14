using System;
using System.Runtime.InteropServices.JavaScript;
using Annium.Blazor.Interop.Internal;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Logging;
using static Annium.Blazor.Interop.Internal.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Abstract base record for DOM elements providing core functionality and interop capabilities
/// </summary>
public abstract partial record Element : IObject, IDisposable
{
    /// <summary>
    /// Gets the interop context instance for JavaScript interop operations
    /// </summary>
    protected static IInteropContext Ctx => InteropContext.Instance;

    /// <summary>
    /// Gets the unique identifier of the element
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Disposable box for managing resource cleanup
    /// </summary>
    private readonly DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    /// <summary>
    /// Initializes a new instance of the Element class
    /// </summary>
    protected Element()
    {
        _disposable += _keyboardEvent = InteropEvent<KeyboardEvent>.Element(this);
        _disposable += _mouseEvent = InteropEvent<MouseEvent>.Element(this);
        _disposable += _wheelEvent = InteropEvent<WheelEvent>.Element(this);
        _disposable += _resizeEvent = InteropEvent<ResizeEvent>.Element(this);
    }

    /// <summary>
    /// Gets or sets the style attribute of the element
    /// </summary>
    public string Style
    {
        get => GetStyle(Id);
        set => SetStyle(Id, value);
    }

    /// <summary>
    /// Gets the style attribute of the element with the specified ID
    /// </summary>
    /// <param name="id">The element ID</param>
    /// <returns>The style attribute value</returns>
    [JSImport($"{JsPath}element.getStyle")]
    private static partial string GetStyle(string id);

    /// <summary>
    /// Sets the style attribute of the element with the specified ID
    /// </summary>
    /// <param name="id">The element ID</param>
    /// <param name="style">The style attribute value to set</param>
    [JSImport($"{JsPath}element.setStyle")]
    private static partial void SetStyle(string id, string style);

    /// <summary>
    /// Gets the bounding client rectangle of the element
    /// </summary>
    /// <returns>The bounding client rectangle</returns>
    public DomRect GetBoundingClientRect() => Ctx.Call<DomRect>("element.getBoundingClientRect", Id);

    /// <summary>
    /// Releases all resources used by the Element
    /// </summary>
    public void Dispose()
    {
        _disposable.Dispose();
    }
}
