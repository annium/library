namespace Backuper.Api.State
{
    public class Plan
    {
        public string Name { get; }

        public Storage.Abstract.Storage Storage { get; }

        public string Interval { get; }

        public int Capacity { get; }

        public Notification.Abstract.Channel[] Notifications { get; }

        public Plan(
            string name,
            Storage.Abstract.Storage storage,
            string interval,
            int capacity,
            Notification.Abstract.Channel[] notifications
        )
        {
            Name = name;
            Storage = storage;
            Interval = interval;
            Capacity = capacity;
            Notifications = notifications;
        }

        public override string ToString() => Name;
    }
}