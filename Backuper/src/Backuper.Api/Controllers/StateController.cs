using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Backuper.Api.Controllers
{
    [Route("/")]
    public class StateController : ControllerBase
    {
        public StateController()
        {

        }

        [HttpGet("state")]
        public IActionResult GetState()
        {
            return Ok("State goes here");
        }

        [HttpGet("{server}/backups")]
        public async Task<IActionResult> ListBackupsAsync(string server)
        {
            await Task.CompletedTask;

            return Ok(new [] { "a", "b" });
        }

        [HttpPut("{server}/backups")]
        public async Task<IActionResult> CreateBackupAsync(string server)
        {
            await Task.CompletedTask;

            return Ok("Backup complete");
        }

        [HttpPost("{server}/backups/{backupId}")]
        public async Task<IActionResult> RestoreBackupAsync(string server, string backupId)
        {
            await Task.CompletedTask;

            return Ok("Restore complete");
        }

        [HttpDelete("{server}/backups/{backupId}")]
        public async Task<IActionResult> DeleteBackupAsync(string server, string backupId)
        {
            await Task.CompletedTask;

            return Ok("Delete complete");
        }
    }
}