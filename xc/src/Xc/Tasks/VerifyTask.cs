using System.Collections.Generic;
using Xc.Cfg;

namespace Xc.Tasks
{
    internal class VerifyTask : IFuncTask<IReadOnlyCollection<string>, Configuration, string>
    {
        public IReadOnlyCollection<string> Execute(Configuration cfg, string root)
        {
            throw new System.NotImplementedException();
        }
    }
}