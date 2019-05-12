using System;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Npgsql;

namespace Backuper.Connection.PostgreSQL
{
    public class Connection : Abstract.Connection
    {
        private readonly Configuration cfg;

        public Connection(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }

        public override async Task SetupAsync()
        {
            Debug("setup");

            try
            {
                using(var cn = new NpgsqlConnection(GetConnectionString()))
                {
                    await cn.OpenAsync();
                }
                Debug("connection ok");
            }
            catch (PostgresException)
            {
                throw new InvalidOperationException(msg("connection failed"));
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