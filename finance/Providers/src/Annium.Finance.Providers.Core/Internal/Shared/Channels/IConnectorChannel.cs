using System;

namespace Annium.Finance.Providers.Core.Internal.Shared.Channels;

/// <summary>
/// Fans values written by a connector out to the subscribers of one of its observables.
/// </summary>
/// <remarks>
/// Two implementations, chosen by <see cref="Core.Shared.ConnectorDelivery"/>: <see cref="ChannelPair{T}"/>
/// hands values to a background pump, <see cref="InlineChannel{T}"/> delivers them on the writer's thread.
/// Both hold values written before the first <see cref="Connect"/> and deliver them when it happens, so a
/// connector can write during its own construction.
/// </remarks>
/// <typeparam name="T">The type of value carried.</typeparam>
internal interface IConnectorChannel<T>
{
    /// <summary>Gets the observable subscribers see the written values on.</summary>
    IObservable<T> Observable { get; }

    /// <summary>Writes a value.</summary>
    /// <param name="value">The value to write.</param>
    void Write(T value);

    /// <summary>Starts delivering written values, including any held from before.</summary>
    /// <returns>A disposable that stops delivery.</returns>
    IAsyncDisposable Connect();
}
