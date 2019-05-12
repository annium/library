namespace Backuper.Notification.Abstract
{
    public abstract class Channel
    {
        public string Name { get; }

        public Channel(string name)
        {
            Name = name;
        }
    }
}