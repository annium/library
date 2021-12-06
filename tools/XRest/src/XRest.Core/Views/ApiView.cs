using System;

namespace XRest.Core.Views;

public class ApiView
{
    public ControllerView[] Controllers { get; set; } = Array.Empty<ControllerView>();
}