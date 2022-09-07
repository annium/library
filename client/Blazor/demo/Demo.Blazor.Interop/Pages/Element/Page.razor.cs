using System;
using Annium.Blazor.Interop;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Demo.Blazor.Interop.Pages.Element;

public partial class Page : ILogSubject<Canvas.Page>, IDisposable
{
    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger<Canvas.Page> Logger { get; set; } = default!;

    private Div _container = default!;
    private Div _div = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;
        Console.WriteLine("Init Element interaction");
    }

    public void Dispose()
    {
        Console.WriteLine("Element page disposal");
    }
}