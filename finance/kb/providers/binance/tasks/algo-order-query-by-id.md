---
title: A conditional order queried by id answers "order does not exist"
type: task
status: open
provider: binance
market-type: usd-futures
opened: 2026-09-19
blocks: nothing
---

# A conditional order queried by id answers "order does not exist"

## What happens

`GET /fapi/v1/algoOrder?algoId=<id>` answers the error the venue spells `-2013` — the code its reference
gives for an order that does not exist — for an order that demonstrably does. The same order is listed by
`GET /fapi/v1/openAlgoOrders` in the same second, and cancelling it by the same id succeeds.

Measured 2026-09-19 against a real conditional order placed by the write probe, reproduced, and captured:
[`2026.09/2026.09.19-algo-probe/algoOrder.query.400.json`](../2026.09/2026.09.19-algo-probe/algoOrder.query.400.json),
beside the placement and cancellation that bracket it.

## Why it is open rather than fixed

Nothing depends on the answer. Ingestion keys on the listing endpoint, which works and is pinned; the
cancellation path carries the order's own type and does not query first. The single-order query is the
only caller, and it has none.

It is recorded rather than dropped because the obvious future change — fetching one conditional order by
id, for a reconciliation or a status check — would walk into it, and the hours would go into reading our
own code first.

## What would settle it

Three readings, in the order they cost:

1. **A parameter the endpoint wants and we do not send.** Compare the request against the reference for
   the query endpoint specifically rather than against the placement's, which is what a port would copy.
2. **An id that is not the id.** The placement answers an `algoId`; check the listing's spelling of the
   same order's identifier, and whether the query wants a client id instead.
3. **The endpoint is stale.** The conditional-order family moved once already, with the announcement
   sitting in a changelog this module had fetched. A sweep of the snapshot for the query path would say.

Do it against the stored snapshot first; a live re-probe costs an order.
