using System;

namespace Xc.Config.Raw
{
    internal class TargetConfig
    {
        public string[] Copy { get; set; } = Array.Empty<string>();
        public string[] To { get; set; } = Array.Empty<string>();
    }
}