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
        ) : base(name, logger)
        {
            this.cfg = cfg;
            this.shell = shell;
        }

        public override async Task SetupAsync()
        {
            Debug("setup");

            try
            {
                using(var conn = new NpgsqlConnection(GetConnectionString()))
                {
                    await conn.OpenAsync();
                }
                Debug("connection ok");
            }
            catch (PostgresException)
            {
                throw new InvalidOperationException(msg("connection failed"));
            }
        }

        public override async Task<string> BackupAsync()
        {
            Debug("start backup");
            var path = Path.GetTempFileName();

            try
            {
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

                Debug("backup succeed");
            }
            catch
            {
                throw new InvalidOperationException(msg("backup failed"));
            }

            return path;
        }

        public override async Task RestoreAsync(string path)
        {
            Debug("start restore");
            try
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

                Debug("restore succeed");
            }
            catch (PostgresException)
            {
                throw new InvalidOperationException(msg("restore failed"));
            }
        }

        private string GetConnectionString() => string.Join(';', new string[]
        {
            $"Host={cfg.Host}",
            $"Port={cfg.Port}",
            $"Database={cfg.Db}",
            $"Username={cfg.User}",
            $"Password={cfg.Pass}",
        });

        private void Debug(string message) => logger.Debug(msg(message));

        private string msg(string message) => $"Connection PostgreSQL {Name}: {message}";
    }
}