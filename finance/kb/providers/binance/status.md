---
title: Binance provider status
type: provider-status
status: living
created: 2026-09-01
---

# Provider status — binance

> **Living. Changes when we do work.** The contract itself is in `manifest.md`, deliberately apart.

## Meta

- provider: `binance`; market types: spot, usd-futures
- manifest: [`manifest.md`](manifest.md)
- docs revision: spot `828ca74b809cfedbd5602df328b5f706368d483b` (the 2026-09-18 snapshot; this line
  still named the 2026-09-01 one until 2026-09-21); futures fetched 2026-09-18 (no repository exists,
  so the date is the only anchor)
- working branch: `main` where converged
- last reconciled: 2026-09-21 — **spot brought level with futures through steps 3, 4 and 5's offline
  half, and its trading block written.** Step 5 for spot began by finding that its account stream could
  not be ported: the key mechanism is deprecated and gone from the reference, and the replacement is a
  different transport. That is `manifest.md` §11, collected first. **No spot stage has been run live**,
  so nothing in §11 is `live`. Before it, on 2026-09-20: step 5 complete on futures, the write block
  clean three times, and a post-convergence defect fixed

## Convergence

| step | state | evidence | outstanding |
|---|---|---|---|
| 1 — derive existing state | **converged, re-run 2026-09-18** | re-anchored against the tree after steps 3-5: of 107 line-anchored facts 76 resolved and 31 had moved, no file gone. 21 facts present in the code and absent from the manifest were added, three new `[DEAD]` entries recorded, and the verification census corrected in ~13 places. The manifest asserted the opposite of the code in one place — "Nothing reads `Retry-After`" — and contradicted itself in another, over the decay constants | none on this axis. The four test defects it turned up are queued below for step 5 |
| 2 — collect facts, compute drift | **converged, re-run 2026-09-18, with one accepted gap** | 13 futures pages, 7 spot files, both Postman collections, the per-endpoint API reference and one page reached by following a link out of the changelog. Every category given an outcome **with a citation into the snapshot** — the new rule, added to the skill during this run. Three mechanical sweeps ran before any reading by judgment | **one accepted gap**: the nested user-data-stream payloads (~20 short field names, now including `ALGO_UPDATE`'s) are `unretrievable` — they render from a schema embed present in neither index file. The `avgPrice` gap is **closed**, settled from documentation rather than by a live order. One `contested` left, `MAX_NUM_ALGO_ORDERS`, settleable by logging one `exchangeInfo` response |
| 3 — wire types and serialization | **converged; spot re-run 2026-09-21** | assessed 2026-09-17 against the manifest by a fresh verifier: field coverage holds in both directions (every documented field read, no field read that is not documented), all five enumeration tables map both ways, the kline indices are pinned by six distinct values. Two branch gaps and one defect remediated the same day — see the run report. **Spot reconciled separately 2026-09-21** against the same checklist: a documented status that threw rather than mapped, the same guarded quotient unexercised in three converters, and five drop rules nothing named — nine facts moved, six mutation-checked and three held by the compiler. See [the report](2026.09/2026.09.21-step-3-spot.md) | none |
| 4 — provider, read paths (+ registration, config, read-only live validation) | **converged on both venues; spot 2026-09-21** | every read path on both venues driven offline, failure paths included; endpoints pinned; **live read block green on 2026-09-16: 20 tests, 14 passed, 6 skipped, none failed** — and this time with credentials present, so the two signature tests ran rather than skipping | none. The upstream defect found here — an exchange error discarded when the success type is a collection — was fixed in `Annium.Net.Http` 1.1.49 and taken up with the package bump; the test that pinned the loss now pins the reason. Spot's `UserProviderTests` were the six skipped as `Not implemented`; the paths they drive were built on 2026-09-21 and they run now |
| 5 — connector, streams and orders (+ registration, config, trading live validation) | **converged 2026-09-20 on usd-futures; spot converged offline 2026-09-21, with its trading block built and never run** | the test lib gained a websocket server; the streams are pinned by 20 offline tests, the order lifecycle by 13 more, and every registration key by two. Six defects found and fixed in the process — see the [run report](2026.09/2026.09.18-step-5.md) | **spot has never been validated live.** The offline half is done — the stream, the commands and the ingestion are pinned by 35 tests and mutation-checked ([report](2026.09/2026.09.21-step-5-spot.md)) — and the trading block exists as of the same day, three stages of it, **not run** ([report](2026.09/2026.09.21-step-5-spot-trading-block.md)). Nothing in §11 is `live`. On usd-futures: none. **5f's fourth stage went green on 2026-09-19** once the conditional-order migration landed, and the whole write block then ran end to end **three times clean** - 8 of 8 each time, no connector errors, the account flat after each, and the three runs identical in what they traded. See [the run report](2026.09/2026.09.19-trading-runs.md). **5e validated live on 2026-09-18**: 21 tests, 15 passed, 6 skipped, none failed, run twice - including a new read-only user connector test that connects, takes a listen key, opens the stream and receives the account snapshot without placing anything. The two behaviours that needed a decision have one: a refused leverage change now reaches the caller, and one the exchange accepts without applying is refused by the connector itself; a limit order asked to become a market order stays refused |
## Queued work

Open items only. Two things that used to live here are settled and their reasoning is where it belongs
now: the signing property is pinned by `HttpRequestSignatureExtensionsTests` (the golden value could never
have seen it — it signs a literal, so no query is composed), and the read-side enumeration gaps are closed
by `WireMappingTests` on both venues. Rate-limit handling in the runtime shipped in 1.3.0–1.3.4. The
manifest carries what each of them now defends; the run reports beside this file carry how it went.

| left | where it stands, verified 2026-09-18 |
|---|---|
| **MIGRATE THE CONDITIONAL ORDER FAMILY TO `/fapi/v1/algoOrder`** — **in progress 2026-09-19.** Done: the status mapping (derived from a live trigger), the response converter, placement routing, cancellation routing (the request now carries the order's type), and open orders merged from both stores. **Done as of 2026-09-19**, offline: status mapping, response converter, placement and cancellation routing, open orders and history merged from both stores, and `ALGO_UPDATE` ingestion from a captured payload. **Live stage 4 is GREEN as of 2026-09-19** - the stage blocked since 2026-09-18, and the reason this whole pass was opened. A real position opened, real stop-loss and take-profit placed through `/fapi/v1/algoOrder`, and the account left flat.

Its first run stopped before the conditional orders: the market order that opens the position came back `FILLED` with an executed price of zero, and the assertion added on 2026-09-18 caught it exactly as predicted. That run failed before its own teardown and **left a position open**, closed by hand - the write block's own warning, met rather than avoided.

**Decided 2026-09-19:** the executed price is asserted on the order as the connector reports it, never on the command's answer. A placement acknowledgement says how much filled and refuses to say at what - measured byte for byte, there is no price field under any name - while the price reaches a caller on the order stream within the same second. The assertion moved to where the venue answers rather than being weakened: a venue reporting no fill price at all still fails it | **Answered by the documentation pass; this is now implementation work, not a question.** The five types (`STOP_MARKET`, `TAKE_PROFIT_MARKET`, `STOP`, `TAKE_PROFIT`, `TRAILING_STOP_MARKET`) moved on 2025-12-09 and `POST /fapi/v1/order` refuses them with `-4120`. Full request shape, endpoint list and the three behavioural changes are in `manifest.md` §5, cited to the snapshot. **Not** `/sapi/v1/algo/futures/*`, which the error message's wording suggests and which is a different product. Shape of the work: step 3 gains converters for the algo responses and the `ALGO_UPDATE` event; step 4/5 route the four `OrderType`s through the new endpoints and re-map `algoId`/`clientAlgoId`. ~~**One blocker of its own:** the response schemas are `unretrievable`~~ — **cleared 2026-09-19.** The probe placed, listed and cancelled a real conditional order; every raw answer is stored in `2026.09/2026.09.19-algo-probe/` and written up in `manifest.md` §5. What it found beyond the schemas: `orderType`/`algoStatus` where the order endpoint says `type`/`status`; `algoId` is a **number**; the list carries `actualOrderId`/`actualQty`/`isActivated` that the placement answer does not; the cancel answers `code` as a **string** `"200"`, which the existing `OperationResult` would fail to parse; and `GET /fapi/v1/algoOrder?algoId=` answered **`-2013 Order does not exist.`** for an order that did exist — unresolved, so ingestion should key on `openAlgoOrders`, the endpoint proven to work. **And one more, found 2026-09-19 because the order could not be found in the account's order history:** a conditional order never enters the ordinary order store - it is in `allAlgoOrders` as `CANCELED` and absent from `allOrders` entirely - so `LoadOrdersAsync`, which reads `allOrders`, needs a second source per symbol or it loses every conditional order silently. Three response shapes across the four endpoints, not one |
| ~~the `avgPrice` question waits on one live run~~ — **settled from documentation 2026-09-18** | `avgPrice` and `cumQuote` are removed from the placement / modify / cancel responses, and the notice states they *"were always `0` in the placement ack (fills happen asynchronously)"* — `coin-futures_Important-CM-UM-Integration-Notice.md:80-106`. So the converter has been reading a field that was zero before it was absent, and the fill price must come from the order query or `userTrades`. **Consequence to expect:** the `ExecutedPrice > 0` assertion added on 2026-09-18 (`Tests.Lib/User/UserConnectorTestBase.cs`) will **fail** on the next stage-4 run. That is the assertion doing its job; the fix is to read the price from where it now lives, not to weaken the assertion |
| ~~`MapOperationCode` folds negative codes to `BadRequest`~~ — **closed 2026-09-19** | classified by what a caller would do differently, from the documented list rather than a guess: transport, rate, access, absent, funds, refused. One shared table (`BinanceErrors.Classify`) consulted by both mappings, so the two copies cannot drift apart again — which they had. `MarketOperationStatus` gained `Forbidden` to make the access class expressible; it had none, which is why its HTTP map sent 401/403/404 to `UnknownError`. Every class pinned on both mappings. **Follow-up in `crypted`, not done here:** `MarketOperationStatusExtensions.IsWorthRetrying` defaults to `true`, so an access failure is retried forever — it was retried before this change too, as `UnknownError`, so nothing regressed, but `Forbidden` is now the member that would stop it | the reason it was deferred was that the real code list was missing and inventing a taxonomy would ship it to every caller, `crypted`'s order path included. `error-code.md` carries **206 codes in five families Binance groups itself** (`10xx` general/network, `11xx` request, `20xx` processing, `40xx` filters, `50xx` execution) — so the table is now a reading exercise, not a taxonomy exercise; the families are in `manifest.md` §6. And there is a **defect** behind it rather than only coarseness: `-1008 Request Throttled` falls to `BadRequest`, so a system-level throttle takes no pause in the limiter and the retry goes straight back into it. The ban-message regex cannot rescue it — that text states no deadline |
| ~~three `[UNVERIFIED]` markers~~ — **all three resolved, the last live 2026-09-19** | the rate-limit window **is** one minute (`common-definition.md:133-134`), so the hard-coded `-1m` header suffix is confirmed; the decay arithmetic resolved as **a copied constant** — 300/3000ms is spot's 6000/min put on futures' 2400/min ceiling, 2.5× too fast, with an XML doc claiming a match the arithmetic refuses. And the third, which the documentation never stated, is now `live`: a one-way account reports **one row per symbol regardless of whether a position is open, always `BOTH`** — 905 rows, 905 symbols, all zero, on a flat account |
| ~~two defects of ours, found by comparison~~ — **fixed 2026-09-19, and there were three** | (1) futures decay is `120` every 3000ms now, not spot's `300`. (2) the post-refusal pause caps at **3 days**, matching the documented maximum, not at 1 hour. (3) **the one the other two uncovered:** a refusal stating no deadline paused *nothing at all* — `Block` was called only for a positive pause while both deadline readers returned zero when the response said nothing. So the commonest refusal of all, a plain 429 or a throttle, cost the limiter nothing. It now stands down for the weight window it guards. A test asserted the old behaviour as correct and argued for it in its own summary; the argument does not survive being restated, since the weight accounting it deferred to is what the refusal has just disproved. All three mutation-checked |
| ~~**`userTrades` truncates `limit` from the wrong end**~~ — **fixed 2026-09-19** | `userTrades?limit=5` returns the five **oldest** trades; `allOrders?limit=3` returns the three **newest**. `LoadLatestTradesAsync` asked for the latest page with `limit=1000` and was safe only below a thousand trades in the window. It is bounded by **time** now - a one-hour window, which is what selects recency on an endpoint whose page cap selects the oldest - and the window is pinned by an offline test. The history path was never affected: it pages by `startTime` and the endpoint returns ascending |
| ~~**`TRADE_LITE` is unread**~~ — **read as of 2026-09-19** | a user-stream event carrying the fill that arrives **before** `ORDER_TRADE_UPDATE`. Read now, and deliberately **not published as a trade**: it carries no commission and no realised PnL, so a trade built from it would have a zero fee - not missing data a caller can see but wrong data it cannot. What it buys is starting the trades reload sooner, which is what the connector does with it. The fill's own price is `L`, where `p` beside it is the order's requested price and is zero on a market order; mutation-checked by reading the wrong one |
| ~~**two silent changes to live behaviour**~~ — **addressed 2026-09-19** | `GET /fapi/v1/userTrades` reach cut from 6 months to 3, so a back-fill past ~90 days returns empty pages indistinguishable from a quiet week; `allOrders` and `userTrades` now return **both** UM and CM rows, which we survive only by always sending `symbol`. The paging cursor no longer rests on the venue's ordering at all: it takes the **highest id** in a page rather than the last row, which is the same value on a sorted page and the correct one on any other. Pinned by a test that serves a deliberately unsorted page, mutation-checked. ~~`MAX_NUM_ALGO_ORDERS`~~ settled live 2026-09-19: absent from `exchangeInfo`, so nothing to read and nothing to implement |
| ~~no test file at all: `HttpRequestLogExtensions`~~ — **closed 2026-09-20**; the filter converters | the logging wrapper is pinned from both sides now - a header that must appear and one that must not - and the pass found its documentation asserting the opposite of its code: the header list is an allow-list, keeping the rate-limit headers and dropping every other, while three doc comments called it masking. The filter converters are covered through the exchange-info fixture, which the manifest says plainly - a missing file, not a missing fact. `WebSocketService` and `ListenKeyResolver` left this list on 2026-09-18: both are pinned offline now |
| ~~**three test defects, found by step 1 on 2026-09-18**~~ — **fixed 2026-09-19** | decided the day they were found: they are defects in tests, not in the contract, and step 1-2 specifies rather than repairs. (1) `Tests.Lib/User/UserConnectorReadTestBase.cs:82` asserts `positions.Count.IsGreaterOrEqual(0)` — a count is never negative, so the line cannot fail, and it is the only thing covering positions in the read suite. (2) `WireMappingTests.SmallTables_MapEveryMemberBothWays` on both venues is tautological: `StringToValue` is built as `ValueToString.ToDictionary(x => x.Value, x => x.Key)`, so renaming `"BUY"` to `"buy"` passes it in full. (3) `MarketProviderReadPathTests.LoadContext_GuessesAPrecisionForAssetsTheInstrumentsDoNotDescribe` asserts `8` in both of its cases and never reaches the `Contains("USD") → 2` branch its own summary claims to cover. All three are closed: the count comparison is replaced by an assertion on what the write fixture's precondition actually rests on; the tautological round trip keeps its place and gains a companion that names the literals, mutation-checked by renaming one; and the precision fixture gained an undescribed asset whose code contains USD, which is the branch that makes the heuristic a heuristic |
| ~~**the listen key's facts are carried by no test**~~ — **closed 2026-09-19** | the resolver has 6 offline tests and every exchange fact inside it is synthetic: a `/listenKey` stub endpoint, 50 ms in place of the `60_000`/`5_000` cadence, no assertion on the HTTP method, and a rate limiter that records nothing — so deleting `.WithRateDelay1M(...)` breaks nothing. The component is covered; the contract it carries is not. Step 5 work, and the cheapest of the four to close is the method |

**Step 5 is the work, not this list.** Everything offline is done as of 2026-09-18; the live stages are
what remains, and each is approved on its own.

Step 4's residue was worked through on 2026-09-18. Four of its six items are closed and left this table:
the paging fixture now spans three windows instead of one, the registered decay constants are pinned, the
missing-header error is silenced on the one path that answers without the header, the listen key request
is counted against the limiter, and the order-count headers are recorded as deliberately not tracked. What
remains needs either a live run or the documentation reopened.

The list above was a list of investigations and is now mostly a list of decisions: the header census is
done, the paging fixture's defect is named, the order-count answer exists. What is left to decide is
small and specific - a log level, whether to delete a fixture, whether to split an error mapping - and
none of it blocks the live stages.

Step 3 was the other half of that sentence until 2026-09-17, when it was assessed and found already
written: the code and its tests existed while this document called the step not started. That error is
the expensive direction — it invites rewriting what is already there, and losing tests that exist
nowhere else.

## Running the read block: `test.env` is copied, not read from source

Worth writing down because it cost a run and because the obvious check does not catch it.

`TestEnv` reads `test.env` from the process's working directory, which for a test run is
`bin/Release/net10.0/` — not the copy beside the `.csproj`. The project declares
`<None Update="test.env" CopyToOutputDirectory="Always" />`, so the two are kept in step **by building**,
and every `just test-*` recipe runs `--no-build`.

So editing `test.env` and running the block straight away tests the previous contents. On 2026-09-16 that
produced a signature mismatch whose expected value was the literal `test_expected_signature` — the
placeholder from `test.env.example`, still sitting in a copy made before the file was filled in. The
source file was correct the whole time, and a script that checked the source said so.

**Run `just build-finance` after touching `test.env`.** This is the stale-binary trap the fix-code reports
already record for `just test`, arriving through data rather than through code: the binary was current and
the file beside it was not.

The failure was at least legible — the assertion printed the literal, so the placeholder was recognisable
on sight. Had the placeholder been a plausible-looking hex string, the same run would have read as "our
HMAC disagrees with Binance" and sent someone into the signing code.

## Spot step 5 — built, 2026-09-21

The section below records where the step stopped when it opened; it was then built, and this is the
current statement. The facts and citations are in `manifest.md` §11, the run in
[the report](2026.09/2026.09.21-step-5-spot.md).

**What exists now.** A WebSocket API request builder, a second implementation of the account stream
speaking that protocol, and a connector that was five `NotImplementedException`s. 35 offline tests, 13
mutations, all killed.

**The trading block exists as of the same day, and has never been run.** Three stages: a read-only
connector test, four write cases, and a standalone cleanup tool in the probe block. See
[the second report](2026.09/2026.09.21-step-5-spot-trading-block.md). **No fact in §11 is `live`** and
none will be until a stage is approved and executed.

**The shared trading fixture could not be used**, and the reason generalises: it is built around positions
existing, so on a cash account its opening wait never ends and its mode check throws. The half worth
repeating is the cleanup — flattening is restorative on a margined account and destructive on a cash one,
where the balances *are* the holdings. Spot's fixture cancels orders and touches nothing else.

**The block trades nothing.** Every order is a limit buy at half the market, which rests until cancelled:
placement, stream reporting, balance locking, replacement and cancellation are all exercised, and nothing
is bought, sold or charged. A fill is not asserted live, and that is a decision rather than an oversight.

**One thing to carry forward.** The listen-key configuration that spot was filling and never resolving
is gone, and `UserConfigBase` no longer asks every venue for a mechanism only one of them has.

## Where step 5 stopped when it opened, 2026-09-21

Step 5 was opened for spot and stopped at its first task, which the connector skill makes the first
task: check that the contract covers what is about to be ported. It did not, and collecting the rest
turned up something that changes the step's shape rather than its size. The facts and their citations
are in `manifest.md` §11; what follows is only what it means for the work.

**Spot's user stream cannot be a port of the futures one.** The mechanism this module has machinery for
— a listen key fetched over REST, a socket opened at a URL ending in that key, the key kept alive on a
timer — is deprecated on spot and its documentation has been **removed**: `listenKey` and
`userDataStream` have zero occurrences in the spot REST reference. The replacement is not another URL,
it is another **transport**: a request/response WebSocket API at its own endpoint, where a subscription
is a method call and every event arrives wrapped in an envelope the existing converters do not expect.

Three things follow, and they are the reason this is a gate and not a note:

1. **`ListenKeyResolver` and `UserStream` are not reusable here**, and they are what steps 5b and 5c of
   the futures run spent most of their effort pinning. Spot needs a client for a request/response
   protocol — correlating replies by `id`, tracking a subscription id, handling `eventStreamTerminated`
   — which has no counterpart in the tree.
2. **The two spot stream converters need an envelope**, or every event fails to parse. They are marked
   `[DEAD]` and pinned, which is exactly the combination that makes this invisible: their tests feed
   them the bare object and will go on passing.
3. **Spot's registered listen-key configuration is dead weight pointed at a deprecated mechanism** —
   five sites in the spot module. Harmless only because nothing resolves it.

Independent of the stream, **`ModifyOrderAsync` cannot be a port either**: spot's amend endpoint reduces
quantity and nothing else, so a price change is a cancel-and-replace on this venue, at the back of the
queue and against a second rate budget the limiter does not model.

**The decision this stops at.** Step 5 for spot is a new client for a protocol this repository does not
speak, not a second wiring of an existing one — and whether that is worth building is the same question
the step-4 assessment stopped at, now with a price attached. Nothing else is blocked on it: spot's
market side works, and its four user read paths landed on 2026-09-21.

## Reconcile history

One line per run. The report holds the findings; the snapshot beside it holds the documentation those
findings were read from.

| date | layer | report | outcome |
|---|---|---|---|
| 2026-09-01 | 1-2 — contract | *(derived from code, no report)* | manifest inventoried; `checked_against: never` |
| 2026-09-01 | 1-2 — contract | [`2026.09/2026.09.01-contract.md`](2026.09/2026.09.01-contract.md) | **blocking drift**: futures WebSocket URLs decommissioned. Step 1 converged; step 2 complete but for the futures endpoint schemas. One unverified assumption settled in our favour; the sandbox environment removed from the code entirely |
| 2026-09-02 | 4 — provider | *(no report; the work is in the branch)* | every read path driven offline on both venues, failure paths included; **first live read run**, which failed on spot's server time path — `v1` where the exchange documents `v3`, an oddity the manifest had marked and never checked. Fixed and pinned; the block is green. Step 4 was called converged once before it was, on the strength of the live run alone — the checklist had three items left |
| 2026-09-02 | 4 — provider | *(no report; the work is in the branch)* | packages bumped to 1.1.49, closing the upstream union-parse defect this step found. The test that pinned the loss now asserts what the exchange actually said. `just update` could not be used: `.xs` points the tool at `api.pkg.annium.com`, which serves a certificate for `*.avito.ru` — versions were bumped by hand instead, and the registry is an infrastructure question outside this work |
| 2026-09-16 | 4 — provider | *(no report; the work is in [library#19](https://github.com/annium/library/pull/19))* | **credentials supplied, so the read block ran whole for the first time**: 20 tests, 14 passed, 6 skipped, none failed. The two signature tests had skipped since the module existed and now pass, which moves `TEST_EXPECTED_SIGNATURE` from `unchecked` to `live`. The six still skipped are Spot's `UserProviderTests`, marked `Not implemented` |
| 2026-09-16 | 4 — provider | *(same)* | the percent-encoding question closed by measurement rather than by rebuilding the golden value — see the settled item under queued work. A new offline test pins that a signed request signs the query it sends |
| 2026-09-18 | 1 — derive existing state | [`2026.09/2026.09.18-contract.md`](2026.09/2026.09.18-contract.md) | re-anchored against the tree: 31 of 107 anchors had moved, 21 facts were missing, 3 new `[DEAD]`. **One entry asserted the opposite of the code** (`Retry-After`), and the decay constants were recorded as both `none` and `pinned` in the same document. Four test defects found and queued for step 5 |
| 2026-09-19 | 2 — collect facts (live probe) | [`2026.09/2026.09.19-algo-probe/`](2026.09/2026.09.19-algo-probe/) | the three answers documentation could not give, read from the live exchange. **`MAX_NUM_ALGO_ORDERS` is absent** from `exchangeInfo` — the changelog was right and the reference page stale, so the `contested` row closes with nothing to implement. **`positions[]` confirmed exactly as assumed** — 905 rows, one per symbol, all `BOTH`, all zero on a flat account; last `[UNVERIFIED]` closed. **Algo response schemas captured** by placing, listing and cancelling one real conditional order (~5.5 USDT notional, trigger 4% away, never filled, balance unchanged). New findings inside that: `orderType`/`algoStatus` diverge from the order endpoint's `type`/`status`, the cancel answers `code` as a string, and query-by-`algoId` answers `-2013` for an order that exists |
| 2026-09-18 | 2 — collect facts, compute drift | [`2026.09/2026.09.18-contract.md`](2026.09/2026.09.18-contract.md) | **the blocking question is answered and it was documented all along.** Conditional orders migrated to `/fapi/v1/algoOrder` on 2025-12-09 — and that text was sitting in the *previous* run's own snapshot, missed by reading rather than by fetching. The retrieval gap that run called impossible is closed too: `llms.txt` and `llms-full.txt` exist, and the endpoint chapter is in the file it had already fetched. Also settled: `avgPrice`, the listen-key period, two of three `[UNVERIFIED]`. Found: two defects of ours (decay constant, pause cap), one error-mapping defect (`-1008` throttle read as a bad request), three silent live changes. **The skill was amended first** — three mechanical sweeps before any reading by judgment, and a citation required for every outcome |
| 2026-09-19 | 5 — connector (live trading) | [`2026.09/2026.09.19-trading-runs.md`](2026.09/2026.09.19-trading-runs.md) | **the write block run three times end to end, clean**: 8 of 8 each time, no connector errors, the account flat after each, and the three runs identical in what they traded. Getting there cost three defects, none of which looked like what it was. A websocket whose connect deadline and connection lifetime shared one cancellation source: an overrun handshake cancelled it, the scheduled reconnect then waited on that same cancelled token, and the socket stayed `Connecting` for good - one test died on a five-minute deadline and its still-polling connector exhausted the weight budget, failing the next test as `NotConnected`. The conditional-order list measured at 40 weight, the same as the unscoped order list, so the suite's one-second reload ran the block at 83% of the venue's 2400/min allowance and two runs in a row could not both fit; raised to fifteen seconds it peaks near 1100 and is no slower, which says the user stream was carrying the tests all along. And two assertions that failed a healthy connector, both only visible once the socket retried instead of hanging |
| 2026-09-20 | 5 — connector (a defect found after convergence) | *(no report; the work is in [library#40](https://github.com/annium/library/pull/40))* | a standalone write run failed one time in four on a conditional order that stayed `New` after being cancelled. **An order reported as over was being raised again**, by two routes that both leave the caller permanently wrong - the order is absent from every later snapshot and every later event, so nothing corrects it, and a caller would go on believing a stop loss was armed. First route: a snapshot older than the cancellation, the window measured at 270ms on the wire. Second: the venue's own events are not ordered against each other - a placement event was measured arriving 600ms late, enough to land after the cancellation of the same order. One rule for both, since over is final here and no id is reused: the report earliest in time stands, whichever arrived last. Asked in review whether the notes leak - they are retired by the next snapshot, and snapshot loads turned out to be strictly serialised, which is what makes that rule exact rather than approximate; the one path where nothing retired them (stream up, reload failing, observed the day before against a rate limit) is now capped. Five offline tests, each mutation-checked |

## Backlog — open, not scheduled

Carried out of step 5 deliberately rather than left implied. None of them blocks anything today.

| item | where it stands |
|---|---|
| `GET /fapi/v1/algoOrder?algoId=` answers `-2013` for an order that exists | measured 2026-09-19 against a real conditional order, reproduced, unexplained. Ingestion keys on `openAlgoOrders`, the endpoint proven to work, so nothing depends on the answer. Worth reopening only if a path needs a single conditional order by id |
| the nested user-data-stream payloads are `unretrievable` | ~20 short field names that render from a schema embed present in neither index file. Closed in practice by live capture - every event this module reads is pinned against a payload recorded off the wire - but the documentation axis stays `unretrievable`, and a future pass should not mistake that for unchecked |
| the filter converters have no test file | covered through the exchange-info fixture. A missing file, not a missing fact |
| spot's account-stream subscription spends weight nothing counts | 2 on connect and 2 per retry against a 6000/min ceiling, so it is a trickle rather than a risk. The limiter's model is to report what a response header said, and this transport has no header; counting it means reading the `rateLimits` array the reply carries. Worth doing before anything else runs on that socket |
| spot is a step behind | **narrowed 2026-09-21.** The read half is closed: the four user read paths exist, are pinned offline and ran green live, so `UserProviderTests` are no longer `Not implemented` and no longer skipped. What is left is the trading block, which covers futures only - and that is now blocked on a decision rather than queued as work, because spot's user stream is a different transport. See "Spot step 5 — where it stops" |

## Spot assessment — 2026-09-21

Two fresh verifiers, one per step, against the step's own done-checklist. Read-only; no live run. The
headline was confirmed by hand afterwards rather than taken on the verifiers' word, because everything
below rests on it.

**Spot is at step 4, not step 5.** The step table used to read as though the venue trailed only in
trading validation. It does not: four of its six read paths do not exist.

`Spot/Internal/User/UserProvider.cs:14-64` issues **no HTTP at all** — `LoadContextAsync`,
`LoadOpenOrdersAsync`, `LoadOrdersAsync` and `LoadTradesAsync` each return an empty success,
synchronously. `Spot/Internal/User/UserConnector.cs` throws `NotImplementedException` in five places.
The manifest records this as `[DEAD]` and is right; what was wrong was the step table, which is
corrected above.

The cost, stated plainly because it is easy to read this as merely missing work: a caller asking spot
for its account, open orders or trades is told, with an `Ok` status, that there are none. **A wrong API
key, an IP ban and a signing defect are all indistinguishable from an empty account, permanently**,
because no request is ever attempted to fail.

### What that makes of the converters

Spot's user-side converters, enumeration tables and query processor are well built and genuinely
pinned — and parse nothing, because nothing calls them. Two consequences worth keeping:

- "Pinned" is true of the suite and irrelevant to production for that whole domain. The verification
  axis says nothing about whether a fact is reachable, and here it should be read alongside `[DEAD]`.
- One landmine waits there. `executedQty != 0 ? executedSum / executedQty : 0m` appears in three spot
  converters and **only the non-zero arm is ever driven** — a brand-new unfilled order is exactly the
  zero case, and collapsing the guard would be a divide-by-zero on the commonest order state. Inert
  today; a first-day crash when the path is revived.

### Drift found in spot's live code — the market domain

This part is reachable today and the findings are real:

| drift | cost |
|---|---|
| ~~the ticker drop rule's price half was pinned by nothing~~ — **closed 2026-09-21** | two independent conditions joined in one line, and the only test satisfied both at once, so deleting the price half survived every test on both venues. It backs the live book-ticker stream: a quote with no price on either side would have reached subscribers. Now pinned separately on both venues, mutation-checked |
| ~~the candle request is pinned only by `startTime`~~ — **closed 2026-09-21** | all four parameters and the path are asserted now, each mutation-checked |
| ~~the candle failure path is unverified on spot~~ — **closed 2026-09-21** | a refusal yields a batch that is not `Ok`; a window answered with no candles yields no batch at all. Distinguishable, and not in the shape first assumed |
| ~~the two endpoints this step drives are never asserted as composed URLs~~ — **closed 2026-09-21** | both paths asserted as the server received them, both mutation-checked |
| three more drop rules have no negative test, and one OR is satisfied by a single fixture | low today, and exactly what a fixture rewrite breaks silently |

### The question this stops at

Reviving spot's user path is not a repair, it is a feature: four read paths, their offline tests, their
live read validation, and only then a connector. **Whether spot needs a user path at all is the
decision, and it is not ours.** Nothing else about spot is blocked on it - the market side works and is
being improved above.

**Answered in part on 2026-09-21**: the four read paths were built, pinned and validated live. The
connector half stopped at its contract — see "Spot step 5 — where it stops" below, which is the current
statement and supersedes this paragraph.

| 2026-09-21 | 3 — types and serialization (spot) | [`2026.09/2026.09.21-step-3-spot.md`](2026.09/2026.09.21-step-3-spot.md) | spot reconciled against step 3's checklist, the first run of `implement-provider-types` since it was written. **A documented status was unmapped and the lookup throws** on what it does not know - so a pending leg would have failed the parse of the whole order list it arrived in, rather than arriving wrong. **The same guarded quotient sat unexercised in three converters**: an executed price divided by a quantity, with every fixture having filled something, so only the dividing arm ever ran - and nothing filled is what a new order looks like. **Five drop rules nothing named**, two of them behind a test that looked like coverage because one fixture satisfied every arm of an `OR` at once. Nine facts moved: six killed a mutation, three cannot be mutated because removing them fails the build, the model taking non-nullable strings. Finance tests 519 → 529 |

| 2026-09-21 | 4 — provider read paths (spot, market half) | [`2026.09/2026.09.21-step-4-spot-market.md`](2026.09/2026.09.21-step-4-spot-market.md) | the three market-side items from the assessment closed, through `implement-provider-reads` on its first use. Five request facts pinned and mutation-checked; the refusal path pinned as a regression guard over an upstream fix. **Two things the run learnt rather than confirmed**: a refusal stands the shared rate limiter down, so the next load in the same test answers `TooManyRequests` without the server hearing about it - the cause was visible only because the assertion printed the status instead of a boolean; and a window the venue answers with no candles yields **no batch at all**, where the assumption was an empty batch. Both are now written into the tests that found them. Finance 529 → 532. **The user half of step 4 remains: it does not exist, and whether it should is the gate** |

| 2026-09-21 | 2 — collect facts (spot, step 5's contract) | *(no report yet; manifest §11 and the snapshot addition are the artefact)* | **step 5 for spot stopped before its first line of code, and the stop is the finding.** §10 covered the four read endpoints and nothing else, so the order lifecycle and the user stream had to be collected the same way. The order endpoints were straightforward. The stream was not: **spot's listen-key mechanism is deprecated and its documentation removed** — `listenKey` and `userDataStream` each have **0 occurrences** in the spot REST reference, while the changelog says the feature still works until a future retirement. Recorded `contested`, because those two readings disagree and neither is stale. The replacement is a **different transport** — a request/response WebSocket API at its own endpoint, subscribed with `userDataStream.subscribe.signature`, and delivering every event wrapped as `{"subscriptionId", "event"}` where futures delivers it bare. Three more divergences beside it: no general amend (spot's amend reduces quantity only), a second rate budget the limiter does not model, and a required symbol on cancel-all. **The page describing all of this was not in the snapshot** — it was never referenced, and became load-bearing without any referenced page changing. Fetched at the same pinned commit and added |
| 2026-09-21 | 4 — provider read paths (spot, user half) | *(no report; the work is in [library#42](https://github.com/annium/library/pull/42))* | **the four spot user read paths built, where there were none.** The contract carried no spot user endpoints at all - the dead path gave step 1 nothing to anchor and step 2 nothing to check - so they were collected from the stored snapshot into manifest §10 first, with citations, and three of them **diverge from futures**: a 24-hour history window against seven days, an unscoped open-order list at 80 against 40, and a trade endpoint returning the most recent rows without a cursor where the neighbour's page cap selects the oldest. Ten offline tests, five mutants killed including a refusal swallowed as an empty collection. **Live read run green**: 60 responses, all 200, the two history walks making 21 and 22 requests - the windowing running rather than described. **Census correction from the same run**: four of the six live read tests assert only `Ok` and not-null, so they would have passed against the stub and never distinguished a working path from an absent one. The offline suite is what pins this; the live run answers only whether the venue accepts what we send |
| 2026-09-21 | 5 — connector, offline (spot) | [`2026.09/2026.09.21-step-5-spot.md`](2026.09/2026.09.21-step-5-spot.md) | **the account stream had to be written, not ported.** This venue reaches its account events through a signed method call on a request/response socket; the key mechanism it used to share with the other venue is deprecated and gone from the reference. Built: a WebSocket API request builder, a second implementation of the stream, and a connector that was five `NotImplementedException`s. The listen-key configuration spot was filling and never resolving is gone, and the base config no longer asks every venue for a mechanism only one has. **Thirteen mutations, all killed - three only after something was fixed first.** Two mutants did not compile, caught by reading the build result before the test result. **Two tests were vacuous and passed anyway**: one asserting a wrong-id reply is rejected raced its own send, since sending returns when the bytes leave rather than when they are read; one asserting the ended-order notes are bounded did not discriminate a cap of a thousand from a cap of one. Both rewritten, both then killed their mutant. A defect in existing code found on the way: a time in force sent on order types the venue refuses it for, with three tests asserting that as correct. Finance 532 → 576. **No live stage ran and none is approved**: this venue has no trading block, so nothing in §11 is `live` |
| 2026-09-21 | 5 — connector, trading block built (spot) | [`2026.09/2026.09.21-step-5-spot-trading-block.md`](2026.09/2026.09.21-step-5-spot-trading-block.md) | **built, and deliberately not run.** The shared trading fixture could not be used: it waits for a position snapshot a cash account never sends, so it hangs rather than failing, and its cleanup closes what it finds - which on a cash account means selling the holdings, since there the balances *are* the position. Spot's own fixture cancels orders and nothing else. **The block trades nothing**: every order is a limit buy at half the market, resting until cancelled, so placement, stream reporting, balance locking, replacement and cancellation are all exercised while nothing is bought, sold or charged. A fill is left unasserted live and said so. Three stages written - a read-only connector test, four write cases, and a standalone cleanup tool in the probe block, written **before** the first trading run rather than after it. Verified without touching the exchange: the offline block is 576 with **zero skipped**, so the five new live tests are excluded by their traits rather than silently joining the routine run |
