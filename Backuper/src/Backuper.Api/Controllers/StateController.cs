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

            var backups = await plan.Storage.ListAsync();

            return Ok(backups);
        }

        [HttpPut("{serverName}/backups/{planName}")]
        public async Task<IActionResult> CreateBackupAsync(string serverName, string planName)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            // create backup
            var path = await server.Connection.BackupAsync();

            // upload backup
            var name = namer.GetName();
            await plan.Storage.UploadAsync(path, name);

            // delete temp file
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return Ok(name);
        }

        [HttpPost("{serverName}/backups/{planName}/{backupId}")]
        public async Task<IActionResult> RestoreBackupAsync(string serverName, string planName, string backupId)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            // ensure backup exists
            var list = await plan.Storage.ListAsync();
            if (!list.Contains(backupId))
                return NotFound($"Backup {backupId} not found in storage");

            // get temp file path
            var path = Path.GetTempFileName();
            System.IO.File.Delete(path);

            // download backup to temp path
            await plan.Storage.DownloadAsync(backupId, path);

            // restore backup
            await server.Connection.RestoreAsync(path);

            // delete temp file
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return Ok("Restore complete");
        }

        [HttpDelete("{serverName}/backups/{planName}/{backupId}")]
        public async Task<IActionResult> DeleteBackupAsync(string serverName, string planName, string backupId)
        {
            var(server, plan, errorResult) = ResolveServerPlan(serverName, planName);
            if (errorResult != null)
                return errorResult;

            // ensure backup exists
            var list = await plan.Storage.ListAsync();
            if (!list.Contains(backupId))
                return NotFound($"Backup {backupId} not found in storage");

            await plan.Storage.DeleteAsync(backupId);

            return NoContent();
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