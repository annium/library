using System.Collections.Generic;

namespace Xc.Cfg
{
    internal class Configuration
    {
        public string Source { get; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TargetConfiguration>> Targets { get; }

        public Configuration(
            string source,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, TargetConfiguration>> targets
        )
        {
            Source = source;
            Targets = targets;
        }
    }
}