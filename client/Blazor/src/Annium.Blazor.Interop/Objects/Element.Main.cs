using System;
using Annium.Blazor.Interop.Domain;
using Annium.Blazor.Interop.Internal;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public abstract partial record Element : IObject, IDisposable
{
    protected IInteropContext Ctx => InteropContext.Instance;
    public abstract string Id { get; }
    private readonly DotNetObjectReference<Element> _ref;
    private readonly DisposableBox _disposable = Disposable.Box();

    protected Element()
    {
        _disposable += _ref = DotNetObjectReference.Create(this);
        _disposable += _keyboardEvent = new InteropEvent<KeyboardEvent>(nameof(Element), this, _ref);
        _disposable += _mouseEvent = new InteropEvent<MouseEvent>(nameof(Element), this, _ref);
        _disposable += _wheelEvent = new InteropEvent<WheelEvent>(nameof(Element), this, _ref);
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