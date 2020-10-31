using System.Collections.Generic;

namespace Xc.Cfg.Raw
{
    internal class Config
    {
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, string> Includes { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, Dictionary<string, TargetConfig>> Targets { get; set; } =
            new Dictionary<string, Dictionary<string, TargetConfig>>();
    }
}