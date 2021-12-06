using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xc.Tasks;

namespace Xc.Commands;

internal class ShowCommand : Command<RootCommandConfiguration>
{
    private readonly Func<LoadSetupTask> _createLoadSetupTask;
    public override string Id { get; } = "show";
    public override string Description { get; } = "show solution";

    public ShowCommand(
        Func<LoadSetupTask> createLoadSetupTask
    )
    {
        _createLoadSetupTask = createLoadSetupTask;
    }

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        var setup = _createLoadSetupTask().Execute(cfg.Path);
    }
}