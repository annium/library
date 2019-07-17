using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Jobs;
using Annium.Logging.Abstractions;
using Backuper.Api.Tools;
using Backuper.Notification.Abstract;

namespace Backuper.Api.State
{
    public class StateManager
    {
        private readonly IScheduler scheduler;

        private readonly Namer namer;

        private readonly ILogger<StateManager> logger;

        public State State { get; private set; }

        public StateManager(
            IScheduler scheduler,
            Namer namer,
            ILogger<StateManager> logger
        )
        {
            this.scheduler = scheduler;
            this.namer = namer;
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
            var connections = State.Servers.Values.Select(s => s.Connection).ToArray();
            await Task.WhenAll(connections.Select(s => s.SetupAsync()));

            logger.Debug($"Setup storages");
            var storages = State.Servers.Values.SelectMany(s => s.Plans.Values).Select(p => p.Storage).ToArray();
            await Task.WhenAll(storages.Select(s => s.SetupAsync()));

            logger.Debug($"Schedule operations");
            foreach (var server in State.Servers.Values)
                foreach (var plan in server.Plans.Values)
                    scheduler.Schedule(() => BackupAsync(server, plan), plan.Interval);
        }

        private async Task BackupAsync(Server server, Plan plan)
        {
            var backupId = namer.GetName();
            try
            {
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: start scheduled backup {backupId} procedure"));

                // cleanup
                var deletedItems = (await plan.Storage.ListAsync()).OrderByDescending(i => i).Skip(plan.Capacity - 1).ToArray();
                if (deletedItems.Length > 0)
                {
                    await notifyAll(ch => ch.InfoAsync($"{server} {plan}: cleanup {deletedItems.Length} old backups"));
                    foreach (var item in deletedItems)
                    {
                        await notifyAll(ch => ch.InfoAsync($"{server} {plan}: delete old backup {item}"));
                        await plan.Storage.DeleteAsync(item);
                    }
                }
                else
                    await notifyAll(ch => ch.InfoAsync($"{server} {plan}: no cleanup needed"));

                // create backup
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: create backup {backupId}"));
                var path = await server.Connection.BackupAsync();
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} created"));

                // upload backup
                var name = namer.GetName();
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: upload backup {backupId}"));
                using(var fs = File.OpenRead(path)) await plan.Storage.UploadAsync(fs, name);
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} uploaded"));

                // delete temp file
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: scheduled backup {backupId} procedure succeed"));
            }
            catch (Exception e)
            {
                await notifyAll(ch => ch.ErrorAsync($"{server} {plan}: scheduled backup {backupId} procedure failed: {e}"));
            }

            Task notifyAll(Func<IChannel, Task> notifyChannel) =>
                Task.WhenAll(plan.Notifications.Values.Select(notifyChannel));
        }
    }
}