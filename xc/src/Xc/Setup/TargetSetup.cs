using System.Collections.Generic;

namespace Xc.Setup
{
    internal class TargetSetup
    {
        public IReadOnlyCollection<string> Copy { get; }
        public IReadOnlyCollection<string> To { get; }

        public TargetSetup(
            IReadOnlyCollection<string> copy,
            IReadOnlyCollection<string> to
        )
        {
            Copy = copy;
            To = to;
        }
    }
}