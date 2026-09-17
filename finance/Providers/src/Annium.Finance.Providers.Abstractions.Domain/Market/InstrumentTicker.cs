namespace Annium.Finance.Providers.Abstractions.Domain.Market;

/// <summary>
/// Represents the current best bid and ask price for an instrument.
/// </summary>
/// <remarks>
/// <para>
/// A value type. Tickers arrive at the rate the market moves and are consumed immediately — the live path
/// allocated one per message, and a backtest materializes 1.77 million of them into an array it holds for
/// the whole run. Neither keeps a ticker for its identity; both want the three numbers.
/// </para>
/// <para>
/// The size is what makes that safe to say: a string reference and two decimals, forty bytes, copied
/// rather than chased. What it is not safe to assume is that a field holding one publishes atomically the
/// way a reference did — a forty-byte write is not a single store, so a reader can see one update's bid
/// beside another's ask. Every shared slot that holds a ticker has to publish it explicitly; crypted's
/// three were made to do so before this change landed.
/// </para>
/// </remarks>
/// <param name="Symbol">The instrument's trading symbol.</param>
/// <param name="BidPrice">The highest price a buyer is currently willing to pay.</param>
/// <param name="AskPrice">The lowest price a seller is currently willing to accept.</param>
public readonly record struct InstrumentTicker(string Symbol, decimal BidPrice, decimal AskPrice)
{
    /// <summary>
    /// Gets the instrument's trading symbol.
    /// </summary>
    /// <remarks>
    /// Empty rather than null on a <c>default</c> instance. A value type can be brought into existence
    /// without its constructor — an array element, an uninitialized field — and there the symbol would be
    /// a null the type's own signature says cannot happen. Callers compare and log it without asking.
    /// </remarks>
    public string Symbol
    {
        get => field ?? string.Empty;
    } = Symbol;

    /// <summary>Gets a placeholder ticker with an empty symbol and zero prices.</summary>
    /// <remarks>Equal to <c>default</c>, so the two cannot disagree about what "no ticker" looks like.</remarks>
    public static InstrumentTicker Empty { get; } = new(string.Empty, 0, 0);

    /// <summary>Returns the symbol and bid/ask spread as a string.</summary>
    /// <returns>A string in the form "Symbol: BidPrice - AskPrice".</returns>
    public override string ToString() => $"{Symbol}: {BidPrice} - {AskPrice}";
}
