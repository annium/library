using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace xrest.Commands
{
    internal class GenerateCommand : Command<GenerateCommandConfiguration>
    {
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate client";
        private readonly ILogger<GenerateCommand> logger;

        public GenerateCommand(
            ILogger<GenerateCommand> logger
        )
        {
            this.logger = logger;
        }

        public override void Handle(GenerateCommandConfiguration cfg, CancellationToken token)
        {
            logger.Info("Generate");
        }
    }

    internal class GenerateCommandConfiguration
    {
    }
}