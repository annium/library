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

        public State? State { get; private set; }

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
            var connections = State!.Servers.Values.Select(s => s.Connection).ToArray();
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
                // cleanup
                var deletedItems = (await plan.Storage.ListAsync()).OrderByDescending(i => i).Skip(plan.Capacity - 1).ToArray();
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
                var name = namer.GetName();
                using (var fs = File.OpenRead(path)) await plan.Storage.UploadAsync(fs, name);

                // delete temp file
                if (File.Exists(path))
                    File.Delete(path);

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