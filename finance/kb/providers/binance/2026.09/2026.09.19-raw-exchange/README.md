# Raw exchange answers — 2026-09-19

Every byte here came off the live USD-M futures account, captured because a filled order came back with an
executed price of zero and "the field was removed" is a reading of a document rather than a measurement.

One market order was placed and closed while the user data stream was open, and every answer about it was
kept: the placement, the same order queried back, the trades, the order history, and every stream message.

## What the capture settles

**The placement acknowledgement carries no executed price, under any name.** Not `avgPrice`, not
`cumQuote`, not anything else — the question was whether we were reading a renamed field, and the answer is
that there is no field to read. Compare `cap.place-RESULT.200.json` against `cap.query.200.json`, which are
the same order seconds apart:

| field | placement | query |
|---|---|---|
| `executedQty` | `"4.9"` | `"4.9"` |
| **`cumQty`** | `"4.9"` | absent |
| `avgPrice` | **absent** | `"1.1359000"` |
| `cumQuote` | **absent** | `"5.5659100"` |

`cumQty` is new and is not a price: it repeats `executedQty`. So the placement answer says *how much*
filled and refuses to say *at what*.

**The price arrives on the stream, within the same second.** `cap.stream.03.json` is an
`ORDER_TRADE_UPDATE` with `X: FILLED` and `ap: 1.1359` — the field this module's order-update converter
already reads. So nothing is lost; it arrives by a different road, slightly later, and a caller that waits
for the connector's order stream rather than the placement result has it.

**An event we do not handle at all.** `cap.stream.00.json` is `TRADE_LITE`, which arrives *before*
`ORDER_TRADE_UPDATE` and carries the fill: `L` last price, `l` last quantity, `t` trade id, `i` order id,
`c` client order id. It is the earliest notice of a fill the exchange gives, and nothing in this module
reads it.

**`limit` means opposite things on two neighbouring endpoints.** `userTrades.limit5.json` holds the five
**oldest** trades of the account; `userTrades.limit1000.json` holds all eleven, ascending, newest last.
Meanwhile `cap.allOrders.200.json` with `limit=3` holds the three **newest** orders. So on the trade
endpoint `limit` truncates from the *start* of the window, and on the order endpoint from the *end*.

The consequence is the shape everything else here has: a trade loader asking for "the latest page" with a
limit gets the earliest one, and notices nothing, because a full page of real trades is exactly what it
expected. This module asks for `1000`, so it is only exposed on an account with more than a thousand trades
in the window — where it would go wrong silently and permanently.

## Files

| file | what it is |
|---|---|
| `cap.ticker.200.json` | the price the order was sized from |
| `cap.place-RESULT.200.json` | `POST /fapi/v1/order` with `newOrderRespType=RESULT` |
| `cap.query.200.json` | `GET /fapi/v1/order` for the same order |
| `cap.trades.200.json` | `GET /fapi/v1/userTrades?limit=5` |
| `cap.allOrders.200.json` | `GET /fapi/v1/allOrders?limit=3` |
| `cap.close.200.json` | the reduce-only market order that closed the position |
| `cap.stream.00..07.json` | every user data stream message, in order |
| `userTrades.limit5.json`, `userTrades.limit1000.json` | the two halves of the `limit` finding |
| `trigger.allAlgoOrders.200.json` | the algo history after a real trigger, kept beside the rest |

The account was left flat and the balance is unchanged but for fees and price movement.
