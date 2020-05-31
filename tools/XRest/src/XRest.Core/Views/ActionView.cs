using System;
using System.Net.Http;

namespace XRest.Core.Views
{
    public class ActionView
    {
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ParameterView[] Parameters { get; set; } = Array.Empty<ParameterView>();
        public TypeView? Body { get; set; }
        public TypeView? Response { get; set; }

        public override string ToString() => $"{Method} {Path}";
    }
}