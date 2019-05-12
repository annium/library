using System;
using NodaTime;

namespace Backuper.Api.State
{
    public class Plan
    {
        public Storage.Abstract.Storage Storage { get; }

        public Func<Instant, bool> IsTime { get; }

        public Notification.Abstract.Channel[] Notifications { get; }

        public Plan(
            Storage.Abstract.Storage storage,
            Func<Instant, bool> isTime,
            Notification.Abstract.Channel[] notifications
        )
        {
            Storage = storage;
            IsTime = isTime;
            Notifications = notifications;
        }
    }
}