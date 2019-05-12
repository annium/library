namespace Backuper.Connection.Abstract
{
    public abstract class Connection
    {
        public string Name { get; }

        public Connection(string name)
        {
            Name = name;
        }
    }
}