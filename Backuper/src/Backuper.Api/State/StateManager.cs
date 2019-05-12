using System;
using Annium.Logging.Abstractions;

namespace Backuper.Api.State
{
    public class StateManager
    {
        private readonly ILogger logger;

        private State state;

        public StateManager(
            ILogger<StateManager> logger
        )
        {
            this.logger = logger;
        }

        public void SetState(State state)
        {
            if (this.state != null)
                throw new InvalidOperationException($"State is already set");

            this.state = state;
            Start();
        }

        private void Start()
        {
            logger.Debug($"StateManager starting");
        }
    }
}