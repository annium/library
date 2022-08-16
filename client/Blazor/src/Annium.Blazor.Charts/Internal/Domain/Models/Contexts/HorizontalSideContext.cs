using System;
using System.Threading;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record HorizontalSideContext : IManagedHorizontalSideContext
{
    public Canvas Canvas { get; private set; } = default!;
    public Canvas Overlay { get; private set; } = default!;
    public DomRect Rect { get; private set; }
    private int _isInitiated;

    public void Init(
        Canvas canvas,
        Canvas overlay,
        DomRect rect
    )
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(VerticalSideContext)} more than once");

        Canvas = canvas;
        Overlay = overlay;
        Rect = rect;
    }
}