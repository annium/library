using System;

namespace XRest.Core.Views
{
    public class ControllerView
    {
        public string? Area { get; set; }
        public string Name { get; set; } = string.Empty;
        public ActionView[] Actions { get; set; } = Array.Empty<ActionView>();
    }
}