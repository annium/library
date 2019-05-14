namespace Backuper.Api.State
{
    public class Server
    {
        public string Name { get; }

        public Connection.Abstract.Connection Connection { get; }

        public Plan[] Plans { get; }

        public Server(
            string name,
            Connection.Abstract.Connection connection,
            Plan[] plans
        )
        {
            Name = name;
            Connection = connection;
            Plans = plans;
        }

        public override string ToString() => Name;
    }
}