namespace Backuper.Storage.Abstract
{
    public abstract class Storage
    {
        public string Name { get; }

        public Storage(string name)
        {
            Name = name;
        }
    }
}