using System;
using System.Runtime.InteropServices.JavaScript;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Logging;
using static Annium.Blazor.Interop.Internal.Constants;

namespace Annium.Blazor.Interop;

public abstract partial record Element : IObject, IDisposable
{
    protected static IInteropContext Ctx => InteropContext.Instance;
    public abstract string Id { get; }
    private readonly DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    protected Element()
    {
        _disposable += _keyboardEvent = InteropEvent<KeyboardEvent>.Element(this);
        _disposable += _mouseEvent = InteropEvent<MouseEvent>.Element(this);
        _disposable += _wheelEvent = InteropEvent<WheelEvent>.Element(this);
        _disposable += _resizeEvent = InteropEvent<ResizeEvent>.Element(this);
    }

    public string Style
    {
        get => GetStyle(Id);
        set => SetStyle(Id, value);
    }

    [JSImport($"{JsPath}element.getStyle")]
    private static partial string GetStyle(string id);

    [JSImport($"{JsPath}element.setStyle")]
    private static partial void SetStyle(string id, string style);

    public DomRect GetBoundingClientRect()
        => Ctx.Call<DomRect>("element.getBoundingClientRect", Id);

    public void Dispose()
    {
        _disposable.Dispose();
    }
}