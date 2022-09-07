using System;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Canvas;

public partial class Page : ILogSubject<Page>, IDisposable
{
    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger<Page> Logger { get; set; } = default!;

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