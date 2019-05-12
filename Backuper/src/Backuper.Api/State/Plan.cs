using System;
using NodaTime;

namespace Backuper.Api.State
{
    public class Plan
    {
        public string Name { get; }

        public Storage.Abstract.Storage Storage { get; }

        public Func<Instant, bool> IsTime { get; }

        public Notification.Abstract.Channel[] Notifications { get; }

        public Plan(
            string name,
            Storage.Abstract.Storage storage,
            Func<Instant, bool> isTime,
            Notification.Abstract.Channel[] notifications
        )
        {
            Name = name;
            Storage = storage;
            IsTime = isTime;
            Notifications = notifications;
        }
    }
}