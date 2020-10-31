using System.Collections.Generic;

namespace Xc.Config
{
    internal class TargetConfiguration
    {
        public IReadOnlyCollection<string> Copy { get; }
        public IReadOnlyCollection<string> To { get; }

        public TargetConfiguration(
            IReadOnlyCollection<string> copy,
            IReadOnlyCollection<string> to
        )
        {
            Copy = copy;
            To = to;
        }
    }
}