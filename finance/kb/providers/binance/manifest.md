---
title: Binance contract manifest
type: provider-manifest
status: living
created: 2026-09-01
checked_against: 2026-09-18
docs_revision_spot: 828ca74b809cfedbd5602df328b5f706368d483b
docs_revision_postman: 367b396861b8debd5475444f3e0ce4caaf8dfe78
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

**`checked_against: 2026-09-18`** — the documentation axis was re-closed on that date against the
snapshot stored beside [`2026.09/2026.09.18-contract.md`](2026.09/2026.09.18-contract.md), with one
accepted gap recorded against its own entries.

**Read that report before trusting any `confirmed` written before it.** The 2026-09-01 pass closed this
axis too, and it had missed a migration that made four of our six futures order types unplaceable —
with the announcement sitting in a file that run had itself fetched and stored. The lesson is written
into the skill rather than only here: from 2026-09-18 an entry may be `confirmed` **only if it carries
a citation into the snapshot**, `<file>:<line>`. Entries below without one predate that rule and are
the ones to re-derive first.

The **verification** axis moves independently of that date, and did most recently on 2026-09-16, when
the read block ran with credentials for the first time.

**§10 and §11 were collected on 2026-09-21 and `checked_against` still reads 2026-09-18.** That is not
an oversight: both were read out of the snapshot pinned at `docs_revision_spot`, the same commit the
2026-09-18 run stored, so they are that date's documentation and not a later one. What they add is
**coverage** rather than freshness — facts this module depends on that no row of this document had,
because the code paths needing them did not exist. §11 also added a page to that snapshot,
`spot/web-socket-api.md`, fetched at the same commit; see its `SOURCES.md` for why it was missing.

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

**The two machine-readable indexes, found 2026-09-18 — check these first on any vendor.**

| path | what it is |
|---|---|
| `developers.binance.com/en/docs/llms.txt` | the page index **and** an "API Reference" chapter listing every endpoint by method and path. Store it: an endpoint appearing or disappearing there is a contract change needing no changelog entry |
| `developers.binance.com/en/docs/llms-full.txt` | the documentation set as one file, 8.2 MB, 833 documents, including the per-endpoint reference |

~~**Not found, still a gap:** the per-endpoint futures reference pages.~~ **Closed 2026-09-18** — not by
finding the catalog URL, which still returns the HTML shell for every form tried, but because the same
material is in `llms-full.txt`. The previous run had `llms.txt` in hand and read it as a list of page
paths; the endpoint chapter is 450 lines further down the same file. Extract what the manifest needs
and store the extract with the source's hash and line range.

**The Postman repository was restructured.** Collections now live under `collections/` with product
names — `collections/Binance Derivatives Trading USDS Futures API.json`. The old paths
(`usd-futures/postman-usds-futures.json`) return `404: Not Found` as a **14-byte file with exit code
0**, which no check but a size comparison will catch.

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
| Spot cancel-replace endpoint — ~~**[DEAD]**~~ **issued since 2026-09-21**, and validated live | spot | `Spot/Internal/Shared/Endpoints.cs` (`ModifyOrderUriPath`), issued at `Spot/Internal/User/UserConnector.cs:279`; parameters at `Spot/Internal/User/Services/QueryProcessor.cs:68-111` |

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

### Spot orders — ~~**[DEAD]**~~ **live since 2026-09-21** — `Spot/Internal/User/Services/QueryProcessor.cs`

Same base set minus `positionSide`/`reduceOnly` (24-28); modify via cancel-replace with
`cancelReplaceMode="STOP_ON_FAILURE"`, `cancelOrigClientOrderId`, `newClientOrderId`,
`timeInForce="GTC"` (72-79). Invoked from `Spot/Internal/User/UserConnector.cs:232,279,314,367` — placement, cancel-replace, cancel and cancel-all — and exercised live on 2026-09-21.

Its **response** shape is encoded too: top-level `code` / `msg` / `data`, with `data.cancelResponse`
and `data.newOrderResponse` nested inside, and the cancel leg's error preferred over the init leg's
when only one failed — `Spot/.../ModifyOrderFailureResponseConverter.cs:60-129`,
`ModifyOrderSuccessResponseConverter.cs:49-50`. Read by the connector's modify path since 2026-09-21.

---

## 3. Response fields

### Exchange info

- Rate limits: entry with `rateLimitType`/`limit`; only `"REQUEST_WEIGHT"` is read, and its window is
  **assumed to already be one minute** — `Base/Market/Contracts/Converters/RateLimitsConverter.cs:37-44`, field names read at `:68,71`
  **[UNVERIFIED]**. A payload carrying limits but no `REQUEST_WEIGHT` entry **drops the whole exchange
  info** rather than yielding one with no ceiling — `pinned` 2026-09-17 by
  `ExchangeInfoWithoutRequestWeightLimit_IsDropped` (which lives in the **futures** test project, for a converter shared by both venues - so spot runs the pinned code without owning the test), because the alternative failure is a ban arriving
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
- **~~[UNVERIFIED]~~ → `live` 2026-09-19. Confirmed exactly as assumed.** A one-way account reports one
  `positions[]` row **per symbol**, whether or not a position is open, always `positionSide=BOTH`: the
  probe read **905 rows, 905 distinct symbols, every one `BOTH`, every one with `positionAmt` zero** on a
  flat account. Evidence: [`2026.09/2026.09.19-algo-probe/account.positions-row.json`](2026.09/2026.09.19-algo-probe/account.positions-row.json).
  The last of the three `[UNVERIFIED]` markers, and the one the documentation never stated.
  Two consequences the assumption alone did not carry: **a context load is ~340 KB** and grows with the
  venue's listing count, on a loader that runs every second by configuration; and a row carries more
  than §3 records — `breakEvenPrice`, `notional`, `maxNotional`, `askNotional`, `bidNotional`,
  `isolatedWallet`, `openOrderInitialMargin`, `positionInitialMargin` are all present and unread.
- The assumption above is what the test fixture's position-mode precondition depends on —
  `finance/Providers/tests/Annium.Finance.Providers.Tests.Lib/User/UserConnectorTestBase.cs:614-627`
  (`EnsureOneWayPositionMode`), which is in the **Write** block, so the assumption is `gated`. The read
  suite looks as though it also covers it and does not: `UserConnectorReadTestBase.cs:82` asserts
  `positions.Count.IsGreaterOrEqual(0)`, which a count can never fail — `vacuous`, found 2026-09-18,
  queued for step 5

### Orders and trades

> **[NEW, unread] `TRADE_LITE`** — a user-stream event this module does not handle, captured 2026-09-19.
> It arrives **before** `ORDER_TRADE_UPDATE` and carries the fill: `L` last price, `l` last quantity, `t`
> trade id, `i` order id, `c` client order id, `s`, `S`, `q`, `m`, plus `E`/`T`. The earliest notice of a
> fill the exchange gives, and nothing reads it.
>
> **[DEFECT, ours] `limit` means opposite things on `userTrades` and `allOrders`.** Measured 2026-09-19:
> `GET /fapi/v1/userTrades?limit=5` returns the **five oldest** trades of the account, while
> `GET /fapi/v1/allOrders?limit=3` returns the **three newest**. So on the trade endpoint `limit` truncates
> from the start of the window and on the order endpoint from the end.
>
> `LoadLatestTradesAsync` asks for the latest page with `limit=1000`, so it is safe only while an account
> has fewer than a thousand trades in the window. Past that it receives the *earliest* thousand and cannot
> tell - a full page of real trades is exactly what it expected. The history path is unaffected: it walks
> windows by `startTime` and takes `.Last()` as the cursor, which the ascending order this endpoint returns
> makes correct.


| Fact | Where |
|---|---|
| Spot order: `orderId`, `clientOrderId`, `symbol`, `type`, `side`, `origQty`, `price`, `stopPrice`, `status`, `executedQty`, `cummulativeQuoteQty` (executed price **derived** as sum ÷ qty), `time`, `updateTime` | `Spot/.../GetOrderResponseConverter.cs:79-121` |
| Futures order: same core plus `positionSide`, `reduceOnly`, and `avgPrice` used **directly** — **[DIVERGES]** | `UsdFutures/.../GetOrderResponseConverter.cs:87-135` |
| Spot init-order uses `workingTime` for created and `transactTime` for updated — **[DIVERGES]** from its own get-order, which uses `time`/`updateTime` | `Spot/.../InitOrderResponseConverter.cs:115-120` |
| Futures init-order has **no creation timestamp**; `updateTime` serves as both | `UsdFutures/.../InitOrderResponseConverter.cs:126-128` |
| **~~[CONTESTED]~~ → [CHANGED], and measured 2026-09-19.** `avgPrice` and `cumQuote` are **absent from the placement acknowledgement under any name** - the question was whether we were reading a renamed field, and there is no field to read. The same order queried back seconds later carries `avgPrice` and `cumQuote`; the placement carries neither, plus a **new `cumQty`** that repeats `executedQty` and is not a price. So the placement says how much filled and refuses to say at what. Raw evidence: [`2026.09/2026.09.19-raw-exchange/`](2026.09/2026.09.19-raw-exchange/). **The price does arrive**, on the stream, in the same second: `ORDER_TRADE_UPDATE` with `X: FILLED` carries `ap`, which this module's order-update converter already reads | `UsdFutures/.../InitOrderResponseConverter.cs:123`; docs `…/usd-futures/coin-futures_Important-CM-UM-Integration-Notice.md:80-106` |
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
| Spot `executionReport` — ~~**[DEAD]**~~ ~~**[DRIFT]**~~, `live` 2026-09-21 | `e`,`s`,`t`,`i`,`c`,`o`,`S`,`q`,`p`,`P`,`X`,`z`,`Z`,`l`,`L`,`n`,`N`,`m`,`O`,`T` — **the field list holds and the envelope is handled.** The converters read a bare object, and the transport unwraps `{"subscriptionId", "event": {…}}` before handing the event over — see §11. Every field above was read off a real fill on 2026-09-21, including `N` arriving **null** on the acceptance event | `Spot/.../OrderUpdateEventConverter.cs:99-163` |
| Spot `outboundAccountPosition` — ~~**[DEAD]**~~ ~~**[DRIFT]**~~, `live` 2026-09-21 | `e`,`u`,`B[]` with `a`,`f`,`l` — same envelope, same handling. **`B[]` is a delta**: it carries only the balances that changed, so the connector reads none of them and reloads the account instead | `Spot/.../AccountUpdateEventConverter.cs:63-84` |
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
| Price band **[DIVERGES]**, read since 2026-09-21 | type `"PERCENT_PRICE_BY_SIDE"`, four fields — `bidMultiplierUp`, `bidMultiplierDown`, `askMultiplierUp`, `askMultiplierDown` — plus `avgPriceMins`, against an average of recent trades — `…/spot/filters.md:81-108` | type `"PERCENT_PRICE"`, two fields — `multiplierUp`, `multiplierDown` — plus `multiplierDecimal`, against the **mark price** — `…/usd-futures/common-definition.md:276-294` |
| `MAX_NUM_ALGO_ORDERS` — **[CONTESTED]**, see below | — | `…/usd-futures/common-definition.md:265-273` vs `…/usd-futures/change-log.md:632-633` |

**Citations for every filter above**, collected 2026-09-18 from
`2026.09/2026.09.18-docs/usd-futures/common-definition.md`: `PRICE_FILTER` `:170`, `LOT_SIZE` `:197`,
`MARKET_LOT_SIZE` `:223`, `MAX_NUM_ORDERS` `:249`, `MAX_NUM_ALGO_ORDERS` `:265`, `MIN_NOTIONAL` `:302`
with its single field `notional` and the example value `"5.0"`. Futures documents **no** `NOTIONAL`
filter — grepped, zero hits — so the `[DIVERGES]` against spot's spelling is measured rather than
assumed.

**The price band is one filter with two spellings, and the divergence is not only in the field names.**
Spot states both ends for each side: a buy between `bidMultiplierDown` and `bidMultiplierUp`, a sell
between `askMultiplierDown` and `askMultiplierUp` (`…/spot/filters.md:88-96`). Futures states **one
inequality per side** — `price <= markPrice * multiplierUp` for a buy, `price >= markPrice *
multiplierDown` for a sell (`…/usd-futures/common-definition.md:293-294`) — which leaves a buy with no
floor and a sell with no ceiling.

Reading the futures pair as a two-sided band is the mistake this entry exists to prevent, and it is
refuted by our own trading block rather than by argument: it rests buys at 0.7 of the market while
`DOTUSDT` publishes `multiplierDown: 0.9500`, and the exchange has accepted them on every run. A
symmetric reading would have the provider refuse orders that work.

**Census, taken live 2026-09-21** over both public `exchangeInfo` payloads. Every symbol open for
trading carries the band — spot 1368 of 1368 `PERCENT_PRICE_BY_SIDE`, futures 773 of 773
`PERCENT_PRICE` — and spot's window is `avgPriceMins: 5` on all 1368, with no symbol carrying the older
`PERCENT_PRICE`. The bands are not uniform: spot's most common quadruple is `1.2 / 0.5 / 2 / 0.8` (898
symbols) and futures' most common pair is `1.1500 / 0.8500` (426), so a caller cannot assume a house
default. `DOTUSDT` reads `1.2 / 0.5 / 2 / 0.8` on spot and `1.0500 / 0.9500` on futures — the spot floor
of half the reference being exactly the refusal measured on 2026-09-21 in §11.

Universal today, but read as **optional**: an absent band is representable as an unbounded one, where an
absent price or lot filter leaves nothing to compute an order from and drops the symbol. Pinned in both
directions, both venues, in `InstrumentFiltersConverterTests`.

**~~[CONTESTED]~~ `MAX_NUM_ALGO_ORDERS` — settled live 2026-09-19, in the changelog's favour.**

Two pages of the 2026-09-18 snapshot disagreed: `common-definition.md:265-273` documents it as an
`exchangeInfo` filter with `limit: 100`, while `change-log.md:632-633`, dated 2025-12-29, says it was
removed from that endpoint and the limit is a flat 200 across all symbols. Neither was picked, and the
manifest said the payload would decide.

It did. A live `GET /fapi/v1/exchangeInfo` on 2026-09-19 carries **no `MAX_NUM_ALGO_ORDERS` at all** —
grepped, zero hits across 1.1 MB. The reference page is stale; the conditional-order bound is not a
filter we can read and is not per-symbol. **Nothing to implement**, which is the opposite of what the
first draft of this entry concluded from the reference page alone.

The filter types the live payload does carry, on `DOTUSDT`: `PRICE_FILTER`, `LOT_SIZE`,
`MARKET_LOT_SIZE`, `MAX_NUM_ORDERS`, `MIN_NOTIONAL`, `PERCENT_PRICE`, **`POSITION_RISK_CONTROL`**. The
last is `[NEW]` — undocumented in the snapshot and unread by us; it arrived as
`{"filterType":"POSITION_RISK_CONTROL","positionControlSide":"NONE"}`. Unread is safe here, since
`InstrumentFiltersConverter` ignores unknown types, but it is a fact about the payload we did not have.

Recorded as a caution about method as much as about the filter: the first draft took the reference page
alone and wrote the 100 down as fact. The changelog sweep contradicted it within the hour, and the live
payload confirmed the sweep. One source read confidently is how a manifest fills with fiction.

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

**~~[DRIFT]~~ Spot's `PENDING_NEW` is mapped as of 2026-09-21** — an order in an order list waits in
that state until its working order fills, so it is live and unfilled, which is what `New` says. Folded
like the `PENDING_CANCEL` beside it and `pinned` by the parse theory.

Worth keeping for the shape of it: the lookup **throws** on a value it does not know, so this was not
a status that would have arrived wrong - it was one pending leg failing the parse of the whole list it
came in. It was `[DEAD]` in practice while the spot user path issued no requests; since 2026-09-21 it does, so the lookup is reachable again. Binance also notes
`PENDING_CANCEL` is "currently unused", so our folding of it costs nothing and proves nothing.

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

**Futures citations, collected 2026-09-18** from `2026.09/2026.09.18-docs/usd-futures/common-definition.md`:
contract status `:23-34` (all ten, matching what we admit and what we drop), order status `:36-44`
(seven, and **no `PENDING_CANCEL`** — the `[DIVERGES]` against spot is confirmed), order types `:46-53`
(all seven, unchanged), order side `:55-58`, position side `:60-64`, `timeInForce` `:66-75`,
`newOrderRespType` `:82-85` (`ACK` and `RESULT` only, so our hard-coded `RESULT` is valid), kline
intervals `:88-105` including `1m`.

**The futures order-type list is unchanged, and that matters for the contested rows below.** All seven
names, the four trigger types included, are still documented exactly as this manifest records them. The
migration took the *endpoint*, not the vocabulary — which is what the contested block said before it
could be checked, and is now checked.

> **~~[CONTESTED]~~ → [CHANGED, blocking]. Settled from documentation 2026-09-18.** The four futures
> trigger types are refused on `POST /fapi/v1/order` because **they were migrated to a different
> endpoint family on 2025-12-09**, announced 2025-11-06 — `…/usd-futures/change-log.md:729`.
>
> The live refusal we recorded on 2026-09-18 is `-4120 STOP_ORDER_SWITCH_ALGO`, and its documented text
> is our observed text word for word (`…/usd-futures/error-code.md:900` area, `### -4120
> STOP_ORDER_SWITCH_ALGO — Order type not supported for this endpoint. Please use the Algo Order API
> endpoints instead.`).
>
> **The endpoint is not the one the error message suggests.** "Algo Order API" also names
> `/sapi/v1/algo/futures/*`, a separate product for execution algorithms — TWAP and volume
> participation. Conditional orders went to **`/fapi/v1/algoOrder`**, inside the same futures API. A
> reader following the error message literally would implement the wrong product; this is exactly the
> fiction the contested marker was protecting against.
>
> | | |
> |---|---|
> | affected types | `STOP_MARKET`, `TAKE_PROFIT_MARKET`, `STOP`, `TAKE_PROFIT`, **and `TRAILING_STOP_MARKET`** — all five, so the trailing type we fold into `StopLossMarket` on read is implicated too |
> | also blocked | `POST /fapi/v1/batchOrders`, which we do not use |
> | place / cancel / query | `POST` / `DELETE` / `GET /fapi/v1/algoOrder` |
> | cancel all, open, history | `DELETE /fapi/v1/algoOpenOrders`, `GET /fapi/v1/openAlgoOrders`, `GET /fapi/v1/allAlgoOrders` |
> | stream event | new `ALGO_UPDATE`, with **its own status vocabulary**: `NEW`, `TRIGGERING`, `TRIGGERED`, `FINISHED`, `REJECTED`, `EXPIRED`, `CANCELED` (`…/usd-futures/user-data-streams.md:51523`-region, "Event: Algo Order Update") |
> | websocket API | `algoOrder.place`, `algoOrder.cancel` |
>
> **Request shape, `confirmed` at tier 1** from the Postman collection
> (`…/usd-futures/postman-usds-futures.json`, "New Algo Order (TRADE)"). It is not the order endpoint
> with a different path — four of our field names change:
>
> | ours today | on `algoOrder` |
> |---|---|
> | — | **`algoType`**, required, `CONDITIONAL` for this family |
> | `stopPrice` | **`triggerPrice`** |
> | `newClientOrderId` | **`clientAlgoId`**, `^[\.A-Z\:/a-z0-9_-]{1,36}$` — a GUID satisfies it |
> | `orderId` | **`algoId`** |
> | cancel sends `symbol` | cancel takes **`algoId`/`clientAlgoId` only, no `symbol`** |
>
> Carried over unchanged: `side`, `positionSide`, `type`, `quantity`, `price`, `timeInForce`,
> `reduceOnly`, `closePosition`, `workingType`, `priceProtect`, `newOrderRespType`, and
> `activatePrice`/`callbackRate` for the trailing type.
>
> **Three behavioural changes stated in the announcement**, none of them visible in a parameter list:
> no margin check before a conditional order triggers; `GTE_GTC` orders now depend on positions rather
> than on opposite-side open orders; and **modification of an untriggered conditional order is not
> supported** — which settles, for this family, the modify question step 5c decided on other grounds.
>
> **What is still not known:** the response shape. The per-endpoint reference gives operation
> descriptions, not schemas, and the Postman collections carry no response examples. So the placement
> and query responses of `/fapi/v1/algoOrder`, and the payload of `ALGO_UPDATE`, are `unretrievable`
> from this snapshot — the same gap that leaves `avgPrice` contested below.
>
> **Response shapes, `live` 2026-09-19** — read from a real conditional order placed, listed and
> cancelled on a live account. Raw answers stored in
> [`2026.09/2026.09.19-algo-probe/`](2026.09/2026.09.19-algo-probe/); the documentation publishes none of
> this, so these files are the only source the converters can be written from.
>
> **Placement — `POST /fapi/v1/algoOrder`.** The field names are not the order endpoint's:
>
> | field | note |
> |---|---|
> | `algoId` | **a JSON number**, e.g. `4000001910058351` — not the string `orderId` of the order endpoint |
> | `clientAlgoId` | the GUID we sent, echoed |
> | `algoType` | `CONDITIONAL` |
> | **`orderType`** | `STOP_MARKET` — the order endpoint spells this field `type` **[DIVERGES]** |
> | **`algoStatus`** | `NEW` — the order endpoint spells it `status` **[DIVERGES]** |
> | `triggerPrice` | what `stopPrice` is called here |
> | `quantity`, `price`, `side`, `positionSide`, `timeInForce`, `workingType`, `priceMatch`, `closePosition`, `priceProtect`, `reduceOnly`, `selfTradePreventionMode`, `goodTillDate`, `icebergQuantity` | present; `timeInForce=GTC`, `workingType=CONTRACT_PRICE` and `selfTradePreventionMode=EXPIRE_MAKER` came back as defaults we never sent |
> | `createTime`, `updateTime`, `triggerTime` | `triggerTime` is `0` until the order triggers |
>
> **No `avgPrice`, no `executedQty`, no `cumQuote`** on the placement answer — consistent with their
> removal from order placement responses recorded in §3, and a second confirmation of it.
>
> **Listing — `GET /fapi/v1/openAlgoOrders`.** A **bare JSON array**, `[]` when empty; not an object
> wrapping one. Each element carries every placement field **plus three the placement answer lacks**:
> `actualOrderId` (empty string until triggered), `actualQty` (`"0.0"`), `isActivated` (`false`). Those
> three are the link from a conditional order to the real order it becomes, so they are the fields that
> matter for ingestion.
>
> **Cancellation — `DELETE /fapi/v1/algoOrder`.** Answers
> `{"algoId":…, "clientAlgoId":…, "code":"200", "msg":"success"}`. **`code` is a JSON *string* here**,
> where the error envelope this module already parses (`OperationResult`) reads `code` as a number. A
> converter reusing that type on this response fails on a success. Recorded because it is the kind of
> thing that is found at runtime and read as our defect.
>
> **[UNDOCUMENTED] `GET /fapi/v1/algoOrder?algoId=…` answered `-2013 Order does not exist.`** for an order
> that demonstrably existed — `openAlgoOrders` had listed it moments earlier, and the cancel that followed
> succeeded on the same `algoId`. The parameter set matches what the official Postman collection
> documents (`algoId` or `clientAlgoId`, nothing else required). So either the query needs something
> undocumented, or it reads a different store than the open-orders endpoint. **Not resolved**, and worth
> resolving before the migration relies on it: `openAlgoOrders` is the endpoint proven to work.
>
> **[BLOCKING for ingestion] A conditional order never enters the ordinary order store.** Measured
> 2026-09-19, after the probe's order could not be found in the account's order history. The same order,
> by `algoId` and `clientAlgoId`, is present in `GET /fapi/v1/allAlgoOrders?symbol=…` with
> `algoStatus: CANCELED`; `GET /fapi/v1/allOrders?symbol=…` returns only orders from the previous day's
> run and nothing from the probe at all.
>
> So the two stores are disjoint until a trigger fires. `UserProvider.LoadOrdersAsync` reads `allOrders`,
> which means that after the migration **a connector rebuilding state from order history alone loses every
> conditional order the account holds** — and loses them silently, as an empty history rather than an
> error. Conditional orders need `allAlgoOrders` as a second source, per symbol.
>
> **And there are three response shapes, not one**, which a single converter cannot serve:
>
> | endpoint | fields beyond the common set |
> |---|---|
> | `POST /fapi/v1/algoOrder` | none |
> | `GET /fapi/v1/openAlgoOrders` | `actualOrderId`, `actualQty`, `isActivated` |
> | `GET /fapi/v1/allAlgoOrders` | `actualOrderId`, `actualPrice`, `tpOrderType` — **no** `actualQty`, **no** `isActivated` |
>
> **What a trigger does, measured live 2026-09-19** by letting one conditional order fire and closing the
> position it opened. This is what settles the status mapping, which no reading of the documentation could:
>
> | observed | value |
> |---|---|
> | the conditional order becomes an **ordinary order** | `allOrders` gains `orderId 33877541028`, `type MARKET`, `status FILLED`, `origQty 4.9`, `avgPrice 1.1325` |
> | **its `clientOrderId` is our `clientAlgoId`** | `60d0f110-…`, the same GUID we sent to `algoOrder` |
> | the algo record settles at | `algoStatus: FINISHED`, `actualOrderId` equal to that `orderId`, `actualPrice`, `actualQty`, and **`actualType`** — a field absent from every untriggered shape |
> | `openAlgoOrders` | empty: a triggered order leaves it |
> | cancelling a finished one | `-2011 Unknown order sent.` — not the `-2013` an unknown id gives |
> | `isActivated` | **`false` even on the triggered order**, so it does not mean "triggered". Unexplained; do not read it |
>
> **The identity is preserved across the trigger**, and that is the fact the whole migration rests on. Our
> domain keys an order by the GUID it sent as the client id; the exchange carries that same GUID from the
> conditional order onto the ordinary order it becomes. So a conditional order does not change identity
> when it fires — it changes *store*.
>
> Which decides the status mapping without guessing at `FINISHED`:
>
> | `algoStatus` | domain | why |
> |---|---|---|
> | `NEW`, `TRIGGERING` | `New` | open and untriggered; the algo store is the only place it exists |
> | `TRIGGERED`, `FINISHED` | **not mapped — the record is dropped** | an ordinary order with the same id exists and carries the real outcome. Mapping `FINISHED` to `Filled` would have been wrong whenever the triggered order was cancelled in the book, which the documentation says plainly and which no reading could have resolved |
> | `CANCELED` | `Canceled` | terminal, never triggered |
> | `REJECTED` | `Rejected` | refused by the matching engine |
> | `EXPIRED` | `Expired` | cancelled by the system |
>
> One more thing the trigger established, recorded because it shapes the tests rather than the code: **a
> conditional order that would fire immediately cannot be placed at all** — `-2021 Order would immediately
> trigger`. A test that wants a trigger has to wait for the market to reach it.
>
> **The `ALGO_UPDATE` payload, captured live 2026-09-19** by placing and cancelling one conditional order
> with the user data stream open. Stored at
> [`2026.09/2026.09.19-algo-probe/ALGO_UPDATE.new.json`](2026.09/2026.09.19-algo-probe/ALGO_UPDATE.new.json)
> and its `CANCELED` twin; the exchange documents this event only through a schema component that cannot
> be fetched.
>
> Top level `e` (`ALGO_UPDATE`), `T`, `E`, and the order under `o`. Inside `o`: `caid` clientAlgoId,
> `aid` algoId (a number), `at` algoType, **`o` orderType**, `s`, `S`, `ps`, `f`, `q`, `X` algoStatus,
> `ai` actualOrderId, `tp` triggerPrice, `p`, `V`, `wt`, `pm`, `cp`, `pP`, `R`, `tt`, `gtd`, `ia`.
>
> **The event nests a field under its own name.** The order object is `o` and the order *type* inside it
> is `o` as well. A reader matching on property name without tracking depth reads one as the other, and
> does it silently. Worth stating here rather than only in the converter, because it is the sort of thing
> a second implementation repeats.
>
> Remediation belongs to steps 3-5, specified in `status.md`, not performed here.
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

**~~[DEFECT] `-1008` is a throttle and we classify it as a bad request.~~ Fixed 2026-09-19**, on both
mappings, `pinned` on both. Found 2026-09-18 by the changelog sweep. `error-code.md:52-56` defines `-1008 Request Throttled` with two messages, the second
added 2025-10-23: *"Request throttled by system-level protection. Reduce-only/close-position orders are
exempt. Please try again."*

It fell into "any other negative" above and arrived at a caller as `BadRequest` — a request the caller
might have malformed, rather than a rate it must slow down. **And the investigation found the deeper
half**: the limiter took no pause, and not because of the status mapping. `Block` was only called for a
positive pause, while both deadline readers returned zero for a response that stated none — so no
refusal without a deadline ever paused anything, whatever its code. Both halves are fixed; the second
is in §7.

The argument for the `MapOperationCode` split queued in `status.md` stands and is sharper for it: the
taxonomy is not tidiness, it decides whether a control path runs.

**The split was made 2026-09-19**, from the real list rather than from a guess. Codes are classified by
**what a caller would do differently** — `BinanceErrors.Classify`, one table shared by both mappings, so
the two can no longer drift apart as they did before:

| class | codes | user status | market status |
|---|---|---|---|
| transport, retryable as-is | `-1001` DISCONNECTED, `-1007` TIMEOUT, `-1016` SERVICE_SHUTTING_DOWN | `NetworkError` | `NetworkError` |
| rate | `-1003`, `-1008`, `-1015` TOO_MANY_ORDERS | `TooManyRequests` | `TooManyRequests` |
| access | `-1002`, `-1011` NON_WHITE_LIST, `-2014`, `-2015`, `-2017` API_KEYS_LOCKED | `Forbidden` | `Forbidden` |
| absent | `-1099`, `-1121` BAD_SYMBOL, `-2013` NO_SUCH_ORDER | `NotFound` | `NotFound` |
| funds | `-2018`, `-2019` | `InsufficientBalance` | folds to `BadRequest` — market endpoints spend nothing |
| refused on the merits | every other negative code | `BadRequest` | `BadRequest` |

**`MarketOperationStatus` gained `Forbidden`** to make this expressible; it had none, which is why its
HTTP map sent `401`/`403`/`404` to `UnknownError` while the user map named all three. That asymmetry was
never a decision.

The gain is not tidiness. An expired key, an order already cancelled and a malformed price were one
status, and they want three different responses — fix configuration, look elsewhere, correct the input.
Every class above is `pinned`, on both mappings, by codes named after the documented constant so the
table can be checked against `error-code.md` without guessing what a number meant.

**The real code list behind it.** `error-code.md` carries **206 codes** in five documented families:

| family | line | what a caller would do |
|---|---|---|
| `10xx` general server or network | `:15` | retry — includes `-1008` throttle and `-1021` timestamp, the latter wanting a clock re-sync first |
| `11xx` request issues | `:104` | stop and fix the request or the configuration; `-1125` invalid listen key belongs here and is recoverable by re-fetching |
| `20xx` processing | `:228` | mixed — `-2018`/`-2019` reduce size, `-2015` rejected key is configuration |
| `40xx` filters and other | `:309` | adjust the order to the instrument's filters; `-4120` lives here |
| `50xx` order execution | `:842` | accept a market refusal; `-5047` lives here |

The families are Binance's own grouping, not one we invented — which was the objection that kept this
queued. Writing the table is now a reading exercise rather than a taxonomy exercise.

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
| Weight header `x-mbx-used-weight-1m`, matched case-insensitively. **The documented name is a template** — `X-MBX-USED-WEIGHT-(intervalNum)(intervalLetter)`, emitted "for all request rate limiters defined" — so the `1m` suffix is only right while the `REQUEST_WEIGHT` limiter's interval is one minute. It is: `"interval": "MINUTE", "intervalNum": 1`. `confirmed` 2026-09-18 | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:68`; docs `2026.09/2026.09.18-docs/usd-futures/general-info.md:145`, interval at `…/common-definition.md:133-134` |
| IP bans **scale in duration from 2 minutes to 3 days** for repeat offenders. `confirmed` 2026-09-18 | docs `…/usd-futures/general-info.md:152` |
| Rate limits are counted **against the IP, not the API key**. `confirmed` 2026-09-18 | docs `…/usd-futures/general-info.md:155` |
| **USDⓈ-M and COIN-M share one pool**, since the 2026-06-30 architecture integration: a single 2400/min IP weight budget counted on the same `X-MBX-USED-WEIGHT-1M` header whether the request went to `fapi` or `dapi`, and a single order budget of 1200/min and 300/10s. `confirmed` 2026-09-18 | docs `…/coin-futures_Important-CM-UM-Integration-Notice.md:37-45` |
| A missing or unparseable header leaves the weight unchanged; the response is still returned | same, 73-99 |
| A missing header is logged at `Error`, except on a refusal - which does not carry one - where it is `Trace` | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:57,79-85` |
| `418` and `429` are treated alike in a second respect beyond status mapping: both are read as a refusal that states a deadline and carries no weight header | `Base/Shared/HttpExtensions/HttpRequestRateExtensions.cs:57` |
| The listen key request is counted against the same limiter as everything else | `Base/Internal/User/Services/ListenKeyResolver.cs:147` |
| Initial ceilings: spot `6000`/min, futures `2400`/min. Both `confirmed` 2026-09-18 | `Spot/ProviderRegistrationContextExtensions.cs:109`, `UsdFutures/...:121`; docs `…/spot/enums.md:142` and `…/usd-futures/common-definition.md:135` |
| Decay: spot `300`, futures **`120`**, both every `3000`ms — each draining exactly what its ceiling allows. **~~[DEFECT, ours]~~ fixed 2026-09-19.** Futures carried spot's `300` against a 2400/min ceiling, handing budget back 2.5× faster than the exchange, while its own XML doc claimed the two matched. `pinned` on both venues now, bracketed from both sides on step and bounded on interval — the previous test could only tell the step from a larger one and passed against the wrong value by construction; spot had no decay test at all | same lines |
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
| Neither source is honoured beyond **3 days**, matching the longest ban documented (`…/usd-futures/general-info.md:152`). **~~[DEFECT, ours]~~ fixed 2026-09-19**: the cap was one hour, below the documented maximum by a factor of 72, so a long ban was resumed into — the behaviour that same page names as the cause of longer bans | `_maxPause` |
| **A refusal that states no deadline still pauses, for one minute** — the weight window it guards. **~~[DEFECT, ours]~~ fixed 2026-09-19**, and the graver of the two: `Block` was called only for a positive pause and both deadline readers returned zero when the response said nothing, so the commonest refusal of all cost nothing at all. A test asserted this as correct (`RefusalWithoutADeadline_PausesNothing`) and now asserts the opposite | `_defaultPause` |

The regex is the entry worth staring at. **It is a contract over an exchange's human-readable text**,
which carries none of the stability of a field name: Binance can reword that message in a release note
nobody reads as an API change, and the failure is silent — no deadline parsed, `TimeSpan.Zero` returned,
and the client resumes straight into a ban it was told about. Documentation axis: `undocumented`,
**measured** 2026-09-18 — neither `Retry-After` nor any ban-message wording appears anywhere in the
futures snapshot. It belongs on the highest-drift list, and is now on it.

**On the severity of the two rate-limit defects above, stated honestly.** Neither is "a ban is
coming". `_usedWeight` is overwritten from the weight header on **every** response, so the exchange's
own count arrives continuously and the local decay only has to bridge the gap between responses — at a
response every few hundred milliseconds it subtracts little. What the wrong decay does is bias a
fallback mechanism in the direction of permissiveness, and it is wrong in the one situation the
fallback exists for: when responses stop arriving. The pause cap is the graver of the two, because it
applies exactly when the exchange has already refused us.

---

## 8. Auth and signing

| Fact | Where |
|---|---|
| HMAC-SHA256 over the query string, hex, lowercase. Since 2026-01-15 the payload must be **percent-encoded before signing** or the request is rejected `-1022`. We sign `Uri.Query`, which is already encoded — compliant, but by construction rather than by intent, and **nothing pins it**: the golden-value test's query contains no character needing encoding | `Base/Internal/User/Services/SignatureService.cs:49-54` |
| Signed content is the full query string built so far, excluding `signature` | `Base/Shared/HttpExtensions/HttpRequestSignatureExtensions.cs:31-44` |
| `timestamp` is the **synced server time**, not the local clock | same, 35 |
| Listen key fetch and keep-alive both issue **`POST`** — correct: a `POST` on an account with an active key returns it and extends validity 60 minutes. The class doc comment claiming "periodic PUT" is what is wrong | `Base/Internal/User/Services/ListenKeyResolver.cs:145` |

**~~[CONTESTED] — the validity period is recorded twice~~ — settled the same day, 2026-09-18.** The
manifest said 60 minutes; `ListenKeyResolver.cs:220` said in a code comment that the period is half an
hour. The documentation says **60 minutes**, three times over
(`…/usd-futures/user-data-streams.md:6,7,11`), so the manifest was right and the code comment is wrong.
Nothing in the tree could have distinguished them — the keep-alive fires every 60 s, far inside either
window — which is why it took a document rather than a test.

**And the `POST`-only design is better than merely acceptable.** The same page documents `PUT` as the
keep-alive and `POST` as the start, and both extend validity by 60 minutes; but it also says a `PUT`
can answer `-1125` "This listenKey does not exist", after which you must `POST` to recreate. A
`POST`-only resolver cannot reach that state: on an account with an active key, `POST` returns that key
and extends it. So the class doc comment promising "periodic PUT keep-alive confirmations" is wrong
about what the code does, and the code is right about what the exchange wants.

Composition of the user-stream URI is `confirmed` from the same page: base
`wss://fstream.binance.com/private`, path `/ws/<listenKey>` (`…/user-data-streams.md:13-16`), which is
the `/private/ws/` this module composes.

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

**~~[DEAD]~~ — the whole spot user path, and further than this said. Revived and validated live 2026-09-21; the paragraph below is kept as the record of what was dead and how far it reached.**
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

**And that is what happened, on 2026-09-21.** The factory builds a connector with an account stream —
a different transport from the one this paragraph assumed, because the mechanism it named was retired
(§11) — the connector issues placement, cancel-replace, cancel and cancel-all
(`Spot/Internal/User/UserConnector.cs:232,279,314,367`), and `UserProvider` issues real requests on
every load (`:72,104,132,146`). Each of the rows this paragraph listed as unreachable is reachable, and
each was read from a live answer: the read block, the trading block, and on the same day the first
fills. What the paragraph predicted — that reviving the path revives every one of them at once — is
exactly how it went, which is the reason it is kept rather than deleted.

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

## Spot step-3 rules pinned on 2026-09-21

Recorded as their own entries because a field list has no slot for them, and every one of them was a
rule the code carried and no test named.

| rule | where | state |
|---|---|---|
| an executed price is zero when nothing filled, rather than a division | spot get-order, init-order and order-update converters | `pinned`, mutation-checked on all three. Only the dividing arm had ever run, and nothing filled is what a new order looks like |
| a trade with no order id is dropped | spot get-trade converter | `pinned`, mutation-checked |
| a trade with no symbol or no commission asset is dropped | spot get-trade converter | **enforced by the compiler**: the model takes non-nullable strings, so removing either check fails the build. Documented by a test; a mutation of it does not exist to run |
| an order update with no order id is dropped | spot order-update converter | `pinned`, mutation-checked. Only the wrong-event-tag half of the same condition had been driven |
| an exchange info answer carrying one collection and not the other is dropped | spot exchange-info converter | **enforced by the compiler**, same shape as above; both arms now driven independently by a test, where one fixture used to satisfy both at once |
| a quote with no price on either side is dropped | shared instrument-ticker converter, both venues | `pinned`, mutation-checked. The only test satisfied both halves of the condition at once, so the price half survived every mutation |

Nine facts, six killed by a mutation and three held by the type system. The three are worth naming
rather than counting as untested: a check the compiler needs cannot be quietly deleted, which is a
stronger guarantee than a test and a weaker one than a test plus the compiler.

## Spot step-4 facts pinned on 2026-09-21 — the market half

| fact | state |
|---|---|
| the candle request asks `api/v3/klines` and carries `symbol`, `interval`, `limit` and `startTime` | `pinned`, each of the five mutation-checked. Only the cursor had been asserted, because the paging test needed it and nothing else needed anything |
| the exchange info request asks `api/v3/exchangeInfo` | `pinned`, mutation-checked. Offline the test server answers whatever it is asked, so the one string that must match the venue was the one nothing checked - and this venue has already lost a live run to exactly that |
| a refused candle load yields a batch whose status is not `Ok`; a window the venue answers with no candles yields **no batch at all** | `pinned`. The two are distinguishable, which is the property that matters - though not in the shape first assumed. Worth stating because "empty answer" and "empty batch" are different things on this path |

The refusal half has no local mutation: the discarding it guards against lived upstream and is fixed
there, so there is nothing in this repository to break. The test is a regression guard against that fix
being undone, which is what it is for.

## §10 — spot user read endpoints, collected 2026-09-21

Collected because §9 records the spot user path as `[DEAD]`, so step 1 had no `file:line` to anchor and
step 2 had nothing to check: the response *shapes* were `confirmed` at tier 1 all along, and the
**endpoints that return them were in no row of this document**. Reviving the path needs them, and a
plausible reading of the futures venue is not a source — the two diverge on the facts that matter most
here.

Source: the stored snapshot `2026.09/2026.09.18-docs/spot/rest-api.md`, tier 1. Every row cites it.

| endpoint | weight | required | optional | citation |
|---|---|---|---|---|
| `GET /api/v3/account` | 20 | `timestamp` | `omitZeroBalances`, `recvWindow` | `rest-api.md:4174-4190` |
| `GET /api/v3/openOrders` | **6 with a symbol, 80 without** | `timestamp` | `symbol`, `recvWindow` | `rest-api.md:4287-4305` |
| `GET /api/v3/allOrders` | 20 | `symbol`, `timestamp` | `orderId`, `startTime`, `endTime`, `limit`, `recvWindow` | `rest-api.md:4339-4367` |
| `GET /api/v3/myTrades` | **20 without `orderId`, 5 with** | `symbol`, `timestamp` | `orderId`, `startTime`, `endTime`, `fromId`, `limit`, `recvWindow` | `rest-api.md:4570-4608` |

### The three facts that diverge from futures **[DIVERGES]**

Named separately because each is a trap for anyone porting the neighbouring venue's provider, which is
the obvious and wrong way to build this.

1. **The history window is capped at 24 hours**, on both `allOrders` and `myTrades` — "the time between
   `startTime` and `endTime` can't be longer than 24 hours" (`rest-api.md:4367`, `:4600`). Futures pages
   in seven-day windows. A loop written for seven days asks spot for a window it refuses.
2. **`openOrders` costs 80 without a symbol**, against 40 on futures. A one-second reload of an
   unscoped open-order list is 4800 a minute against a 6000 ceiling — the whole allowance for one list.
3. **`myTrades` returns the most recent trades when `fromId` is absent** (`rest-api.md:4598-4599`),
   where the futures trade endpoint's page cap selects the **oldest**. The same parameter name, the
   opposite end. `undocumented` on futures and `confirmed` here, which is exactly backwards from how it
   feels.

### Cursor semantics

- `allOrders` with `orderId` returns orders **>= that id**; without it, the most recent
  (`rest-api.md:4364-4365`).
- `myTrades` with `fromId` returns trades **>= that id**; without it, the most recent
  (`rest-api.md:4598-4599`).
- `limit` on both: default 500, maximum 1000.

Verification axis: everything in this section is `confirmed` on the documentation axis and `none` on
the verification axis until the read paths that call it exist and are pinned.

### §10 verification — the live read run of 2026-09-21

All four endpoints called against the real account, 60 responses, every one `200 OK`, no venue error
code and no warning. So every row in §10 moves from `none` to **`live` (2026-09-21)** on the
verification axis: the paths, the parameter spellings, the signing, and the 24-hour window - the two
history walks made 21 and 22 requests, which is the windowing running rather than being described.

**What the live assertions do not prove, and this is the census correction.** Four of the six live read
tests assert only that the status is `Ok` and the data is not null. **Those would have passed against
the stub**, which returned an empty success without issuing a request - so they never distinguished a
working path from an absent one, and reading them as coverage is what let the stub sit behind a
`gated` marking for as long as it did. The one exception is the account test, which requires at least
one balance and therefore could not pass against nothing.

What pins the behaviour is the offline suite, where the request shape, the window walk and the refusal
are asserted and mutation-checked. The live run answers a different and narrower question - whether the
venue accepts what we send - and it is recorded here as answering exactly that.

## §11 — spot order lifecycle and user stream, collected 2026-09-21

Collected for the same reason as §10 and in the same way: the spot connector **was** a stub when this
section was opened, so step 1 had no `file:line` for any of it, and none of these endpoints appeared in
a row of this document. It is not one since later the same day - every row below now has code behind it
and an answer read from the venue. Step 5 needs
them, and the futures connector is not a source — **every one of the four divergences below would be
wrong if ported**, and two of them silently.

Source for the REST rows: `2026.09/2026.09.18-docs/spot/rest-api.md`, tier 1. Source for the stream
rows: `2026.09/2026.09.18-docs/spot/web-socket-api.md` and `spot/user-data-stream.md`, same commit, the
first added to the snapshot on 2026-09-21 — see that snapshot's `SOURCES.md`.

### Order lifecycle over REST

| endpoint | weight | unfilled-order count | required | citation |
|---|---|---|---|---|
| `POST /api/v3/order` | 1 | 1 | `symbol`, `side`, `type`, `timestamp`, plus a per-type set | `rest-api.md:2128` |
| `DELETE /api/v3/order` | 1 | 0 | `symbol`, `timestamp`, and **one of** `orderId` / `origClientOrderId` | `rest-api.md:2380` |
| `DELETE /api/v3/openOrders` | 1 | 0 | `symbol`, `timestamp` | `rest-api.md:2453` |
| `PUT /api/v3/order/amend/keepPriority` | 4 | 0 | `symbol`, `newQty`, `timestamp`, and one of `orderId` / `origClientOrderId` | `rest-api.md:2993` |
| `POST /api/v3/order/cancelReplace` | 1 | 1 | `symbol`, `side`, `type`, `cancelReplaceMode`, `timestamp`, one of `cancelOrderId` / `cancelOrigClientOrderId` | `rest-api.md:2578` |

`DELETE /api/v3/openOrders` requires a symbol; there is no unscoped cancel-all.

### The four facts that diverge from futures **[DIVERGES]**

1. **There is no general amend.** Spot's amend endpoint *reduces quantity only*: "Reduce the quantity of
   an existing open order", and `newQty` "must be greater than 0 and less than the order's quantity"
   (`rest-api.md:2993`, `:3018`). A price change, or any quantity increase, is not an amendment on this
   venue — it is `cancelReplace`, which is a new order at the back of the queue and costs an unfilled-order
   count. Futures amends price and quantity through one endpoint. **A ported `ModifyOrderAsync` would
   refuse half the modifications it is asked for, or silently reprice by replacing.**
2. **Placement is counted against a second, separate limit.** `POST /api/v3/order` costs weight 1 and
   **unfilled order count 1** — a budget distinct from request weight, which the module's rate limiter
   does not model at all. `cancelReplace` charges it even when the new order was never attempted
   (`rest-api.md:2585`). Cancels charge 0.
3. **The user stream is a different transport**, not a different URL — see below.
4. **The stream event is wrapped.** Every spot user event now arrives as
   `{"subscriptionId": <int>, "event": {…}}` (`web-socket-api.md:337-366`, and every payload in
   `user-data-stream.md`), where the futures stream delivers the event object bare.

### The user data stream — listen keys are gone **[DRIFT]** `contested`

| fact | state | citation |
|---|---|---|
| Listen-key user streams on `wss://stream.binance.com` are **deprecated** | `confirmed` | `CHANGELOG.md:953` |
| All listen-key documentation for that endpoint has been **removed** | `confirmed` | `CHANGELOG.md:594` |
| `POST /api/v3/userDataStream` and siblings are no longer in the REST reference | `confirmed` — the string `userDataStream` has **0 occurrences** in `rest-api.md`, and `listenKey` 0 likewise | `rest-api.md` (census) |
| The features "remain available until a future retirement announcement" | `confirmed` | `CHANGELOG.md:600` |

Marked `contested` rather than settled because the two readings genuinely disagree and neither is
stale: the changelog says the mechanism still works, and the reference no longer describes it. Nothing
in the snapshot says when it stops. What would settle it is a retirement announcement, or a live
attempt — and a live attempt answers only "today".

**What this means for our code.** `Spot/Constants.cs:50`, `Spot/ProviderConfiguration.cs:11`,
`Spot/Internal/User/Contracts/UserContracts.cs:61-64`, `Spot/ProviderRegistrationContextExtensions.cs:76`
and `Spot/Internal/User/Profiles/UserConfigProfile.cs:37-38` all configure the listen-key mechanism, and
`Base/Internal/User/Services/{ListenKeyResolver,UserStream}.cs` implement it. **Nothing in spot resolves
any of it** — `Spot/Internal/User/UserConnector.cs` builds no stream at all — so today this is registered,
unused machinery for a mechanism the venue has stopped documenting. The cost of not noticing was zero
only because the connector was never written.

### The replacement mechanism

Base endpoint **`wss://ws-api.binance.com:443/ws-api/v3`** (`web-socket-api.md:102`) — a
request/response WebSocket API, and **not** the market-stream endpoint this module already connects to.
A connection is valid for 24 hours (`:105`); the server pings every 20s and disconnects if no pong
arrives within a minute (`:111-114`).

Two routes to a subscription (`web-socket-api.md:8174-8182`):

| route | method | weight | key type | params |
|---|---|---|---|---|
| authenticated session | `session.logon`, then `userDataStream.subscribe` | 2 | **Ed25519 only** (`:1299`) | none on subscribe |
| per-request signature | `userDataStream.subscribe.signature` | 2 | HMAC, RSA or Ed25519 (`:107`) | `apiKey`, `timestamp`, `signature`, optional `recvWindow` |

Both answer `{"result": {"subscriptionId": <int>}}`. `userDataStream.unsubscribe` takes an optional
`subscriptionId` and closes all subscriptions when given none (`:8236-8262`). Limits: one subscription
per account per connection; 1,000 active and 65,535 lifetime per session (`:8185-8191`).

**The second route is the one that matters for this module**, because it does not require Ed25519 — the
account's existing HMAC key signs it, the same key §10's read paths already use. Choosing the first
route would make the stream depend on a key type the account may not have, which is a configuration
change on the user's side rather than a code change on ours.

`eventStreamTerminated` is sent when a logon subscription ends after `session.logout`, or when the
subscription is stopped (`user-data-stream.md:325-343`). It is the signal a reconnect keys on, and it
has no counterpart in the listen-key mechanism, where expiry was inferred from a closed socket.

### §11 verification — pinned 2026-09-21

Everything below is `pinned`: an offline test fails if it changes, and each one was mutation-checked.
Nothing here is `live` — this venue's trading block does not exist, so no fact in §11 has been observed
against the exchange.

| fact | what pins it |
|---|---|
| the signature payload is the parameters **sorted by name** | the venue's own worked example, reproduced exactly (`WsApiRequestBuilderTests`) |
| values go into the payload **raw**, encoded UTF-8 | the venue's second worked example, whose symbol is non-ASCII |
| a number is a JSON number and text a JSON string in the frame | `ANumberIsANumber_AndTextIsAString` |
| a method taking no parameters sends **no** `params` member | `AMethodWithNoParameters_SendsNoParamsMember` |
| the subscription method is the **signature** variant | `WsApiUserStreamTests.Connected_SendsASignedSubscription` |
| the event envelope is `{subscriptionId, event}` and the event is what a consumer gets | `AnEvent_ArrivesUnwrappedFromItsEnvelope` |
| `eventStreamTerminated` ends a subscription without ending the connection | `ATerminatedStream_IsSubscribedAgainOnTheSameConnection` |
| `POST /api/v3/order`, `POST /api/v3/order/cancelReplace`, `DELETE /api/v3/order`, `DELETE /api/v3/openOrders` | `UserConnectorCommandTests.EachCommand_GoesToItsDocumentedEndpoint`, a theory over all four |
| `cancelReplaceMode=STOP_ON_FAILURE` on a replacement | `Modify_StopsOnAFailedCancelRatherThanReplacingRegardless` |
| a spot account has no leverage to set | `SetLeverage_IsRefusedAndSendsNothing` |

**The two golden signatures are worth their length and one of them is worth more than the other.**
Percent-encoding every value passes the ASCII example and fails only the non-ASCII one — measured, by
mutating the builder to escape its values. A contract pinned only by the first example would be wrong
about every symbol outside ASCII and green everywhere.

**The weight of the subscription is not counted.** It is 2, on connect and on each retry, against a
6000/min ceiling; the limiter's model is to report what a response header said, and this transport has
no header. Recorded rather than plumbed through — see the backlog in `status.md`.

### §11 live — measured 2026-09-21, four facts the documentation did not give

The first live run of this venue's trading block. Every row below is `live` on the verification axis and
was measured, not read.

| fact | what it means for our code |
|---|---|
| the WebSocket API subscription is **accepted with an HMAC key**, over `userDataStream.subscribe.signature` | the whole transport works; the route that needs an Ed25519 key was never touched |
| `DELETE /api/v3/openOrders` answers **the array of orders it cancelled** — not a `{code,msg}` envelope **[DIVERGES]** | the connector read it as the envelope, so every successful cancellation reached the caller as a parse failure. The cancellation had already happened |
| the same endpoint is **refused** on a symbol with nothing open, under the code its reference calls `CANCEL_REJECTED` with the message "Unknown order sent." | cancel-all is **not idempotent** here. And the code cannot be folded into success: the same one carries "Market is closed." and "This account may not place or cancel orders." — the reason is in the message, not the code |
| `PERCENT_PRICE_BY_SIDE` bounds how far from the market a limit order may be priced, against an **average of recent trades** rather than the current bid | `InstrumentModel` does not carry the bound, so no caller can compute it. Measured for one instrument: a `BUY` floor at half the reference, with a five-minute averaging window |

**The last one was the gap worth naming, and it is closed as of 2026-09-21.** The provider read the
price, lot, notional and order-count filters into `InstrumentModel` and dropped this one, so a caller
pricing an order away from the market had no way to know how far it could go — and the refusal names a
filter rather than a bound. `InstrumentModel` now carries the band as four ratios of the venue's
reference price, on both market types; see §4 for the two spellings and why the futures one must not be
read as a two-sided band.

### §11 live — lifetime and recovery, measured 2026-09-21

| fact | how |
|---|---|
| a connection **survives past the venue's keep-alive deadline** — it pings every 20s and closes what does not answer within a minute | held for 150s, never left connected. The answer is the framework's rather than ours, which was the reasoning before and is now the observation |
| a cut connection is **noticed, re-established, and subscribed again** — the venue accepts a second signed subscription on a new connection | cut on purpose through a byte relay, which is the only way to make a venue drop one |

The relay is not a WebSocket proxy, deliberately: one that understood the protocol would answer the
venue's ping itself, and the long-connection property would be masked by the instrument measuring it.

**Still unmeasured on this axis:** the 24-hour session limit the venue documents. It cannot be reached by
a test suite, and what stands in for it is the reconnect path above, which is now measured.

### §11 live — execution, measured 2026-09-21

The first fills on this market type. Two real round trips on `DOTUSDT`, one at market and one with a
limit priced through the book, captured in
[`2026.09/2026.09.21-spot-fill/`](2026.09.21-spot-fill/README.md) — every row below is `live` and was
read from those bytes.

| fact | what it means for our code |
|---|---|
| an order raises **two events**: `x:"NEW"`/`X:"NEW"` on acceptance, then `x:"TRADE"`/`X:"FILLED"` carrying the execution | the fill is its own event, so ingestion keyed on the first one sees an order that was accepted and never filled. Ours keys on the status and reloads trades on `PartiallyFilled` or `Filled` |
| the acceptance event carries **`"N":null`** and **`"n":"0"`** | a null commission asset, and an amount spelled bare rather than padded. Every fixture before this capture spelled both otherwise, so neither had been read. Now pinned by `OrderUpdateEventConverterTests` |
| a **marketable limit fills at the book's price**, not at its own — placed at `1.202`, filled at `1.19700000` | `price` is what was asked for and `cummulativeQuoteQty / executedQty` is what it cost. The three converters that compute the quotient are right, and this is the payload that can tell them from one reading `price` |
| the **fee's asset depends on the side**: the base asset on a buy (`"N":"DOT"`), the quote asset on a sell (`"N":"USDT"`) | a buy credits less of the asset than it filled, so selling back the filled quantity is refused for insufficient balance. Anything returning a position has to subtract the fee and re-align to the lot step |
| **`outboundAccountPosition` carries only the balances that changed** — three assets, on an account holding five **[DIVERGES from its own name]** | it is a delta, not a snapshot. Our connector reads none of its balances and treats the event as a signal to reload the account, which is what makes this harmless. Publishing from it directly would erase every balance it does not mention |
| the stream's `executionReport` and the placement's `fills` **agree** on price, quantity, fee and trade id | either could be a trade's source, so the choice is ours to make once. The connector uses neither and asks for the symbol's trades, so a fill reaches a caller exactly once |

**Not settled by this capture:** whether the executed price is the average over several fills rather
than the last one. Both round trips filled in a single trade, where the two readings coincide. The
distinction is pinned offline instead, by a fixture whose cumulative and last prices differ.

**One registration is deliberately unpinned and says so.** `CancelAllOrders`'s serializer carries the
element converter so the array parses truthfully rather than by accidental name binding, but the
connector discards the payload — so removing the converter changes nothing a test can see. Mutation
confirmed it survives. What *is* pinned is the response **type**: reading the array as an envelope kills
the test.
