namespace Annium.Finance.Providers.Core.Shared;

/// <summary>
/// What a connector's buffer owes the values written into it while the consumer is behind.
/// </summary>
/// <remarks>
/// The right answer differs by stream, and pretending it does not is how one bad compromise gets made for
/// all of them. It is a property of what the values mean, not of the provider producing them, so the
/// connector bases choose it rather than the connectors.
/// </remarks>
public enum ConnectorBuffer
{
    /// <summary>
    /// Every value is kept. The buffer is unbounded, and a consumer falling behind is reported rather than
    /// silently absorbed.
    /// </summary>
    /// <remarks>
    /// For streams where losing one value corrupts the consumer's state — a change event describing an
    /// order, a position, a balance. There is no correct value to drop, and blocking the writer would
    /// stall the socket the values arrive on, so the only honest option is to keep them and be loud about
    /// the queue growing.
    /// </remarks>
    Complete = 0,

    /// <summary>
    /// Only the most recent values are kept; the oldest are dropped once the buffer is full, and the count
    /// of what was dropped is reported.
    /// </summary>
    /// <remarks>
    /// For streams where a value is superseded by the next one — a ticker's best bid and ask. A consumer
    /// that is behind wants the current price, not the one from ten thousand updates ago, and an unbounded
    /// queue of stale prices is a memory leak wearing a useful disguise.
    /// </remarks>
    Recent = 1,
}
