using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Npgsql;

namespace Backuper.Connection.PostgreSQL
{
    public class Connection : Abstract.Connection
    {
        private readonly Configuration cfg;

        private readonly IShell shell;

        public Connection(
            string name,
            Configuration cfg,
            IShell shell,
            ILogger logger
        ) : base("PostgreSQL", name, logger)
        {
            this.cfg = cfg;
            this.shell = shell;
        }

        protected override async Task DoSetupAsync()
        {
            using(var conn = new NpgsqlConnection(GetConnectionString()))
            {
                await conn.OpenAsync();
            }
        }

        protected override async Task<string> DoBackupAsync()
        {
            var path = Path.GetTempFileName();
            var result = await shell
                .Cmd(
                    "pg_dump -Fc -v",
                    $"--dbname=postgresql://{cfg.User}:{cfg.Pass}@{cfg.Host}:{cfg.Port}/{cfg.Db}",
                    $"-f {path}"
                )
                .Pipe(true)
                .RunAsync();
            if (!result.IsSuccess)
                throw new InvalidOperationException(msg("backup failed"));

            return path;
        }

        protected override async Task DoRestoreAsync(string path)
        {
            var result = await shell
                .Cmd(
                    "pg_restore -Fc --clean --if-exists -v",
                    $"--dbname=postgresql://{cfg.User}:{cfg.Pass}@{cfg.Host}:{cfg.Port}/{cfg.Db}",
                    path
                )
                .Pipe(true)
                .RunAsync();
            if (!result.IsSuccess)
                throw new InvalidOperationException(msg("restore failed"));
        }

        private string GetConnectionString() => string.Join(';', new string[]
        {
            $"Host={cfg.Host}",
            $"Port={cfg.Port}",
            $"Database={cfg.Db}",
            $"Username={cfg.User}",
            $"Password={cfg.Pass}",
        });
    }
}