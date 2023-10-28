using System.Collections.Generic;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Views;

internal class ApiView
{
    public IReadOnlyCollection<TypeView> SharedExports { get; }
    public IReadOnlyCollection<ControllerView> Controllers { get; }

    public ApiView(IReadOnlyCollection<TypeView> sharedExports, IReadOnlyCollection<ControllerView> controllers)
    {
        SharedExports = sharedExports;
        Controllers = controllers;
    }
}
