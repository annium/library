namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;

/// <summary>The envelope Binance wraps combined-stream WebSocket payloads in.</summary>
/// <typeparam name="T">The type of the wrapped payload.</typeparam>
/// <param name="Name">The name of the stream/topic the payload was received on, e.g. <c>btcusdt@bookTicker</c>.</param>
/// <param name="Data">The wrapped payload.</param>
/// <remarks>
/// A value type because it exists for one statement: the envelope is opened and <c>Data</c> is taken out,
/// on every message of a stream. Allocating an object to carry a payload one line further is a cost paid
/// per message for a wrapper nothing keeps.
/// </remarks>
public readonly record struct StreamData<T>(string Name, T Data);
