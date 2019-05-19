using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.AspNetCore.Extensions;
using Backuper.Api.State;
using Backuper.Api.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Backuper.Api.Controllers
{
    [Route("/")]
    public class StateController : ServerController
    {
        private readonly Func<State.State> getState;

        private readonly Namer namer;

        public StateController(
            Func<State.State> getState,
            Namer namer
        )
        {
            this.getState = getState;
            this.namer = namer;
        }

        [HttpGet("state")]
        public IActionResult GetState()
        {
            return Ok(getState());
        }

        [HttpGet("{serverName}/backups/{planName}")]
        public async Task<IActionResult> ListBackupsAsync(string serverName, string planName)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            var backups = await plan.Storage.ListAsync(server.Name);

            return Ok(backups);
        }

        [HttpPut("{serverName}/backups/{planName}")]
        public async Task<IActionResult> CreateBackupAsync(string serverName, string planName)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            var backupId = namer.GetName();
            try
            {
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: start manual backup {backupId} procedure"));

                // cleanup
                var deletedItems = (await plan.Storage.ListAsync(server.Name)).OrderByDescending(i => i).Skip(plan.Capacity).ToArray();
                if (deletedItems.Length > 0)
                {
                    await notifyAll(ch => ch.InfoAsync($"{server} {plan}: cleanup {deletedItems.Length} old backups"));
                    foreach (var item in deletedItems)
                    {
                        await notifyAll(ch => ch.InfoAsync($"{server} {plan}: delete old backup {item}"));
                        await plan.Storage.DeleteAsync(server.Name, item);
                    }
                }
                else
                    await notifyAll(ch => ch.InfoAsync($"{server} {plan}: no cleanup needed"));

                // create backup
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: create backup {backupId}"));
                var path = await server.Connection.BackupAsync();
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} created"));

                // upload backup
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: upload backup {backupId}"));
                await plan.Storage.UploadAsync(path, server.Name, backupId);
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} uploaded"));

                // delete temp file
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: manual backup {backupId} procedure succeed"));

                return Ok(backupId);
            }
            catch (Exception e)
            {
                await notifyAll(ch => ch.ErrorAsync($"{server} {plan}: manual backup {backupId} procedure failed: {e}"));

                return ServerError(e.Message);
            }

            Task notifyAll(Func<Notification.Abstract.Channel, Task> notifyChannel) =>
                Task.WhenAll(plan.Notifications.Select(notifyChannel));
        }

        [HttpPost("{serverName}/backups/{planName}/{backupId}")]
        public async Task<IActionResult> RestoreBackupAsync(string serverName, string planName, string backupId)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            try
            {
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: start restore {backupId} procedure"));

                // ensure backup exists
                var list = await plan.Storage.ListAsync(server.Name);
                if (!list.Contains(backupId))
                {
                    await notifyAll(ch => ch.WarnAsync($"{server} {plan}: backup {backupId} not found in storage"));
                    return NotFound($"Backup {backupId} not found in storage");
                }
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} found in storage"));

                // get temp file path
                var path = Path.GetTempFileName();
                System.IO.File.Delete(path);

                // download backup to temp path
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: download backup {backupId}"));
                await plan.Storage.DownloadAsync(server.Name, backupId, path);
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} downloaded"));

                // restore backup
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: restore backup {backupId}"));
                await server.Connection.RestoreAsync(path);
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} restored"));

                // delete temp file
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: restore {backupId} procedure succeed"));

                return NoContent();
            }
            catch (Exception e)
            {
                await notifyAll(ch => ch.ErrorAsync($"{server} {plan}: restore procedure failed: {e}"));

                return ServerError(e.Message);
            }

            Task notifyAll(Func<Notification.Abstract.Channel, Task> notifyChannel) =>
                Task.WhenAll(plan.Notifications.Select(notifyChannel));
        }

        [HttpDelete("{serverName}/backups/{planName}/{backupId}")]
        public async Task<IActionResult> DeleteBackupAsync(string serverName, string planName, string backupId)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            try
            {
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: start delete {backupId} procedure"));

                // ensure backup exists
                var list = await plan.Storage.ListAsync(server.Name);
                if (!list.Contains(backupId))
                {
                    await notifyAll(ch => ch.WarnAsync($"{server} {plan}: backup {backupId} not found in storage"));
                    return NotFound($"Backup {backupId} not found in storage");
                }
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} found in storage"));

                // download backup to temp path
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: delete backup {backupId}"));
                await plan.Storage.DeleteAsync(server.Name, backupId);
                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} deleted"));

                await notifyAll(ch => ch.InfoAsync($"{server} {plan}: delete {backupId} procedure succeed"));

                return NoContent();
            }
            catch (Exception e)
            {
                await notifyAll(ch => ch.ErrorAsync($"{server} {plan}: delete {backupId} procedure failed: {e}"));

                return ServerError(e.Message);
            }

            Task notifyAll(Func<Notification.Abstract.Channel, Task> notifyChannel) =>
                Task.WhenAll(plan.Notifications.Select(notifyChannel));
        }

        private(Server, Plan, IActionResult) ResolveServerPlan(string serverName, string planName)
        {
            var state = getState();
            var server = state.Servers.FirstOrDefault(s => s.Name == serverName);
            if (server == null)
                return (null, null, NotFound($"Server {serverName} is not configured"));

            var plan = server.Plans.FirstOrDefault(p => p.Name == planName);
            if (plan == null)
                return (null, null, NotFound($"Server {serverName} has no plan {planName}"));

            return (server, plan, null);
        }
    }
}