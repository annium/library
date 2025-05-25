using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.Extensions;
using Annium.Core.Mediator;
using Backuper.Api.State;
using Backuper.Api.Tools;
using Backuper.Notification.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Backuper.Api.Controllers;

[Route("/")]
public class StateController : ServerController
{
    private readonly Func<State.State> _getState;

    private readonly Namer _namer;

    public StateController(Func<State.State> getState, Namer namer, IMediator mediator, IServiceProvider sp)
        : base(mediator, sp)
    {
        _getState = getState;
        _namer = namer;
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        return Ok(_getState());
    }

    [HttpGet("{serverName}/backups/{planName}")]
    public async Task<IActionResult> ListBackupsAsync(string serverName, string planName)
    {
        var (_, plan, errorResult) = ResolveServerPlan(serverName, planName);
        if (errorResult != null)
            return errorResult;

        var backups = await plan.Storage.ListAsync();

        return Ok(backups);
    }

    [HttpPost("{serverName}/backups/{planName}")]
    public async Task<IActionResult> CreateBackupAsync(string serverName, string planName)
    {
        var (server, plan, errorResult) = ResolveServerPlan(serverName, planName);
        if (errorResult != null)
            return errorResult;

        var backupId = _namer.GetName();
        try
        {
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: start manual backup {backupId} procedure"));

            // cleanup
            var deletedItems = (await plan.Storage.ListAsync())
                .OrderByDescending(i => i)
                .Skip(plan.Capacity - 1)
                .ToArray();
            if (deletedItems.Length > 0)
            {
                await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: cleanup {deletedItems.Length} old backups"));
                foreach (var item in deletedItems)
                {
                    await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: delete old backup {item}"));
                    await plan.Storage.DeleteAsync(item);
                }
            }
            else
                await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: no cleanup needed"));

            // create backup
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: create backup {backupId}"));
            var path = await server.Connection.BackupAsync();
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} created"));

            // upload backup
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: upload backup {backupId}"));
            using (var fs = System.IO.File.OpenRead(path))
                await plan.Storage.UploadAsync(fs, backupId);
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} uploaded"));

            // delete temp file
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: manual backup {backupId} procedure succeed"));

            return Ok(backupId);
        }
        catch (Exception e)
        {
            await NotifyAllAsync(ch =>
                ch.ErrorAsync($"{server} {plan}: manual backup {backupId} procedure failed: {e}")
            );

            return new ObjectResult(e.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        Task NotifyAllAsync(Func<IChannel, Task> notifyChannel) =>
            Task.WhenAll(plan.Notifications.Values.Select(notifyChannel));
    }

    [HttpPost("{serverName}/backups/{planName}/{backupId}")]
    public async Task<IActionResult> RestoreBackupAsync(string serverName, string planName, string backupId)
    {
        var (server, plan, errorResult) = ResolveServerPlan(serverName, planName);
        if (errorResult != null)
            return errorResult;

        try
        {
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: start restore {backupId} procedure"));

            // ensure backup exists
            var list = await plan.Storage.ListAsync();
            if (!list.Contains(backupId))
            {
                await NotifyAllAsync(ch => ch.WarnAsync($"{server} {plan}: backup {backupId} not found in storage"));
                return NotFound($"Backup {backupId} not found in storage");
            }

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} found in storage"));

            // get temp file path
            var path = Path.GetTempFileName();
            System.IO.File.Delete(path);

            // download backup to temp path
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: download backup {backupId}"));
            using (var ms = await plan.Storage.DownloadAsync(backupId))
            using (var fs = System.IO.File.OpenWrite(path))
                await ms.CopyToAsync(fs);
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} downloaded"));

            // restore backup
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: restore backup {backupId}"));
            await server.Connection.RestoreAsync(path);
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} restored"));

            // delete temp file
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: restore {backupId} procedure succeed"));

            return NoContent();
        }
        catch (Exception e)
        {
            await NotifyAllAsync(ch => ch.ErrorAsync($"{server} {plan}: restore procedure failed: {e}"));

            return new ObjectResult(e.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        Task NotifyAllAsync(Func<IChannel, Task> notifyChannel) =>
            Task.WhenAll(plan.Notifications.Values.Select(notifyChannel));
    }

    [HttpDelete("{serverName}/backups/{planName}/{backupId}")]
    public async Task<IActionResult> DeleteBackupAsync(string serverName, string planName, string backupId)
    {
        var (server, plan, errorResult) = ResolveServerPlan(serverName, planName);
        if (errorResult != null)
            return errorResult;

        try
        {
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: start delete {backupId} procedure"));

            // ensure backup exists
            var list = await plan.Storage.ListAsync();
            if (!list.Contains(backupId))
            {
                await NotifyAllAsync(ch => ch.WarnAsync($"{server} {plan}: backup {backupId} not found in storage"));
                return NotFound($"Backup {backupId} not found in storage");
            }

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} found in storage"));

            // download backup to temp path
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: delete backup {backupId}"));
            await plan.Storage.DeleteAsync(backupId);
            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: backup {backupId} deleted"));

            await NotifyAllAsync(ch => ch.InfoAsync($"{server} {plan}: delete {backupId} procedure succeed"));

            return NoContent();
        }
        catch (Exception e)
        {
            await NotifyAllAsync(ch => ch.ErrorAsync($"{server} {plan}: delete {backupId} procedure failed: {e}"));

            return new ObjectResult(e.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        Task NotifyAllAsync(Func<IChannel, Task> notifyChannel) =>
            Task.WhenAll(plan.Notifications.Values.Select(notifyChannel));
    }

    private (Server, Plan, IActionResult?) ResolveServerPlan(string serverName, string planName)
    {
        var state = _getState();
        var server = state.Servers[serverName];
        if (server == null)
            return (default!, default!, NotFound($"Server {serverName} is not configured"));

        var plan = server.Plans[planName];
        if (plan == null)
            return (default!, default!, NotFound($"Server {serverName} has no plan {planName}"));

        return (server, plan, null);
    }
}
