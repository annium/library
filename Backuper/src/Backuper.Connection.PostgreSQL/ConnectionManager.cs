using System.Threading.Tasks;

namespace Backuper.Connection.PostgreSQL
{
    public class ConnectionManager : Abstract.ConnectionManager<Configuration>
    {
        public override Task<Abstract.Connection> GetConnectionAsync(Configuration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}