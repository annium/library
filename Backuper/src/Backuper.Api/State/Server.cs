using System.Collections.Generic;

namespace Backuper.Api.State
{
    public class Server
    {
        public Connection.Abstract.Connection Connection { get; }

        public IReadOnlyDictionary<string, Plan> Plans { get; }

        public Server(
            Connection.Abstract.Connection connection,
            IReadOnlyDictionary<string, Plan> plans
        )
        {
            Connection = connection;
            Plans = plans;
        }
    }
}