namespace Backuper.Api.State
{
    public class Plan
    {
        public string Name { get; }

        public Storage.Abstract.Storage Storage { get; }

        public string Interval { get; }

        public Notification.Abstract.Channel[] Notifications { get; }

        public Plan(
            string name,
            Storage.Abstract.Storage storage,
            string interval,
            Notification.Abstract.Channel[] notifications
        )
        {
            Name = name;
            Storage = storage;
            Interval = interval;
            Notifications = notifications;
        }
    }
}