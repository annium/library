using System.Collections.Generic;
using Annium.Storage.Abstractions;
using Backuper.Notification.Abstract;

namespace Backuper.Api.State;

public class Plan
{
    public string Name { get; }
    public IStorage Storage { get; }
    public string Interval { get; }
    public int Capacity { get; }
    public IReadOnlyDictionary<string, IChannel> Notifications { get; set; }

    public Plan(
        string name,
        IStorage storage,
        string interval,
        int capacity,
        IReadOnlyDictionary<string, IChannel> notifications
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