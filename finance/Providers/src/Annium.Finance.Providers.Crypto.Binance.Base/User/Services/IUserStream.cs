using System;

namespace Annium.Finance.Providers.Crypto.Binance.Base.User.Services;

/// <summary>Maintains the WebSocket connection to Binance's user data stream, delivering account, order and trade update messages.</summary>
/// <remarks>
/// Torn down asynchronously, because one implementation of it has a timer to stop and a timer cannot be
/// stopped synchronously without waiting on whatever tick is running. The other has nothing asynchronous to
/// do and says so by returning a completed task.
/// </remarks>
public interface IUserStream : IAsyncDisposable
{
    /// <summary>Raised when the user data stream WebSocket connects.</summary>
    event Action OnConnected;

    /// <summary>Raised when the user data stream WebSocket disconnects.</summary>
    event Action OnDisconnected;

    /// <summary>Raised for every raw message received over the user data stream.</summary>
    event Action<ReadOnlyMemory<byte>> OnMessage;
}
