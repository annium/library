using System.Threading.Tasks;

namespace Backuper.Connection.Abstract
{
    public abstract class ConnectionManager<T> where T : Configuration
    {
        public abstract Task<Connection> GetConnectionAsync(string name, T configuration);
    }
}