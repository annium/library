using System;
using Xc.Cfg;

namespace Xc.Tasks
{
    internal class ConfigureTask : IActionTask<Configuration, string>
    {
        public ConfigureTask()
        {
        }

        public void Execute(Configuration cfg, string env)
        {
            throw new NotImplementedException();
        }
    }
}