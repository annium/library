using System.Threading.Tasks;

namespace Backuper.Connection.PostgreSQL
{
    public class ConnectionManager : Abstract.ConnectionManager<Configuration>
    {
        public override Task<Abstract.Connection> GetConnectionAsync(string name, Configuration configuration)
        {
            return Task.FromResult<Abstract.Connection>(new Connection(name));
        }
    }
}