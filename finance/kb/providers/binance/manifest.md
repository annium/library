---
title: Binance contract manifest
type: provider-manifest
status: living
created: 2026-09-01
checked_against: 2026-09-01
docs_revision_spot: a0057759f1cbcab812af44b75309d72866a57561
---

# Contract manifest — binance

> **Living. Changes when Binance changes.** Our own progress lives in `status.md`, deliberately apart:
> keeping them in one file would mean every reconcile run edits this document for two unrelated
> reasons, and `git log -p manifest.md` would stop answering "what did the exchange change".

Every entry is a fact owned by Binance, not by us, that this module depends on, anchored to
`file:line`. Paths are relative to the repository root; `Base`, `Spot` and `UsdFutures` abbreviate
`finance/Providers/src/Annium.Finance.Providers.Crypto.Binance.<name>/`.

**The line numbers were re-anchored on 2026-09-18** against the tree at
`provider/binance-contract-reconcile`. Of 107 line-anchored facts, 76 still resolved and 31 had moved —
they cluster exactly where steps 3 to 5 worked: `Endpoints.cs` on both venues, the futures
`UserProvider` and `UserConnector`, and the rate-limit extensions. No cited file had disappeared.

That is ordinary wear, and it is why this paragraph exists rather than a claim that the anchors are
permanently good: an anchor resolving to a real file at the wrong line reads as checked, which is worse
than one that visibly does not resolve. Before trusting a `:line` below, confirm the sweep has been run
since the last change to the tree.

**`checked_against: 2026-09-01
docs_revision_spot: a0057759f1cbcab812af44b75309d72866a57561`** — the documentation axis was closed on that
date against the snapshots stored beside
[`2026.09/2026.09.01-contract.md`](2026.09/2026.09.01-contract.md), with two accepted gaps recorded
against their own entries. This line used to say the manifest had never been compared against Binance's
documentation — which its own front matter and that report both contradict. It was written during the
inventory pass and not updated when the comparison landed the same day.

The **verification** axis moves independently of that date, and did most recently on 2026-09-16, when
the read block ran with credentials for the first time.

## Where the documentation comes from

Binance publishes spot and USDⓈ-M futures separately, and they must be fetched separately: a rename on
one venue and not the other produces a failure that looks venue-specific and therefore looks like ours.

| venue | source | tier | how |
|---|---|---|---|
| spot | `github.com/binance/binance-spot-api-docs` | 1 — upstream git | `curl -sSL https://raw.githubusercontent.com/binance/binance-spot-api-docs/master/<path>`; pin the commit SHA |
| usd-futures | `developers.binance.com` | 2 — site, markdown | `curl -sSL "https://developers.binance.com/en/docs/products/derivatives-trading-usds-futures/<page>.md"` |

**Appending `.md` to a developers.binance.com page returns its markdown source.** Fetching the page
itself gets an empty `202` — it is a protected single-page app — so the `.md` suffix is not a
convenience, it is the only way to retrieve that documentation faithfully.

### Page paths that work

Discovering these cost most of the first run. Spot, under
`raw.githubusercontent.com/binance/binance-spot-api-docs/<sha>/`: `CHANGELOG.md`, `enums.md`,
`errors.md`, `filters.md`, `rest-api.md`, `user-data-stream.md`, `web-socket-streams.md`.

Futures, under `developers.binance.com/en/docs/products/derivatives-trading-usds-futures/`, with `.md`
appended: `change-log`, `general-info`, `error-code`, `user-data-streams`, and
`websocket-market-streams/Important-WebSocket-Change-Notice`.

**Not found, still a gap.** The per-endpoint futures reference pages — exchange information, klines,
new / modify / cancel order, account, trade list. Every path tried returned the site's HTML shell.
Until they are located, futures request and response schemas are verified only against `general-info`
and the change log, never against their own pages. Tried and rejected:
`market-data-endpoints/…`, `trade-endpoints/…`, `account-endpoints/…`,
`user-data-streams-endpoints/…`, and `catalog/core-trading-derivatives-trading-usd-s-m-futures/api/rest-api/…`.

Known quirks, both learned the hard way:

- The derivatives change log **is truncated** when read through a summarising fetch — it returned only
  two months. Retrieve the `.md` and read it whole.
- **An unknown path returns the HTML shell with a `200`**, body exactly 65475 bytes. Five endpoint
  paths returned byte-identical responses before this was noticed. Reject anything beginning
  `<!doctype html>`, and compare sizes across a batch.
- **The change log is not the documentation.** The WebSocket migration notice — the largest finding of
  the first run — is not a change-log entry; it sits on its own page, reachable only through a link
  inside the change log. Follow the links out.
- A search result claimed a 2026-04-23 WebSocket decommissioning and the change log did not contain
  it. Held as **unresolved** rather than reported, and the separate notice settled it: the search was
  right and the change log incomplete. Two sources disagreeing stay unresolved until a third decides.

Snapshots of what was fetched live beside each run's report, so a report can be checked against the
text it was written from rather than against a page that has since moved.

## How to read an entry

Every fact carries **two independent states**. They answer different questions and are moved by
different steps, and a fact can be strong on one and worthless on the other.

**Documentation** — what Binance says: `confirmed` · `unchecked` · `undocumented` · `contested` ·
`unretrievable`.

**Verification** — what of ours would notice if it stopped being true: `pinned` (an offline test
fails) · `live` (seen in a real response, dated) · `gated` (covered only by the exchange suite, which
is skipped by default, so it proves nothing on an ordinary run) · `none` · `vacuous` (a test names it
and cannot fail — worse than `none`, because the effort looks spent).

Neither is a boolean; both take a note. Only exceptions are written out — where a category shares a
state it is stated once at its head.

### Where this manifest stands, as of 2026-09-01

Documentation is closed. Verification was censused against the test suites entry by entry, and the
result corrected two claims an earlier category-level reading of this table had got wrong — noted
below, because a summary that reads as an annotation is the failure this model exists to prevent.

| category | documentation | verification |
|---|---|---|
| 1 endpoints | `confirmed`, and this time **path by path** rather than as a set — the earlier pass checked base URLs and path shapes, which is how a wrong `v1` survived it | mixed, and the summary here was too kind until 2026-09-18. `EndpointsTests` asserts the *composed* URL on **USD-M only**; the spot file asserts the bare base `wss://stream.binance.com/`, so the composed spot market-stream route (`/stream`) and the spot user-stream path (`/ws/`) are asserted **nowhere** — `none`. Four futures REST paths became `pinned` offline during step 5 (`POST /fapi/v1/leverage`, `POST /fapi/v1/order`, `DELETE /fapi/v1/order`, `GET /fapi/v1/openOrders`); the rest stay `gated`. Every read path on USD-M is **`live` (2026-09-16)**. Spot has no account read paths at all — see §9 |
| 2 request parameters | `confirmed` at tier 1 from the official Postman collections | futures order shapes are **`pinned`** offline, per order type, both init and modify, with `reduceOnly` branching asserted both ways. The signing scaffolding is **`live` (2026-09-16)** as a whole on USD-M: six signed account reads were accepted, which is the only thing that shows key, timestamp, query and signature compose into something Binance honours. `recvWindow` is **`pinned`**: 30000, asserted on the sent query, where the unit is the whole risk — Binance reads it as milliseconds and caps it at 60000, so a value meant as seconds is a 30 ms window and every request is stale |
| 3 response fields | spot `confirmed` at **tier 1** from `rest-api.md`; futures account / query-order / trade `confirmed` at **tier 3** (a reading, not the page); the user-data-stream nested payloads are `unretrievable` | `pinned` per converter, every one having its own test with real fixtures — which says a converter is covered, not that every branch inside it is, and on 2026-09-17 that gap held two: the futures `createdAt` synthesis (now `pinned` both ways) and the asset drop rule, where the test found a defect rather than a gap (see §3 exchange info). Negative branches are **`pinned`**, and three of the four already were when this said otherwise — `LoadContext_DropsASymbolThatIsNotTrading`, `…WithSpotTradingDisallowed` and `…WithoutSpotPermission` have covered the status, the trading flag and the permission for a while. The fourth is pinned as of 2026-09-16: a `clientOrderId` that is not a GUID drops the whole cancel response, which is what happens to every order Binance named itself (`web_…`, `autoclose-…`) |
| 4 filters | `confirmed` — every type name and field on both venues, including that spot documents **both** `MIN_NOTIONAL` and `NOTIONAL` while futures documents only `MIN_NOTIONAL` | `pinned`, including the lot-size merge arithmetic — but entirely piggybacked on the exchange-info fixture; there is no filter test of its own |
| 5 enumerations | `confirmed` on both venues against their documented lists | **`pinned` as a category since 2026-09-16.** `WireMappingTests` on each venue drives every table both ways: each documented wire value parses to its member, each domain member writes back out, and the round trip holds. Until then coverage was incidental — a value counted only if some fixture happened to carry it, which on the read side left every terminal order status (`FILLED`, `REJECTED`, `EXPIRED`, `EXPIRED_IN_MATCH`, and spot's `PENDING_CANCEL`) parsed by nothing at all |
| 6 error and status codes | `confirmed`, except the **two** maps now written out in §6 — the market one is narrower and that was `unchecked` until 2026-09-18 | there are not three copies and have not been for some time: one market map and one user map, both in `Base`. `HttpRequestUserResultExtensionsTests:149-150` pins `-2018` and `-2019`, so the old note about a drifted untested `Base` copy is stale in both halves |
| 7 rate limiting | `confirmed` for the header; the decay arithmetic is `unchecked`; the `Retry-After` and ban-message rules are `undocumented` | the mechanism is `pinned` — header casing, missing and malformed values, water mark, post-dispose — and so is the **runtime `REQUEST_WEIGHT` overwrite**. The production ceiling and the water-mark fraction are **`pinned` per venue** as of 2026-09-16, asserted through the number they compose to (1920 on USD-M, 4800 on spot). **Decay, corrected 2026-09-18:** this table said `none` while §7 said `pinned`; neither was right. The step is pinned on USD-M in one direction only, the interval is not pinned at all, and spot has no decay test — see the row in §7 |
| 8 auth and signing | `confirmed` | **`live` (2026-09-16)** on USD-M — signed reads accepted by the exchange. The percent-encoding rule is **no longer `vacuous`: it is `pinned`**, and so are what is signed and the exclusion of `signature` itself — `HttpRequestSignatureExtensionsTests` sends real requests to a local server and asserts that the string handed to the signer equals the one that arrived, minus the signature appended after. The use of synced rather than local time is **`pinned`**: the timestamp on the sent query is the one the server-time source reports, not the machine clock — a distinction that costs nothing while the two agree and produces intermittent `-1021` on somebody else's machine when they drift |
| 9 timing and lifecycle | `confirmed` for the documented windows; the reload cadences added 2026-09-18 are ours and `undocumented` | candle interval and page size **`live` (2026-09-16)**. Order history windowing is **`pinned`**, and so is **trade history** — corrected 2026-09-18: `UserProviderReadPathTests:142` drives twenty days through three windows and asserts the boundaries, exactly as the order test does; this table said `none` until driven. Sync and reload cadences `none` |
| 10 hard-coded facts | mixed — `confirmed` where they mirror a documented limit, `undocumented` where they are heuristics | mostly `none`. `"BTCUSDT"` liveness and the kline page size are **`live` (2026-09-16)**. The futures asset-precision heuristic is **half pinned** — corrected 2026-09-18: only its `8` branch is reached, and the branch that makes it a heuristic at all is reached by nothing. For an `undocumented` fact that is the gap that matters, since a test is the only possible watcher |

**One route decision worth keeping visible.** The futures route lives in the *path*, never in the base.
Both call sites compose with `new Uri(base, path)`, which discards the base's path whenever the path
starts with a slash — so a route moved into the base is silently dropped and the legacy URL comes back
from configuration that reads as correct. A mutation doing exactly that is killed by
`MarketStream_ConnectsToThePublicRoute`.

**The two `vacuous` entries known on 2026-09-01 are closed** — and two more were found on 2026-09-18,
which is the honest shape of this category: they are found by looking, not by waiting. The new pair is
recorded in §3 (the account row) and just below. Neither of the closures was achieved by changing the
input the weak test uses. In both cases that test could not have been made to see the property at all,
and a second test at the right level was what settled it.

1. ~~The signing golden value~~ (§8) — **closed 2026-09-16, and the framing was wrong twice over.** The
   worry was that the fixture's query holds no character needing percent-encoding, so the test passes
   whether or not the implementation encodes. But that test hands the signer a *literal*: no query is
   composed and nothing is encoded, so no input could have made it see the rule. And there is no defect
   to see — `Signature` signs `req.Uri.Query`, the composed query, which `UriQuery.ToString()` builds
   with `Uri.EscapeDataString`. Measured across a space, `+`, `&`, `=`, a JSON payload and Cyrillic: the
   signed bytes are the sent bytes. The rule is now `pinned` by an offline test that sends real requests
   to a local server and compares what the signer was asked for against what arrived; signing the
   unescaped query kills 7 of its 8 cases. What the golden value pins is narrower and still worth
   having — that our HMAC of a fixed input matches Binance's.
2. ~~The history paging windows~~ — **closed**, and the clause about the gated fixture is stale as of
   2026-09-18: `Tests.Lib/User/UserProviderTestBase.cs:170-180` asks for twenty days now and says why.
   Offline tests drive both order and trade history through three windows each and assert the
   boundaries.

**Found 2026-09-18, and queued for step 5 rather than fixed here** — both are defects in tests, not in
the contract, and this layer specifies rather than repairs:

1. `Tests.Lib/User/UserConnectorReadTestBase.cs:82` — `positions.Count.IsGreaterOrEqual(0)`. A count is
   never negative, so the line cannot fail in any run, and it is the only thing covering positions in
   the read suite. See the account row in §3.
2. `UsdFutures.Tests/.../WireMappingTests.cs:157` (`SmallTables_MapEveryMemberBothWays`) and its spot
   twin — **tautological**. `OrderSides`, `OrientationRanges` and `MarginTypes` all build `StringToValue`
   as `ValueToString.ToDictionary(x => x.Value, x => x.Key)`, so the round trip holds by construction
   whatever the strings are: renaming `"BUY"` to `"buy"` passes this test in full. §5 stays `pinned`
   because other tests assert the literals — `"BUY"`/`"BOTH"` in `QueryProcessorTests:75-76`,
   `"LONG"`/`"SHORT"`/`"isolated"`/`"cross"` in `BalanceAndPositionUpdateEventConverterTests:118-145` —
   but not by the test whose name promises it, and a reader who trusts the name stops there.

**Two components have no test file at all:** `HttpRequestLogExtensions` and the filter converters — the
latter covered through the exchange-info fixture, so a missing file rather than a missing fact.
`HttpRequestSignatureExtensions` left this list on 2026-09-16; `WebSocketService` and `ListenKeyResolver`
left it on 2026-09-18, when step 5 gave them `BookTickerServiceTests` (9 tests, driving subscribe,
delta-subscribe, unsubscribe, resubscribe-on-reconnect, dispose and failed-send) and
`ListenKeyResolverTests` (6).

**~~[DEFECT, upstream] An exchange error is discarded whenever the success type is a collection.~~ —
closed.** `Annium.Net.Http`'s `AsResponseExtensions` parsed the success type first; when that threw — as
it does for any error body against a collection type — control left for the catch and the branch that
would have read Binance's `{code, msg}` never ran, so `-2015 Invalid API-key` reached the caller as
`ParseError`. Every read endpoint returning a list was affected: open orders, all orders, user trades,
klines. Fixed in `Annium.Net.Http` 1.1.49 (annium/base#68) — the failure shape is now tried after the
success shape throws — and taken up here with the 1.1.49 package bump. `UserProviderReadPathTests` pins
both routes: the collection one, which broke, and the object one, which never did because an object
success type parses an error body into a defaulted instance rather than throwing.

A residue worth naming: the reason now arrives, but the status is coarse. `MapOperationCode` sends every
negative Binance code to `BadRequest` bar the three in §6, so an invalid API key reads the same as a
malformed parameter, and the HTTP 401 that would have mapped to `Forbidden` never gets a say. Precisely:
the code wins over the status **whenever the body parsed as a Binance error** — a synthetic result
yields to the status instead (§6). This paragraph read as though the code always won. Queued, not
closed, and deliberately left alone until the documentation pass supplies the real code list, because
the useful split is by what a caller would do differently and inventing that taxonomy blind would ship
it to every caller, `crypted`'s order path included.

**One lesson from the first live run, kept where the next reader meets it.** A `[DIVERGES]` marker is a
note that something looks odd. It is not a check, and after a while it reads like one. Spot's server
time path carried that marker for the whole contract pass and was simply wrong; the run that found it
was the first thing to actually call the endpoint. Markers record; only a comparison confirms.

**Where the exposure is — re-censused 2026-09-18.** Roughly: `pinned` ~79, `gated` ~15, `none` ~17,
`vacuous` 2. The `none` bucket is no longer the constants: ceilings, water mark and `recvWindow` are all
pinned now. Its centre of mass has moved to **the listen-key lifecycle** — the path, the HTTP method,
the keep-alive cadence and the limiter accounting are four facts, none defended offline, all of them on
the connection path every user stream depends on — and to **the spot websocket routes**, which this
table reported as pinned until today and which nothing composes and checks.

That migration is worth naming as a pattern rather than as a list. The constants got pinned because
they were written down here and read back; the listen-key facts stayed `none` because the tests that
exercise the resolver use *synthetic* values — a `/listenKey` stub endpoint, 50 ms intervals, a rate
limiter that records nothing — so the component is covered while every exchange fact inside it is not.
A test can be thorough about behaviour and blind to the contract in the same breath.

Structural markers stay separate from both axes: **[DIVERGES]** between spot and futures,
**[DUPLICATED]** across files, **[DEAD]** for code unreachable in production, **[DRIFT]** where what we
have no longer matches what Binance documents.

## 1. Endpoints

### Base URLs

| Fact | Where |
|---|---|
| Spot HTTP `https://api.binance.com` | `Spot/Internal/Shared/Endpoints.cs:11` |
| Spot WS `wss://stream.binance.com` | `Spot/Internal/Shared/Endpoints.cs:14` |
| Futures HTTP `https://fapi.binance.com` | `UsdFutures/Internal/Shared/Endpoints.cs:27` |
| Futures WS `wss://fstream.binance.com`, unrouted, with the route in the path: `/public/stream` for market, `/private/ws/` for user data. `/market` carries the regular feeds and this provider subscribes to none of them | `UsdFutures/Internal/Shared/Endpoints.cs:30,33,36` |

Sandbox base URLs are gone: all testing is against the live exchange, so the environment concept was
removed from the code entirely rather than kept and corrected. The futures sandbox had moved to
`demo-fapi` / `demo-fstream` and the spot sandbox websocket host had never been right — both are
recorded here only so a future reader knows the omission is deliberate.

### REST paths

| Method and path | Venue | Where |
|---|---|---|
| `GET api/v3/exchangeInfo` | spot | `Spot/Internal/Market/MarketProvider.cs:48` |
| `GET api/v3/klines` | spot | `Spot/Internal/Market/MarketProvider.cs:91` |
| `GET /api/v3/time` — **[DRIFT, fixed 2026-09-02]** we called `/api/v1/time` and the first live read run failed on it. The manifest had recorded the version oddity as a divergence and never checked it against the documented endpoint list; a curiosity stood in for a verified fact | spot | `Spot/Internal/Shared/Endpoints.cs:30` |
| `GET fapi/v1/exchangeInfo` | futures | `UsdFutures/Internal/Market/MarketProvider.cs:51` |
| `GET fapi/v1/klines` | futures | `UsdFutures/Internal/Market/MarketProvider.cs:100` |
| `GET /fapi/v1/time` **[DIVERGES]** — `v1` here is correct, unlike spot | futures | `UsdFutures/Internal/Shared/Endpoints.cs:42` |
| `GET /fapi/v2/account` — note `v2` | futures | `UsdFutures/Internal/User/UserProvider.cs:73` |
| `GET /fapi/v1/openOrders` | futures | `UsdFutures/Internal/User/UserProvider.cs:108` |
| `GET /fapi/v1/allOrders` | futures | `UsdFutures/Internal/User/UserProvider.cs:168,212,250` |
| `GET /fapi/v1/userTrades` | futures | `UsdFutures/Internal/User/UserProvider.cs:294,337,375` |
| `POST /fapi/v1/leverage` | futures | `UsdFutures/Internal/User/UserConnector.cs:191` |
| `POST /fapi/v1/order` | futures | `UsdFutures/Internal/User/UserConnector.cs:259` |
| `PUT /fapi/v1/order` (modify) | futures | `UsdFutures/Internal/User/UserConnector.cs:315` |
| `DELETE /fapi/v1/order` | futures | `UsdFutures/Internal/User/UserConnector.cs:350` |
| `DELETE /fapi/v1/allOpenOrders` | futures | `UsdFutures/Internal/User/UserConnector.cs:385` |
| `POST /fapi/v1/listenKey` — the path literal moved out of the factory into `Endpoints` on 2026-09-18 (step 5d), where the other futures paths already lived | futures | `UsdFutures/Internal/Shared/Endpoints.cs:39`, issued at `UsdFutures/Internal/User/UserConnectorFactory.cs:59` |
| Spot cancel-replace endpoint **[DEAD]** — parameters built, path never issued | spot | `Spot/Internal/User/Services/QueryProcessor.cs:68-111` |

### WebSocket

| Fact | Where |
|---|---|
| Combined stream path — `/stream` on spot, `/public/stream` on futures **[DIVERGES]** | literals in `Spot/Internal/Shared/Endpoints.cs:17`, `UsdFutures/.../Endpoints.cs:33`; consumed at `Spot/Internal/Market/Profiles/MarketConfigProfile.cs:29`, `UsdFutures/.../MarketConfigProfile.cs:38`. Spot's two moved into `Endpoints` on 2026-09-18 (step 5d); before that they sat inline in the profiles |
| Book ticker topic `{symbol}@bookTicker`, symbol lowercased | `Base/Internal/Market/Services/BookTickerService.cs:79` |
| User stream URI is `{WsApi}{ListenKeyUriPath}{listenKey}` — `/ws/` on spot, `/private/ws/` on futures **[DIVERGES]** | `Base/Internal/User/Services/UserStream.cs:112`; literals in `Spot/Internal/Shared/Endpoints.cs:20`, `UsdFutures/.../Endpoints.cs:36`; path from `Spot/.../UserConfigProfile.cs:37`, `UsdFutures/.../UserConfigProfile.cs:47` |

---

## 2. Request parameters

### Signed-request scaffolding — `Base/Shared/HttpExtensions/HttpRequestSignatureExtensions.cs`

| Fact | Line |
|---|---|
| API key header is `x-mbx-apikey` | 19-20 |
| `recvWindow` sent on every signed request, hard-coded `30_000` | 25 |
| `timestamp` and `signature` appended as query params | 27-44 |

### Klines

`symbol`, `interval` (always `"1m"`), `limit`, `startTime` — `Spot/Internal/Market/MarketProvider.cs:92-95`,
`UsdFutures/Internal/Market/MarketProvider.cs:101-104`.

### Futures orders — `UsdFutures/Internal/User/Services/QueryProcessor.cs`

| Fact | Line |
|---|---|
| Always sent: `newClientOrderId`, `symbol`, `side`, `positionSide`, `type`, `newOrderRespType="RESULT"` | 32-37 |
| Limit adds `timeInForce="GTC"`, `quantity`, `price` | 42-44 |
| Market adds `quantity` | 48 |
| Stop/take-profit market add `quantity`, `stopPrice` | 52-59 |
| Stop/take-profit limit add `timeInForce="GTC"`, `quantity`, `price`, `stopPrice` | 62-74 |
| `reduceOnly="true"` sent **only** in one-way mode — Binance rejects it alongside an explicit `positionSide` in hedge mode | 79-83 |
| Modify supports `Limit` only; sends `origClientOrderId`, `symbol`, `side`, `quantity`, `price`. Binance also accepts optional `priceMatch` and `modifyId`, which we do not send | 96-107 |
| Cancel sends `orderId` and/or `origClientOrderId` + `newClientOrderId`, `symbol` | 119-137 |
| Cancel-all sends `symbol` and nothing else | 145-152 |
| `leverage` floored to int32 | `UsdFutures/Internal/User/UserConnector.cs:187` |

### History paging cursors — `UsdFutures/Internal/User/UserProvider.cs`

Recorded 2026-09-18; the manifest had the 7-day window (§9) but never the parameter that advances it.

| Fact | Line |
|---|---|
| `orderId` is the cursor on `allOrders` once a window returns a full page — the next request asks for ids above the last one seen, not for the next time slice | 253 |
| `fromId` is the same cursor on `userTrades` | 378 |

Both rest on the ordering assumption recorded in §3, and neither is documented as a guarantee: the
parameter is documented, its use as a cursor is our inference from the order the exchange happens to
return.

### Spot orders **[DEAD]** — `Spot/Internal/User/Services/QueryProcessor.cs`

Same base set minus `positionSide`/`reduceOnly` (24-28); modify via cancel-replace with
`cancelReplaceMode="STOP_ON_FAILURE"`, `cancelOrigClientOrderId`, `newClientOrderId`,
`timeInForce="GTC"` (72-79). Never invoked — see §9.

Its **response** shape is encoded too: top-level `code` / `msg` / `data`, with `data.cancelResponse`
and `data.newOrderResponse` nested inside, and the cancel leg's error preferred over the init leg's
when only one failed — `Spot/.../ModifyOrderFailureResponseConverter.cs:60-129`,
`ModifyOrderSuccessResponseConverter.cs:49-50`. Also `[DEAD]`.

---

## 3. Response fields

### Exchange info

- Rate limits: entry with `rateLimitType`/`limit`; only `"REQUEST_WEIGHT"` is read, and its window is
  **assumed to already be one minute** — `Base/Market/Contracts/Converters/RateLimitsConverter.cs:37-44`, field names read at `:68,71`
  **[UNVERIFIED]**. A payload carrying limits but no `REQUEST_WEIGHT` entry **drops the whole exchange
  info** rather than yielding one with no ceiling — `pinned` 2026-09-17 by
  `ExchangeInfoWithoutRequestWeightLimit_IsDropped`, because the alternative failure is a ban arriving
  later from code that reads as though it were respecting a limit
- Spot instrument: `symbol`, `status` (must be `"TRADING"`), `baseAsset`, `baseAssetPrecision`,
  `quoteAsset`, `quoteAssetPrecision`, `isSpotTradingAllowed`, `filters`, `permissions[]` /
  `permissionSets[][]` must contain `"SPOT"` — `Spot/Internal/Market/Contracts/Converters/InstrumentConverter.cs:50-121`
- Futures instrument: `symbol`, `contractType` (must be `"PERPETUAL"`; delivery contracts dropped),
  `status`, `baseAsset`, `baseAssetPrecision`, `quoteAsset`, **`quotePrecision`** — **[DIVERGES]**, spot
  spells the same idea `quoteAssetPrecision` — `UsdFutures/.../InstrumentConverter.cs:49-107`
- Futures assets: `assets[]` with `asset`, `marginAvailable`; an asset that is **not marginable is dropped**
  — `UsdFutures/.../AssetConverter.cs:28-62`, and the drop is only real because the envelope filters the
  nulls it returns (`ExchangeInfoConverter.cs:61-70`). `pinned` 2026-09-17 by
  `AssetThatIsNotMarginable_IsDropped`. Until that date the envelope read the array as a collection of
  **non-nullable** elements and kept the null, so the rule was written and discarded one line later; the
  first thing `MarketProvider.LoadContextAsync` does with the result is read `.Code` off every entry

### Exchange information envelope

| Fact | Where |
|---|---|
| Top level carries `rateLimits` and `symbols` (spot) | `Spot/.../ExchangeInfoConverter.cs:53,56` |
| Top level carries `rateLimits`, `assets` and `symbols` (futures) **[DIVERGES]** | `UsdFutures/.../ExchangeInfoConverter.cs:58,61,71` |

### Market data

| Fact | Where |
|---|---|
| Book ticker `s`, `b`, `a` — fields at `:53-67`. **Two independent drop rules**, both silent: a record with both prices zero (`:39-42`), and a record whose `s` is empty (`:39`). Only the first was recorded before 2026-09-18 | `Base/Market/Contracts/Converters/InstrumentTickerConverter.cs:39-42,53-67` |
| Kline is a **positional array**: 0 open time, 1 open, 2 high, 3 low, 4 close, 5 volume; 6+ ignored | `Base/Market/Contracts/Converters/CandleConverter.cs:48-71` |
| Server time `{"serverTime": long}`; a `serverTime` of `0`, or absent, **drops the whole response** (`:31`) — the sync then keeps its fast retry cadence rather than adopting a clock at the epoch | `Base/Shared/Contracts/Converters/ServerTimeConverter.cs:31,42-43` |
| Error envelope `{"code": long, "msg": string}` | `Base/Shared/Contracts/Converters/OperationResultConverter.cs:43-47` |
| WS control ack `{"id": long, "result": …}` — **[DEAD]**, found 2026-09-18. The converter is registered on both venues (`Spot/MarketContracts:27`, `UsdFutures/…:31`) and `CommandResult` is deserialized nowhere: `BookTickerService.cs:63` reads only `StreamData<InstrumentTicker?>`, and an acknowledgement falls into the bypass branch at `:67-69`. **We send `SUBSCRIBE` and never read the answer** — a subscription the exchange refuses is indistinguishable from one it accepted and that is simply quiet | `Base/Shared/Contracts/Converters/CommandResultConverter.cs:44-48` |
| WS control **request** `{"id": auto-increment, "method": "SUBSCRIBE"\|"UNSUBSCRIBE", "params": [topics]}` | `Base/Internal/Market/Services/WebSocketService.cs:125,220` (`"SUBSCRIBE"`, the second on resubscribe), `:149` (`"UNSUBSCRIBE"`), `:262-278` (the record) |
| Combined-stream envelope `{"stream": string, "data": {…}}` | `Base/Shared/Contracts/Converters/StreamDataConverter.cs:46-54` — matched as UTF-8 via `ValueTextEquals` since 2026-09-18 |
| Listen key `{"listenKey": string}` | `Base/User/Contracts/Converters/ListenKeyResponseConverter.cs:37-39` |

### Account

- Spot: `balances[]` with `asset`, `free`, `locked` — `Spot/.../GetAccountResponseConverter.cs:51-56`,
  `GetAccountResponseBalanceConverter.cs:45-57`
- Futures `/fapi/v2/account`: `assets[]` with `asset`, `marginBalance`, `maxWithdrawAmount`,
  `initialMargin`, `maintMargin`, `updateTime`; `positions[]` with `symbol`, `positionSide`, `isolated`,
  `leverage`, `positionAmt`, `entryPrice`, `unrealizedProfit`, `updateTime` —
  `UsdFutures/.../GetAccountResponseBalanceConverter.cs:65-82`, `GetAccountResponsePositionConverter.cs:71-94`
- **How those asset fields are interpreted** is itself a contract, recorded 2026-09-18: free balance is
  `maxWithdrawAmount` and locked balance is `initialMargin + maintMargin` —
  `UsdFutures/Internal/User/UserProvider.cs:90`. The fields were listed above from the first pass; the
  arithmetic over them was not, and it is the part that decides what a caller sees as available
- **[UNVERIFIED]** A one-way account is assumed to report one `positions[]` row per symbol regardless of
  whether a position is open, always with `positionSide=BOTH`. The test fixture's position-mode
  precondition depends on this — `finance/Providers/tests/Annium.Finance.Providers.Tests.Lib/User/UserConnectorTestBase.cs:614-627`
  (`EnsureOneWayPositionMode`), which is in the **Write** block, so the assumption is `gated`. The read
  suite looks as though it also covers it and does not: `UserConnectorReadTestBase.cs:82` asserts
  `positions.Count.IsGreaterOrEqual(0)`, which a count can never fail — `vacuous`, found 2026-09-18,
  queued for step 5

### Orders and trades

| Fact | Where |
|---|---|
| Spot order: `orderId`, `clientOrderId`, `symbol`, `type`, `side`, `origQty`, `price`, `stopPrice`, `status`, `executedQty`, `cummulativeQuoteQty` (executed price **derived** as sum ÷ qty), `time`, `updateTime` | `Spot/.../GetOrderResponseConverter.cs:79-121` |
| Futures order: same core plus `positionSide`, `reduceOnly`, and `avgPrice` used **directly** — **[DIVERGES]** | `UsdFutures/.../GetOrderResponseConverter.cs:87-135` |
| Spot init-order uses `workingTime` for created and `transactTime` for updated — **[DIVERGES]** from its own get-order, which uses `time`/`updateTime` | `Spot/.../InitOrderResponseConverter.cs:115-120` |
| Futures init-order has **no creation timestamp**; `updateTime` serves as both | `UsdFutures/.../InitOrderResponseConverter.cs:126-128` |
| **[CONTESTED]** the same converter reads `avgPrice`; the catalog lists that field on the query-order response and **not** on the new-order response. If the listing is right, a placed order returns an executed price of zero. The reading is tier 3 and cannot settle it — the first live order will | `UsdFutures/.../InitOrderResponseConverter.cs:123` |
| Trade: `id`, `orderId`, `symbol`, `qty`, `price`, `commission`, `commissionAsset`, `time` | both `GetTradeResponseConverter.cs` |
| Maker flag is `isMaker` on spot, `maker` on futures — **[DIVERGES]** | `Spot/.../GetTradeResponseConverter.cs:91`, `UsdFutures/.../GetTradeResponseConverter.cs:97` |
| Cancel response `clientOrderId` is parsed **as a GUID**; a non-GUID id makes the whole response read as missing | `Spot/.../CancelOrderResponseConverter.cs:50-55`, `UsdFutures/.../CancelOrderResponseConverter.cs:54-59` |
| Leverage response `{"leverage": decimal-string}` | `UsdFutures/.../LeverageResponseConverter.cs:41-43` |
| The leverage response is assumed to echo the **applied** leverage, so a value differing from the one asked for means "accepted but not applied" and is refused by the connector itself | `UsdFutures/Internal/User/UserConnector.cs:217` |
| `DELETE /fapi/v1/allOpenOrders` answers with the `{code,msg}` envelope, not with a list of what it cancelled | `UsdFutures/Internal/User/UserConnector.cs:391` |
| **History comes back sorted**, and the provider depends on it: it takes `.Last()` of a full page as the next cursor. Stated nowhere but a source comment — *"this assumes, that orders are sorted!"*. If the exchange ever returns a page unsorted, paging does not fail, it **silently skips** whatever lies past the element that happened to land last | `UsdFutures/Internal/User/UserProvider.cs:236,361` |

### User data stream events

| Event | Fields | Where |
|---|---|---|
| Spot `executionReport` **[DEAD]** | `e`,`s`,`t`,`i`,`c`,`o`,`S`,`q`,`p`,`P`,`X`,`z`,`Z`,`l`,`L`,`n`,`N`,`m`,`O`,`T` | `Spot/.../OrderUpdateEventConverter.cs:99-163` |
| Spot `outboundAccountPosition` **[DEAD]** | `e`,`u`,`B[]` with `a`,`f`,`l` | `Spot/.../AccountUpdateEventConverter.cs:63-84` |
| Futures `ORDER_TRADE_UPDATE` | top-level `e`, nested `o` with `s`,`t`,`i`,`c`,`o`,`S`,`q`,`p`,`sp`,`R`,`X`,`z`,`ap`,`l`,`L`,`n`,`N`,`m`,`T`. Trigger price is `sp` where spot uses `P`; average price is `ap` where spot derives it. `createdAt` synthesized from `transactionTime` only when status is `New`, else `0` (spot needs no synthesis: its event carries `O`) — both halves `pinned` 2026-09-17 | `UsdFutures/.../OrderUpdateEventConverter.cs:83,104-185` |
| Futures `ACCOUNT_CONFIG_UPDATE` **[DEAD]** | `e`,`T`,`ai` (presence ⇒ multi-assets change), `ac` (presence ⇒ leverage change), `j`,`s`,`l` | `UsdFutures/.../AccountConfigUpdateEventConverter.cs:73-98` |
| Futures `ACCOUNT_UPDATE` **[DEAD]** | `e`,`T`,`a`, `B[]` with `a`,`wb`,`cw`,`bc`, `P[]` with `s`,`ps`,`mt`,`iw`,`pa`,`ep`,`up` | `UsdFutures/.../BalanceAndPositionUpdateEventConverter.cs:67-89` |

**Two of those three futures events are `[DEAD]`, found 2026-09-18.** Both serializers are registered
(`UsdFutures/ProviderRegistrationContextExtensions.cs:83,84`) and **neither is ever resolved**:
`UsdFutures/Internal/User/UserConnector.cs:489` deserializes `OrderUpdateEvent` and nothing else. For
`ACCOUNT_UPDATE` this is deliberate and says so in the code — the connector reloads the whole context
instead, on the grounds that "account info in event is almost useless" (`:485-486`). For
`ACCOUNT_CONFIG_UPDATE` no such reason is recorded; a leverage or margin-mode change made elsewhere
reaches us only on the next context reload.

Both have converter tests, which is exactly why they read as covered. This is the shape worth watching
for on every venue: a registered serializer with a green test and no consumer.

**The event tag is matched literally, and a non-match drops the message in silence** —
`Spot/.../AccountUpdateEventConverter.cs:65`, `Spot/.../OrderUpdateEventConverter.cs:101`,
`UsdFutures/.../OrderUpdateEventConverter.cs:106`. The names were recorded above from the first pass;
the drop rule was not. If Binance renames an event, nothing raises and nothing logs — the stream simply
goes quiet, which is the failure mode hardest to tell from an idle account.

---

## 4. Exchange filters

| Filter | Spot | Futures |
|---|---|---|
| `PRICE_FILTER` | `minPrice`, `maxPrice`, `tickSize` | same |
| `LOT_SIZE` + `MARKET_LOT_SIZE` | merged as max-of-mins, min-of-maxes, max-of-steps **[DUPLICATED]** `Spot/.../InstrumentFiltersConverter.cs:68-72` | identical logic `UsdFutures/.../InstrumentFiltersConverter.cs:70-74` |
| Notional **[DIVERGES]** | type `"NOTIONAL"`, fields `minNotional` and `maxNotional` — `Spot/.../InstrumentFiltersConverter.cs:96-98,137-141` | type `"MIN_NOTIONAL"`, single field `"notional"`, max **hard-coded** to `decimal.MaxValue` — `UsdFutures/.../InstrumentFiltersConverter.cs:98-100,139-140` |
| `MAX_NUM_ORDERS` **[DIVERGES]** | field `maxNumOrders` | field `limit` |

**Absence behaviour:** if the price, lot-size, notional or max-orders filter is missing, the filters
object reads as `null` and `InstrumentConverter` drops **the entire instrument**. An unenforced bound
therefore does not arrive as a zero field — the symbol simply never appears. Both converters, end of
array.

---

## 5. Enumerations

**Order side** — `BUY` / `SELL`, both venues. `Spot/.../OrderSides.cs:19`, `UsdFutures/.../OrderSides.cs:21`

**Order status** — `NEW`, `PARTIALLY_FILLED`, `FILLED`, `CANCELED`, `REJECTED`, `EXPIRED`. Spot also
folds `PENDING_CANCEL` → `Canceled` and `EXPIRED_IN_MATCH` → `Rejected`
(`Spot/.../OrderStatuses.cs:38,41`); futures folds only `EXPIRED_IN_MATCH` **[DIVERGES]**
(`UsdFutures/.../OrderStatuses.cs:40`) — confirmed against the documented futures status list, which has no
`PENDING_CANCEL`.

**[DRIFT] Spot documents `PENDING_NEW` and we do not map it** — an order in an order list waits in that
state until its working order fills. Our lookup would find nothing for it. `[DEAD]` in practice, since
the spot user path throws before any of this runs, but it is a hole in the mapping rather than a
deliberate omission. Binance also notes `PENDING_CANCEL` is "currently unused", so our folding of it
costs nothing and proves nothing.

**Symbol status is not a two-value question.** Spot documents `TRADING`, `END_OF_DAY`, `HALT`, `BREAK`
and `CANCEL_ONLY`; futures documents `PENDING_TRADING`, `TRADING`, `PRE_DELIVERING`, `DELIVERING`,
`DELIVERED`, `PRE_SETTLE`, `SETTLING`, `CLOSE`, `TRADING_HALT` and `TRADING_CANCEL_ONLY`. Our
converters admit `TRADING` alone, so **every other state drops the instrument entirely** — a halted
symbol becomes indistinguishable from one the exchange never listed. Defensible for opening a
position; wrong for a connector holding one, which will see the instrument vanish rather than learn it
was halted.

**Order type — [DIVERGES], entirely different naming schemes:**

| Domain | Spot | Futures |
|---|---|---|
| Limit | `LIMIT` | `LIMIT` |
| Market | `MARKET` | `MARKET` |
| StopLossMarket | `STOP_LOSS` | `STOP_MARKET` |
| TakeProfitMarket | `TAKE_PROFIT` | `TAKE_PROFIT_MARKET` |
| StopLossLimit | `STOP_LOSS_LIMIT` | `STOP` |
| TakeProfitLimit | `TAKE_PROFIT_LIMIT` | `TAKE_PROFIT` |

Spot `Spot/.../OrderTypes.cs:23-40` also folds `LIMIT_MAKER` → `Limit` on read; futures
`UsdFutures/.../OrderTypes.cs:22-41` folds `TRAILING_STOP_MARKET` → `StopLossMarket`.

> **[CONTESTED] — the four futures trigger types are refused on the order endpoint, observed live
> 2026-09-18.** Placing a real `STOP_MARKET` through `POST /fapi/v1/order` on a live account answered:
>
> > `Order type not supported for this endpoint. Please use the Algo Order API endpoints instead.`
>
> Documentation axis for the four futures trigger rows above (`STOP_MARKET`, `TAKE_PROFIT_MARKET`,
> `STOP`, `TAKE_PROFIT`) moves from `confirmed` to **`contested`**: the mapping is what the snapshotted
> documentation says, and the exchange refuses it. Verification moves to **`live`, negatively dated
> 2026-09-18** — the refusal is what is now observed, not the placement.
>
> The name mapping itself is not what is contested; the **endpoint** is. Which endpoint accepts them, what
> it takes and what it answers is a documentation question, and belongs to steps 1-2 rather than to the
> step that found it. Nothing here is amended in code until that is re-derived: guessing an API from one
> error message is how a manifest fills with fiction.
>
> Spot is not implicated — it was not exercised, and its four spot names are untouched by this.

**Margin type** (futures) — `"isolated"` / `"cross"`, lowercase, `UsdFutures/.../MarginTypes.cs:24-25`.
Note the same concept arrives as a **boolean** `isolated` over REST and as the string `mt` over the
stream — `GetAccountResponsePositionConverter.cs:78` vs `BalanceAndPositionUpdateEventPositionConverter.cs:77`.

**Position side** (futures only) — `BOTH` / `LONG` / `SHORT`, `UsdFutures/.../OrientationRanges.cs:24-26`.
Spot has no concept of it and hard-codes `Both` throughout its converters.

---

## 6. Error and status codes

**HTTP — and the two maps are not the same map. [DIVERGES], recorded 2026-09-18.**

| status | user endpoints | market endpoints |
|---|---|---|
| `418`, `429` | `TooManyRequests` | `TooManyRequests` |
| `400` | `BadRequest` | `BadRequest` |
| `401`, `403` | `Forbidden` | **`UnknownError`** |
| `404` | `NotFound` | **`UnknownError`** |
| anything else | `UnknownError` | `UnknownError` |

`Base/Shared/User/HttpExtensions/HttpRequestUserResultExtensions.cs`,
`Base/Shared/Market/HttpExtensions/HttpRequestMarketResultExtensions.cs:75-81`. The manifest presented
these as one mapping until 2026-09-18. They are two, and the market one is the narrower: an expired key
on a market endpoint arrives as `UnknownError` where the same key on a user endpoint arrives as
`Forbidden`.

**Which of the two wins** is itself a rule, at `HttpRequestUserResultExtensions.cs:62-66` and its market
twin: a **synthetic** result — network, abort, parse — yields to the HTTP status wherever that status
means something, while a real Binance code still takes precedence over the status. The residue
paragraph in the header used to read as though the code always won; it wins only against a body that
parsed.

**Binance codes**

| Code | Meaning | Mapped to |
|---|---|---|
| `-1003` | `TOO_MANY_REQUESTS` | `TooManyRequests` |
| `-2018` | `BALANCE_NOT_SUFFICIENT` | `InsufficientBalance` |
| `-2019` | `MARGIN_NOT_SUFFICIENT` | `InsufficientBalance` |
| any other negative | — | `BadRequest` |

`-1003` was missing from this table until 2026-09-18. It is a Binance code living in
`Base/Shared/Contracts/Domain/OperationResult.cs:18` beside the three local ones, mapped at
`HttpRequestUserResultExtensions.cs:94` and `…/Market/…:92` — which is how it escaped a table built by
reading the code list.

~~**[DUPLICATED] and already drifted**~~ — **no longer true, corrected 2026-09-16.** This said the two
special cases lived in per-venue copies while `Base` had only the generic fallback, so a new Binance
code needed adding twice and the third copy was already behind. There is one copy now:
`Base/Shared/User/HttpExtensions/HttpRequestUserResultExtensions.cs`, and both `-2018` and `-2019` are
in it. The per-venue files are gone — consolidated, and consolidated the right way round, keeping the
special cases rather than the fallback. `HttpRequestUserResultExtensionsTests` pins both codes.

Worth noting how this was found: not by reading the code, but because the anchors stopped resolving.
A manifest whose paths are checked mechanically reports a file that no longer exists; a manifest read
only by eye keeps describing a drift that somebody fixed a while ago.

**Local, not Binance:** `NetworkError=1`, `Aborted=2`, `ParseError=3` —
`Base/Shared/Contracts/Domain/OperationResult.cs:9-15`.

---

## 7. Rate limiting

| Fact | Where |
|---|---|
| Weight header `x-mbx-used-weight-1m`, matched case-insensitively | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:68` |
| A missing or unparseable header leaves the weight unchanged; the response is still returned | same, 73-99 |
| A missing header is logged at `Error`, except on a refusal - which does not carry one - where it is `Trace` | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:57,79-85` |
| `418` and `429` are treated alike in a second respect beyond status mapping: both are read as a refusal that states a deadline and carries no weight header | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:57` |
| The listen key request is counted against the same limiter as everything else | `Base/Internal/User/Services/ListenKeyResolver.cs:147` |
| Initial ceilings: spot `6000`/min, futures `2400`/min | `Spot/ProviderRegistrationContextExtensions.cs:109`, `UsdFutures/...:121` |
| Decay `300` every `3000`ms on **both** — i.e. 6000/min, which does not match the futures ceiling **[UNVERIFIED]** on the documentation axis. Verification, corrected 2026-09-18: `RateLimitCeilingTests.Decay_LowersTheRegisteredAmountOnTheRegisteredInterval` exists on **USD-M only** and pins the step one-sidedly — it tells `300` from any *larger* step and rejects intervals *shorter* than 3 s, so a step of 100 or an interval of ~10 s passes every assertion in it. Spot has no decay test at all. So: futures step `pinned` in one direction, futures interval and both spot constants `none` | same lines |
| Binance also returns an `x-mbx-order-*` family of order-count limit headers; the code knows to mask both prefixes in logs but reads neither. **Decided 2026-09-18: the order-rate limit is deliberately not tracked** - the connector learns of it by being refused, and the refusal path already pauses the limiter. Recorded as a fact rather than left as a gap, because "nobody read these headers" and "we chose not to" look identical in code | `Base/Shared/HttpExtensions/HttpRequestLogExtensions.cs:10` |
| Ceiling is overwritten at runtime from exchange-info's `REQUEST_WEIGHT` | `Spot/Internal/Market/MarketProvider.cs:63-65`, `UsdFutures/...:69-71` |
| Local gate at 80% of the ceiling, before the request is sent | `finance/Providers/src/Annium.Finance.Providers.Core/Internal/Shared/RateLimits/RateLimiter.cs:17`, computed at `:95`, gated at `:112,123` |
| A locally-gated request is synthesized as `429` | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:35-50` |

### How long we pause after a refusal — `HttpRequestRateExtensions.ReadPauseAsync`

**Corrected 2026-09-18. This manifest previously said "Nothing reads `Retry-After`", and that sentence
was false** — the code has read it for some time. The sentence is recorded here rather than quietly
deleted, because a manifest asserting the opposite of the code is the failure this document exists to
prevent, and it survived a full contract pass.

| Fact | Where |
|---|---|
| Binance sends `Retry-After` on a refusal, in **whole seconds**, matched case-insensitively; a value that parses and is positive wins outright | `:121-125` |
| Failing that, the deadline is read out of the **prose of the error body**: `banned until (\d+)`, case-insensitive, the capture taken as **Unix milliseconds** | `:141` (the `[GeneratedRegex]`), consumed at `:128-133` |
| Neither source is honoured beyond **one hour** — ours, not Binance's, but it bounds how far we will obey a deadline the exchange states | `:119` |

The regex is the entry worth staring at. **It is a contract over an exchange's human-readable text**,
which carries none of the stability of a field name: Binance can reword that message in a release note
nobody reads as an API change, and the failure is silent — no deadline parsed, `TimeSpan.Zero` returned,
and the client resumes straight into a ban it was told about. Documentation axis: `undocumented`. It
belongs on the highest-drift list, and is now on it.

---

## 8. Auth and signing

| Fact | Where |
|---|---|
| HMAC-SHA256 over the query string, hex, lowercase. Since 2026-01-15 the payload must be **percent-encoded before signing** or the request is rejected `-1022`. We sign `Uri.Query`, which is already encoded — compliant, but by construction rather than by intent, and **nothing pins it**: the golden-value test's query contains no character needing encoding | `Base/Internal/User/Services/SignatureService.cs:49-54` |
| Signed content is the full query string built so far, excluding `signature` | `Base/Shared/HttpExtensions/HttpRequestSignatureExtensions.cs:31-44` |
| `timestamp` is the **synced server time**, not the local clock | same, 35 |
| Listen key fetch and keep-alive both issue **`POST`** — correct: a `POST` on an account with an active key returns it and extends validity 60 minutes. The class doc comment claiming "periodic PUT" is what is wrong | `Base/Internal/User/Services/ListenKeyResolver.cs:145` |

**[CONTESTED] — the validity period is recorded twice, with two different values.** This section says a
`POST` extends validity for **60 minutes**, taken from the futures documentation on 2026-09-01.
`ListenKeyResolver.cs:220` says in a code comment that the period is **half an hour**. One of the two is
wrong and nothing in the tree distinguishes them: the keep-alive fires every 60 s, far inside either
window, so both readings produce identical behaviour and no test can tell them apart. Recorded
2026-09-18; to be settled from the documentation in step 2, not by picking the more plausible one.

**Settled 2026-09-01.** The futures documentation states that a `POST` on an account with an active
`listenKey` returns that key and extends its validity for 60 minutes. Our POST-only resolver is
correct; the class doc comment promising "periodic PUT keep-alive confirmations" is the error, and a
prior review reporting this as a defect was wrong. Still open for **spot**, where `PUT` is the
keep-alive and `POST` may mint a new key — relevant only if the spot user path is ever revived.

Keep-alive cadence is `60_000`ms with a `5_000`ms fetch retry — `Spot/ProviderConfiguration.cs:11`,
`UsdFutures/ProviderConfiguration.cs:16`.

---

## 9. Timing and lifecycle

| Fact | Where |
|---|---|
| Candle interval is always `"1m"`; page size `1000` | both `MarketProvider.cs` |
| Order and trade history paged at `1000`, in 7-day windows. Reach is capped by Binance at **3 months** for `userTrades` (reduced from 6 on 2026-08-26); `allOrders`' `symbol` became optional 2026-08-25, though we always send it | `UsdFutures/Internal/User/UserProvider.cs:44-53` |
| Server time synced at `2_000`ms until first success, `5_000`ms after | `Spot/ProviderConfiguration.cs:14`, `UsdFutures/...:19` |
| Exchange info is reloaded every `600_000`ms, with a `3_000`ms / `10_000`ms retry schedule | `Spot/Internal/Market/MarketConnectorFactory.cs:29`, `UsdFutures/...:34` |
| Account context, open orders and trades reload on their own cadences — `1_000` / `3_000` / `5_000`ms poll, `5_000` / `60_000`ms refresh | `Spot/ProviderConfiguration.cs:17-23`, `UsdFutures/ProviderConfiguration.cs:22-28` |
| Every history query is **widened for clock skew**: 10 s backwards and 50 ms forwards, so we always ask for slightly more than the caller asked for | `finance/Providers/src/Annium.Finance.Providers.Core/User/UserProviderHelper.cs:22-23` |

The three cadence rows above were added 2026-09-18. They had never been recorded, and they are the
rows that decide how much weighted traffic this module generates at rest: §7's ceilings say what we are
allowed, and these say what we spend without anybody asking for anything.

**[DEAD] — the whole spot user path, and further than this said.**
`Spot/Internal/User/UserConnectorFactory.cs:14-31` builds a connector with no listen-key resolver and no
user stream, and `Spot/Internal/User/UserConnector.cs:46-81` throws `NotImplementedException` for
trading and leverage. Corrected 2026-09-18: `Spot/Internal/User/UserProvider.cs:14-64` is a **stub that
issues no HTTP at all** — every load returns an empty success. So the dead set is larger than the rows
that carry the marker. Also unreachable, though written without one: spot `balances[]`, spot get-order,
spot init-order, the spot halves of the trade and maker-flag rows and of the cancel-GUID rule (§3), and
with them the spot `OrderSides` / `OrderStatuses` / `OrderTypes` tables (§5), whose only consumers are
those converters and the dead `QueryProcessor`.

Which also means the census line reading "Spot's account read paths stay `gated`" overstated the case:
there are no spot account read paths to gate. Recorded because reviving the path revives every one of
them at once.

---

## 10. Hard-coded exchange facts

| Value | Meaning | Where |
|---|---|---|
| `recvWindow = 30_000` | request validity window | `Base/Shared/HttpExtensions/HttpRequestSignatureExtensions.cs:25` |
| `newOrderRespType = "RESULT"` | response verbosity, every order | both `QueryProcessor.cs` |
| `timeInForce = "GTC"` | never `IOC` or `FOK` | both `QueryProcessor.cs` |
| ceilings `6000` / `2400`, decay `300`/`3000ms` | §7 | — |
| water mark `0.8f` | local gate fraction | `RateLimiter.cs:17` |
| page size `1000` | klines, orders, trades | §9 |
| `"BTCUSDT"` | assumed live and tradable on both venues | test fixtures, `Spot.Tests/.../MarketConnectorTests.cs:24` and the futures twin |
| precision `8`, or `2` when the code contains `"USD"` | fallback for a futures asset not seen as an instrument resource — a heuristic standing in for real precision data. **Only the `8` branch is exercised**, corrected 2026-09-18: `MarketProviderReadPathTests.LoadContext_GuessesAPrecisionForAssetsTheInstrumentsDoNotDescribe` asserts `8` twice, because the fixture's only USD-bearing asset is `USDT`, which the symbol already describes — so its precision comes from the instrument, not from the guess. The `Contains("USD") → 2` branch is reached by nothing. The test's own summary claims to cover it. Queued for step 5 | `UsdFutures/Internal/Market/MarketProvider.cs:66-67` |
| settlement currency **is** the quote asset | passed as both `Quote` and `Currency` | `Spot/.../InstrumentConverter.cs:66-67`, `UsdFutures/...:60-61` |

---

## Assumptions carried only by tests

- `"BTCUSDT"` being live and tradable is asserted only by fixtures that talk to the real exchange.
- The HMAC algorithm is pinned by a golden value in `test.env` (`TEST_EXPECTED_SIGNATURE`), not by any
  production assertion.
- ~~The `-2018` / `-2019` mappings have no test in `Base.Tests`~~ — stale, corrected 2026-09-18.
  `HttpRequestUserResultExtensionsTests:149-150` pins both, and there is no drifted copy left to miss.
- The `x-mbx-apikey` header name is matched by nothing offline: a grep for it across the whole test tree
  returns only prose. It is the one piece of signing scaffolding defended solely by the live run, where a
  wrong header name arrives as an authentication failure rather than as a named defect.
- **The listen key's own facts are carried by no test at all**, because `ListenKeyResolverTests` uses
  synthetic stand-ins for every one of them: the endpoint is a local `/listenKey` stub (`:49`), so the
  real path `/fapi/v1/listenKey` is unasserted; the HTTP method is never asserted, so `POST` is `gated`;
  the cadence runs on 50 ms rather than `60_000`/`5_000`; and the limiter handed in (`:358-378`) records
  nothing, so deleting `.WithRateDelay1M(...)` from `ListenKeyResolver.cs:147` breaks no test.

## Highest drift risk, ranked

Re-ranked 2026-09-18. What moved to the top is not a field name but a regex over prose.

1. **The ban-message regex** — `banned until (\d+)` parsed out of Binance's human-readable error text
   (§7). Reworded at any time, with no changelog entry, and the failure is silent: no deadline parsed,
   no pause taken, straight back into the ban.
2. **Order type wire strings** — six values, two venues, entirely different schemes, and four of the
   futures six are already `contested` (§5).
3. **The history sort assumption** — `.Last()` as the paging cursor over an order the exchange has never
   promised (§3). An unsorted page does not fail, it skips.
4. **Notional filter** — different type name, different field name, synthesized maximum (§4).
5. **Trade maker flag** — one letter apart between venues (§3).
6. **Kline positional indices** — a positional array has no names to protect it (§3).
7. **Listen-key validity period** — the code and this manifest record two different numbers (§8).

Left this list: **error codes**, which ranked third on the strength of "three copies, one out of sync".
There is one copy of each map now and both special cases are pinned. The entry survived a pass after it
stopped being true, which is the ordinary fate of a ranking nobody re-derives.
