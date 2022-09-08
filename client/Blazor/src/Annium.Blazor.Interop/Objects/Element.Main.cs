using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;

namespace Annium.Blazor.Interop;

public abstract partial record Element : IObject, IDisposable
{
    protected static IInteropContext Ctx => InteropContext.Instance;
    public abstract string Id { get; }
    private readonly DisposableBox _disposable = Disposable.Box();

    protected Element()
    {
        _disposable += _keyboardEvent = new ElementInteropEvent<KeyboardEvent>(this);
        _disposable += _mouseEvent = new ElementInteropEvent<MouseEvent>(this);
        _disposable += _wheelEvent = new ElementInteropEvent<WheelEvent>(this);
    }

    public string Style
    {
        get => Ctx.UInvoke<string, string>("element.getStyle", Id);
        set => Ctx.UInvokeVoid("element.setStyle", Id, value);
    }

    public DomRect GetBoundingClientRect()
        => Ctx.Invoke<DomRect>("element.getBoundingClientRect", Id);

    public void Dispose()
    {
        _disposable.Dispose();
    }
}