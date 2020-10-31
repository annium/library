using System.Collections.Generic;
using Xc.Setup;

namespace Xc.Tasks
{
    internal class VerifyTask : IFuncTask<IReadOnlyCollection<string>, RootSetup, string>
    {
        public IReadOnlyCollection<string> Execute(RootSetup cfg, string root)
        {
            throw new System.NotImplementedException();
        }
    }
}