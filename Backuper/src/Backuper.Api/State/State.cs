using System.Collections.Generic;

namespace Backuper.Api.State;

public class State
{
    public IReadOnlyDictionary<string, Server> Servers { get; }

    public State(
        IReadOnlyDictionary<string, Server> servers
    )
    {
        Servers = servers;
    }
}