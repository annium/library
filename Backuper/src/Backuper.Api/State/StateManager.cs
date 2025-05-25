using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Jobs;
using Annium.Logging;
using Backuper.Api.Tools;
using Backuper.Notification.Abstract;

namespace Backuper.Api.State;

public class StateManager : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IScheduler _scheduler;
    private readonly Namer _namer;

    public State? State { get; private set; }

    public StateManager(IScheduler scheduler, Namer namer, ILogger logger)
    {
        _scheduler = scheduler;
        _namer = namer;
        Logger = logger;
    }

    public void SetState(State state)
    {
        if (State != null)
            throw new InvalidOperationException($"State is already set");

        State = state;
#pragma warning disable VSTHRD002
        StartAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    private async Task StartAsync()
    {
        this.Debug("StateManager starting");

        this.Debug("Setup connections");
        var connections = State!.Servers.Values.Select(s => s.Connection).ToArray();
        await Task.WhenAll(connections.Select(s => s.SetupAsync()));

        this.Debug("Schedule operations");
        foreach (var server in State.Servers.Values)
        foreach (var plan in server.Plans.Values)
            _scheduler.Schedule(() => BackupAsync(server, plan), plan.Interval);
    }

    private async Task BackupAsync(Server server, Plan plan)
    {
        var backupId = _namer.GetName();
        try
        {
            // cleanup
            var deletedItems = (await plan.Storage.ListAsync())
                .OrderByDescending(i => i)
                .Skip(plan.Capacity - 1)
                .ToArray();
            if (deletedItems.Length > 0)
            {
                foreach (var item in deletedItems)
                {
                    await plan.Storage.DeleteAsync(item);
                }
            }

            // create backup
            var path = await server.Connection.BackupAsync();

            // upload backup
            var name = _namer.GetName();
            using (var fs = File.OpenRead(path))
                await plan.Storage.UploadAsync(fs, name);

            // delete temp file
            if (File.Exists(path))
                File.Delete(path);

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: scheduled backup {backupId} procedure succeed"));
        }
        catch (Exception e)
        {
            await NotifyAllAsync(ch =>
                ch.ErrorAsync($"{server} {plan}: scheduled backup {backupId} procedure failed: {e}")
            );
        }

        Task NotifyAllAsync(Func<IChannel, Task> notifyChannel) =>
            Task.WhenAll(plan.Notifications.Values.Select(notifyChannel));
    }
}
