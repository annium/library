using System;
using System.Threading.Tasks;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public abstract partial record Element : IAsyncDisposable
{
    protected IInteropContext Ctx => InteropContext.Instance;
    protected abstract string Id { get; }
    private readonly DotNetObjectReference<Element> _ref;
    private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected Element()
    {
        _disposable += _ref = DotNetObjectReference.Create(this);
    }

    public string Style
    {
        get => Ctx.UInvoke<string, string>("element.getStyle", Id);
        set => Ctx.UInvokeVoid("element.setStyle", Id, value);
    }

    public DomRect GetBoundingClientRect()
        => Ctx.Invoke<DomRect>("element.getBoundingClientRect", Id);

    public async ValueTask DisposeAsync()
    {
        await _disposable.DisposeAsync();
    }
}