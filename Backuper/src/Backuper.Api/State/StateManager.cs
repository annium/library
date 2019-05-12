using System;
using System.Threading.Tasks;

namespace Backuper.Api.Config
{
    public class StateManager
    {
        private readonly IServiceProvider provider;

        private readonly Configuration config;

        public StateManager(
            IServiceProvider provider,
            Configuration config
        )
        {
            this.provider = provider;
            this.config = config;
        }

        public async Task GetState()
        {
            await Task.CompletedTask;

            return;
        }
    }
}