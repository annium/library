---
title: Spot fill capture — 2026-09-21
type: capture
status: final
created: 2026-09-21
---

# Spot fill capture — 2026-09-21

Every answer this venue gave about two real round trips on `DOTUSDT`, recorded by `FillProbeTests` in
the spot test project. The first thing on this market type that has ever been made to fill: the trading
block prices every order away from the market by design, so until this capture the events an execution
raises were pinned against a payload assembled by hand from the documentation, with every fill field in
it zero.

Each round trip buys about 7 USDT and sells the same quantity back within seconds. The account ended as
it began but for fees, and a dust remainder of 0.00415 DOT — the fee on the buy is taken in the asset
bought, and what is left of it is below the lot step and cannot be sold.

## The files

| prefix | what |
|---|---|
| `market.*` | a market buy and the market sell that returned it |
| `limit.*` | a limit buy priced 0.5% through the ask, and the limit sell that returned it |

Within each: `account-before` / `account-after` (`GET /api/v3/account`), `book`
(`GET /api/v3/ticker/bookTicker`), `buy` and `sell` (`POST /api/v3/order` with
`newOrderRespType=FULL`), `buy.query` (`GET /api/v3/order`), `trades` and `trades-after`
(`GET /api/v3/myTrades`), `cleanup` (`DELETE /api/v3/openOrders`), and `stream.NN` — the account
stream's events in arrival order, unwrapped from their envelope by the stream, which is the form the
connector ingests.

The suffix before `.json` is the HTTP status. `market.cleanup.400.json` and `limit.cleanup.400.json` are
the refusal of a cancel-all on a symbol with nothing open — the venue's answer, kept deliberately.

## What was redacted, and why

This repository is public. The account answers — `*.account-before` and `*.account-after` — carry the
account's numeric id, its withdrawal and trading permissions, and 842 balance rows. They are kept for
the shape of the answer, with the id replaced by `0` and the zero balances dropped; each says so in a
`_redacted` field it did not arrive with. Nothing else in this directory was touched: order ids, trade
ids, prices, quantities and fees are the venue's answers verbatim, and they are what the captures are
for.

The unredacted answers were not committed. If they are needed again, the probe produces them in one
run.

## What the bytes say

- **Two events per order, not one.** `x:"NEW"` with `X:"NEW"` on acceptance, then `x:"TRADE"` with
  `X:"FILLED"` carrying the execution. A caller reading only the first sees an order accepted and never
  filled.
- **The acceptance event carries `"n":"0"` and `"N":null`** — a null commission asset, and an amount
  spelled bare rather than padded. Every hand-written fixture had spelled both otherwise.
- **A marketable limit fills at the book's price, not its own.** The limit buy was priced 1.202 and
  filled at 1.197. `price` in the answer is what was asked for; `cummulativeQuoteQty / executedQty` is
  what it cost.
- **The fee's asset depends on the side**: the base asset on a buy, the quote asset on a sell.
- **`outboundAccountPosition` carries only the balances that changed** — three assets here, while the
  account holds five. It is a delta, not a snapshot. Our connector reads none of its balances: it treats
  the event as a signal to reload the account, which is what makes the distinction harmless. Anything
  that later published from the event directly would erase every balance it does not mention.
- **The stream's own `executionReport` and the placement's `fills` agree** on price, quantity, fee and
  trade id, so either could be the source of a trade. The connector deliberately uses neither: it asks
  for the symbol's trades, so that a fill reaches a caller once.
