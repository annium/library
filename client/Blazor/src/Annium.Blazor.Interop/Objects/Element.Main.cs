using System;
using System.Threading.Tasks;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public abstract partial record Element : IAsyncDisposable
{
    protected IInteropContext Ctx => InteropContext.Instance;
    protected string Id => _id.Value;
    private readonly ElementReference _reference;
    private readonly Lazy<string> _id;
    private readonly DotNetObjectReference<Element> _ref;
    private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected Element(ElementReference reference)
    {
        _disposable += _ref = DotNetObjectReference.Create(this);
        _id = new Lazy<string>(GetId);
        _reference = reference;
    }

    public string Style
    {
        get => Ctx.UInvoke<string, string>("element.getStyle", Id);
        set => Ctx.UInvokeVoid("element.setStyle", Id, value);
    }

    public DomRect GetBoundingClientRect()
        => Ctx.Invoke<DomRect>("element.getBoundingClientRect", Id);

    private string GetId()
    {
        var id = this.GetFullId();
        Ctx.InvokeVoid("objectTracker.track", id, _reference);
        return id;
    }

    public async ValueTask DisposeAsync()
    {
        await _disposable.DisposeAsync();
    }
}