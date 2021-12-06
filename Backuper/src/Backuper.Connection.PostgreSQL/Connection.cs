using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Backuper.Connection.Abstract;
using Npgsql;

namespace Backuper.Connection.PostgreSQL;

public class Connection : IConnection
{
    private readonly Configuration cfg;

    private readonly IShell shell;

    public Connection(
        Configuration cfg,
        IShell shell
    )
    {
        this.cfg = cfg;
        this.shell = shell;
    }

    public async Task SetupAsync()
    {
        using(var conn = new NpgsqlConnection(GetConnectionString()))
        {
            await conn.OpenAsync();
        }
    }

    public async Task<string> BackupAsync()
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
            throw new InvalidOperationException("backup failed");

        return path;
    }

    public async Task RestoreAsync(string path)
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
            throw new InvalidOperationException("restore failed");
    }

    private string GetConnectionString() => string.Join(';', new string[]
    {
        $"Host={cfg.Host}",
        $"Port={cfg.Port}",
        $"Database={cfg.Db}",
        $"Username={cfg.User}",
        $"Password={cfg.Pass}",
        "SSL Mode=Prefer",
        "Trust Server Certificate=true",
    });
}