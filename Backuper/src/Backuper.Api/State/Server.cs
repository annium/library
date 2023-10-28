using System.Collections.Generic;
using Backuper.Connection.Abstract;

namespace Backuper.Api.State;

public class Server
{
    public string Name { get; }
    public IConnection Connection { get; }
    public IReadOnlyDictionary<string, Plan> Plans { get; }

    public Server(string name, IConnection connection, IReadOnlyDictionary<string, Plan> plans)
    {
        Name = name;
        Connection = connection;
        Plans = plans;
    }

    public override string ToString() => Name;
}
