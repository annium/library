using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Backuper.Connection.Abstract;
using Npgsql;

namespace Backuper.Connection.PostgreSQL;

public class Connection : IConnection
{
    private readonly Configuration _cfg;
    private readonly IShell _shell;

    public Connection(Configuration cfg, IShell shell)
    {
        _cfg = cfg;
        _shell = shell;
    }

    public async Task SetupAsync()
    {
        await using var conn = new NpgsqlConnection(GetConnectionString());
        await conn.OpenAsync();
    }

    public async Task<string> BackupAsync()
    {
        var path = Path.GetTempFileName();
        var result = await _shell
            .Cmd(
                "pg_dump -Fc -v",
                $"--dbname=postgresql://{_cfg.User}:{_cfg.Pass}@{_cfg.Host}:{_cfg.Port}/{_cfg.Db}",
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
        var result = await _shell
            .Cmd(
                "pg_restore -Fc --clean --if-exists -v",
                $"--dbname=postgresql://{_cfg.User}:{_cfg.Pass}@{_cfg.Host}:{_cfg.Port}/{_cfg.Db}",
                path
            )
            .Pipe(true)
            .RunAsync();
        if (!result.IsSuccess)
            throw new InvalidOperationException("restore failed");
    }

    private string GetConnectionString() =>
        string.Join(
            ';',
            $"Host={_cfg.Host}",
            $"Port={_cfg.Port}",
            $"Database={_cfg.Db}",
            $"Username={_cfg.User}",
            $"Password={_cfg.Pass}",
            "SSL Mode=Prefer",
            "Trust Server Certificate=true"
        );
}
