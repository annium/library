# Important Notice — COIN-M / USDⓈ-M Architecture Integration

## Background

COIN-M Futures (CM / DAPI) is being integrated into the unified architecture shared with USDⓈ-M
Futures (UM / FAPI). After the migration, several COIN-M REST endpoints, WebSocket streams, and
account-level behaviors are aligned with the existing USDⓈ-M conventions. This page summarizes every
user-facing change so you can adjust client logic accordingly.

> **Document published:** 2026-06-10 **Effective date:** Progressively rolled out since 2026-06-24,
> fully effective by 2026-06-30

> **Note:** The changes described in this document will be rolled out progressively — not all
> changes will take effect simultaneously. Individual changes may be enabled at different times
> after the initial effective date.

---

## A. Account-level changes

### A.1 `dualSidePosition` is unified across UM and CM

- After the migration, UM and CM share the **same** `dualSidePosition` setting.
- Calling `POST /fapi/v1/positionSide/dual` or `POST /dapi/v1/positionSide/dual` will flip the value
  for **both** UM and CM at once.
- If either side has any open order or open position, switching is rejected with:
  - `-4067` — _"Position side cannot be changed if there exists open orders."_
  - `-4068` — _"Position side cannot be changed if there exists position."_
- **Action required:** before flipping `dualSidePosition`, ensure both UM and CM have no open orders
  and no open positions.

### A.2 STP setting follows UM

- Self-Trade Prevention configuration is taken from the UM side. The CM-side standalone STP setting
  is no longer effective.

### A.3 UM and CM share the same rate-limit pools

- **IP request weight (2400 / min)**: UM and CM share a single 2400 weight budget per IP per minute.
  Requests on either `fapi` or `dapi` count against the same `X-MBX-USED-WEIGHT-1M` counter.
- **Account order rate limit**: UM and CM share a single order budget — **1200 / minute**
  (`X-MBX-ORDER-COUNT-1M`) and **300 / 10 seconds** (`X-MBX-ORDER-COUNT-10S`). Order placements on
  either `fapi` or `dapi` consume from the same counters.
- Plan client traffic accordingly: previously isolated weight / order quotas across UM and CM are
  now merged into one.

---

## B. Maintenance window behavior (during the cutover only)

During the maintenance window:

- **UM `dualSidePosition` cannot be changed.** Requests to `POST /fapi/v1/positionSide/dual` or
  `POST /dapi/v1/positionSide/dual` return error `-1016`.
- **CM `lastPrice` is frozen** (unchanged). `indexPrice` and `markPrice` continue to update.
- **Strategy products (Arbitrage / Grid)** on CM are paused:
  - Cannot create new CM Arbitrage bots; existing CM Arbitrage bots cannot reduce size.
  - All CM Grid actions are blocked: create / add margin / withdraw margin / close / cancel.
  - For CM Grid configured with `markPrice` trigger, even if `markPrice` crosses the trigger price
    during the window the grid will **not** fire — it will trigger after the window ends if the
    threshold is still crossed.
- **Portfolio Margin (PM)** during the window:
  - **When a PM account triggers liquidation during the window:**
    - If the user has **no** CM balance/position, liquidation is processed normally.
    - If the user **has** CM balance/position, the UM positions will be settled and assets may be
      processed, while the CM portion is **liquidated after the window ends**. As a result,
      single-leg liquidation may occur (in particular, an arbitrage position may be left with one
      leg liquidated) — the resulting risk is borne by the user.
  - PM **account opening / closing / auto-exchange / close-position** are paused during the window.
- **Error codes returned during the window** may include any of:
  - `{"code":-1016,"msg":"This service is no longer available."}`
  - `{"code":-1016,"msg":"Service is under maintenance."}`
  - `{"239999": "The system is under maintenance, please try again later."}`
  - `{"code":-1109,"msg":"Invalid account."}`

---

## C. REST API changes

### C.1 `avgPrice` / `cumQuote` / `cumBase` removed from order placement responses

The immediate response of order placement / modification / cancellation no longer carries `avgPrice`
and the cumulative quote/base value. These fields were always `0` in the placement ack (fills happen
asynchronously); the actual fill price is still available via the order query / userTrades
endpoints.

**Affected (CM / DAPI — `cumBase` removed):**

- `POST /dapi/v1/order`
- `POST /dapi/v1/batchOrders`
- `PUT /dapi/v1/order`
- `PUT /dapi/v1/batchOrders`
- `DELETE /dapi/v1/order`
- `DELETE /dapi/v1/batchOrders`

**Affected (UM / FAPI — `cumQuote` removed):**

- `POST /fapi/v1/order`
- `POST /fapi/v1/batchOrders`
- `PUT /fapi/v1/order`
- `PUT /fapi/v1/batchOrders`
- `DELETE /fapi/v1/order`
- `DELETE /fapi/v1/batchOrders`

> Read endpoints (`GET /fapi/v1/order`, `/allOrders`, `/userTrades`, etc.) are **not** affected —
> they still return `avgPrice` and `cumQuote` / `cumBase`.

### C.2 `PUT /dapi/v1/order` and `PUT /dapi/v1/batchOrders` — `price` and `quantity` are both required

After the migration, these PUT endpoints require **both** `price` **and** `quantity`. Sending only
one returns `-1102`. For batch, missing field on any element yields element-level `-1102`.

### C.3 The following endpoints can now accept both UM and CM symbols

- `GET /fapi/v1/klines`
- `GET /fapi/v1/continuousKlines`
- `GET /fapi/v1/indexPriceKlines`
- `GET /fapi/v1/markPriceKlines`
- `GET /fapi/v1/premiumIndexKlines`
- `GET /dapi/v1/klines`
- `GET /dapi/v1/continuousKlines`
- `GET /dapi/v1/indexPriceKlines`
- `GET /dapi/v1/markPriceKlines`
- `GET /dapi/v1/premiumIndexKlines`

### C.4 `GET /fapi/v1/assetIndex` renamed to _Asset Index_ and now includes CM settlement-asset price index

- The display name is changed from _Multi-Assets Mode Asset Index_ to **Asset Index**.
- The HTTP path `/fapi/v1/assetIndex` is **unchanged**; no client code change is required for the
  call itself.
- The response now additionally includes COIN-M settlement-asset price index entries — e.g.
  `BTCUSD`, `ETHUSD`, `BNBUSD`, `SOLUSD`. Existing UM-side entries remain.

### C.5 New CM Algo Order endpoints

COIN-M now exposes the same algo-order surface as UM:

- `POST /dapi/v1/algoOrder` — place a CONDITIONAL algo (STOP / TAKE_PROFIT / STOP_MARKET /
  TAKE_PROFIT_MARKET / TRAILING_STOP_MARKET)
- `GET /dapi/v1/openAlgoOrders` — list open algo orders
- `DELETE /dapi/v1/algoOrder` — cancel an algo order (by `algoId` or `clientAlgoId`)

### C.6 Plain `place` order endpoints no longer accept stop-type orders

After a delayed effective period following the migration (the exact effective time will be announced
separately), the following endpoints will reject stop-type `type` values (`STOP_MARKET` /
`TAKE_PROFIT_MARKET` / `STOP` / `TAKE_PROFIT` / `TRAILING_STOP_MARKET`) with:

```json
{
  "code": -4120,
  "msg": "Order type not supported for this endpoint. Please use the Algo Order API endpoints instead."
}
```

**Affected:**

- `POST /dapi/v1/order` (single)
- `POST /dapi/v1/batchOrders` (per element)
- WebSocket API: `order.place` on the COIN-M ws-dapi endpoint

**Action required:** route stop-type orders through the new `/dapi/v1/algoOrder` endpoint described
in C.5.

### C.7 DAPI error code changes

| Endpoint                  | Scenario         | Before           | After   |
| ------------------------- | ---------------- | ---------------- | ------- |
| `GET /dapi/v1/openOrders` | Invalid `symbol` | `200` (silently) | `-1121` |

### C.8 DAPI request weight changes

| Endpoint                       | Before      | After                                      |
| ------------------------------ | ----------- | ------------------------------------------ |
| `GET /dapi/v2/leverageBracket` | `1` (flat)  | `1` with `symbol` / `2` without `symbol`   |
| `GET /dapi/v1/allOrders`       | `20` / `40` | `5` (flat)                                 |
| `GET /dapi/v1/userTrades`      | `20` / `40` | `5` (flat)                                 |
| `GET /dapi/v1/forceOrders`     | `20` (flat) | `20` with `symbol` / `50` without `symbol` |

---

## D. WebSocket changes

### D.1 WebSocket API — `order.place` / `order.modify` / `order.cancel` immediate response no longer returns `avgPrice` / `cumQuote` (UM) / `cumBase` (CM)

Same rationale as C.1 — the immediate response is a placement acknowledgement, fills are
asynchronous.

### D.2 WebSocket API — `order.modify` requires both `price` and `quantity`

Sending only one returns `-1102`.

### D.3 `!assetIndex@arr` stream renamed to _Asset Index_ and additionally pushes COIN-M settlement-asset price index

- Display name changed from _Multi-Assets Mode Asset Index_ to **Asset Index**.
- Stream key (`!assetIndex@arr`) is **unchanged** on the wire; existing subscriptions continue to
  work without code changes.
- The stream now additionally pushes COIN-M settlement-asset entries (e.g., `BTCUSD`, `ETHUSD`,
  `BNBUSD`, `SOLUSD`).

### D.4 Streams that gain a new `st` field on each payload

`st` distinguishes the symbol type: `1` = UM, `2` = CM.

- `!miniTicker@arr`
- `!ticker@arr`
- `!bookTicker`
- `!forceOrder@arr`
- `!contractInfo`
- `<symbol>@depth<levels>` (incl. `@500ms` / `@100ms`)
- `<symbol>@aggTrade`
- `<symbol>@ticker`
- `<symbol>@bookTicker`
- `<symbol>@miniTicker`
- `<symbol>@rpiDepth`

For the all-market arrays (`!*@arr`, `!bookTicker`, `!contractInfo`), both `fstream` and `dstream`
deliver the same merged UM + CM content.

### D.5 UM-side single-symbol streams add a `ps` (pair symbol) field, aligning with the CM payload shape

Affected:

- `!miniTicker@arr`
- `<symbol>@miniTicker`
- `!ticker@arr`
- `!forceOrder@arr`
- `<symbol>@depth<levels>` (incl. `@500ms` / `@100ms`)
- `!bookTicker`
- `<symbol>@bookTicker`
- `<symbol>@rpiDepth@500ms`

### D.6 markPrice / kline stream family — cross-host subscription

Both `fstream` and `dstream` may subscribe to the streams below.

**markPrice streams** — payload gains a new `st` field (`1` = UM, `2` = CM):

- `<symbol>@markPrice`, `<symbol>@markPrice@1s`
- `<pair>@markPrice`, `<pair>@markPrice@1s`
- `!markPrice@arr`, `!markPrice@arr@1s`

> CM-side `!markPrice@arr` / `!markPrice@arr@1s` standalone doc pages will be added in a future
> release.

**UM-side kline streams** — cross-host subscription enabled:

- `<symbol>@kline_<interval>`
- `<pair>_<contractType>@continuousKline_<interval>`

**CM-side kline streams** — cross-host subscription enabled:

- `<symbol>@kline_<interval>`
- `<pair>_<contractType>@continuousKline_<interval>`
- `<pair>@indexPriceKline_<interval>`
- `<symbol>@markPriceKline_<interval>`

---

## Action Required

1. **Account-level**
   - Verify your `dualSidePosition` setting after the migration; flip on either side affects both.
   - Make sure both UM and CM are clean (no open orders, no open positions) before changing
     `dualSidePosition`.

2. **REST**
   - Adjust client code that reads `avgPrice` / `cumQuote` / `cumBase` from order placement / modify
     / cancel responses — fetch fill data via `GET /{f,d}api/v1/order` or `userTrades` instead.
   - When calling `PUT /dapi/v1/order` or `PUT /dapi/v1/batchOrders`, always supply both `price` and
     `quantity`.
   - Update DAPI error handling — `GET /dapi/v1/openOrders` with an invalid `symbol` now returns
     `-1121` (was a silent `200`).
   - Migrate stop-type orders on COIN-M from `POST /dapi/v1/order` / `batchOrders` /
     `ws-dapi order.place` to the new `/dapi/v1/algoOrder` endpoint.

3. **WebSocket**
   - On streams listed in D.4 / D.5 / D.6, expect new fields `st` and/or `ps` in the payload. Add
     tolerance for these new fields in your parser.
   - Plan for the merged universe on `!ticker@arr` / `!miniTicker@arr` / `!bookTicker` /
     `!forceOrder@arr` / `!contractInfo` — both UM and CM symbols will be received.

4. **Maintenance window**
   - Pause CM strategy / PM operations during the announced window. Expect `-1016` / `239999` /
     `-1109` errors during the window.
