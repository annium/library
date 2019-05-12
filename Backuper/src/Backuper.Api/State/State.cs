namespace Backuper.Api.State
{
    public class State
    {
        public Server[] Servers { get; }

        public State(Server[] servers)
        {
            Servers = servers;
        }
    }
}