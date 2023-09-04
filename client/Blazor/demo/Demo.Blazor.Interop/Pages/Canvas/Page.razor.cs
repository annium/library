using System;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Canvas;

public partial class Page : ILogSubject, IDisposable
{
    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger Logger { get; set; } = default!;

    private Annium.Blazor.Interop.Canvas _canvas = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        Console.WriteLine("Canvas manipulation here");
    }

    public void Dispose()
    {
        Console.WriteLine("Canvas page disposed");
    }
}