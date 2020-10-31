using System;
using Xc.Setup;

namespace Xc.Tasks
{
    internal class ConfigureTask : IActionTask<RootSetup, string>
    {
        public ConfigureTask()
        {
        }

        public void Execute(RootSetup cfg, string env)
        {
            throw new NotImplementedException();
        }
    }
}