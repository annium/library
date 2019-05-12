using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Jobs;
using Annium.Logging.Abstractions;

namespace Backuper.Api.State
{
    public class StateManager
    {
        private readonly IScheduler scheduler;

        private readonly ILogger<StateManager> logger;

        public State State { get; private set; }

        public StateManager(
            IScheduler scheduler,
            ILogger<StateManager> logger
        )
        {
            this.scheduler = scheduler;
            this.logger = logger;
        }

        public void SetState(State state)
        {
            if (State != null)
                throw new InvalidOperationException($"State is already set");

            State = state;
            StartAsync().GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            logger.Debug($"StateManager starting");

            logger.Debug($"Setup connections");
            var connections = State.Servers.Select(s => s.Connection).ToArray();
            await Task.WhenAll(connections.Select(s => s.SetupAsync()));

            logger.Debug($"Setup storages");
            var storages = State.Servers.SelectMany(s => s.Plans).Select(p => p.Storage).Distinct().ToArray();
            await Task.WhenAll(storages.Select(s => s.SetupAsync()));
        }
    }
}