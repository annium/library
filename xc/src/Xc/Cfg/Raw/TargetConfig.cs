using System;

namespace Xc.Cfg.Raw
{
    internal class TargetConfig
    {
        public string[] Copy { get; set; } = Array.Empty<string>();
        public string[] To { get; set; } = Array.Empty<string>();
    }
}