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
- docs revision: spot `a0057759f1cbcab812af44b75309d72866a57561`; futures fetched 2026-09-01 (no
  repository exists, so the date is the only anchor)
- working branch: `main` where converged
- last reconciled: 2026-09-18 — step 5 under way on `skill/provider-connector`: 5a and 5b done, four
  defects in the stream services found and fixed

## Convergence

| step | state | evidence | outstanding |
|---|---|---|---|
| 1 — derive existing state | **converged** | ~70 anchors verified and re-anchored after the environment removal; four missing entries added; both axes censused entry by entry against the test suites | none |
| 2 — collect facts, compute drift | **converged, with two accepted gaps** | all 13 futures pages and 7 spot files snapshotted; request side closed at tier 1 from the official Postman collections; every category given a documentation outcome | **accepted, not open**: the nested user-data-stream payloads (~20 short field names) are `unretrievable` — no available technique reaches them, so waiting changes nothing; and the `avgPrice` question is `contested`, settleable only by a live order. Both are recorded against their entries rather than left as unfinished work |
| 3 — wire types and serialization | **converged** | assessed 2026-09-17 against the manifest by a fresh verifier: field coverage holds in both directions (every documented field read, no field read that is not documented), all five enumeration tables map both ways, the kline indices are pinned by six distinct values. Two branch gaps and one defect remediated the same day — see the run report | none |
| 4 — provider, read paths (+ registration, config, read-only live validation) | **converged** | every read path on both venues driven offline, failure paths included; endpoints pinned; **live read block green on 2026-09-16: 20 tests, 14 passed, 6 skipped, none failed** — and this time with credentials present, so the two signature tests ran rather than skipping | none. The upstream defect found here — an exchange error discarded when the success type is a collection — was fixed in `Annium.Net.Http` 1.1.49 and taken up with the package bump; the test that pinned the loss now pins the reason. The six still skipped are Spot's `UserProviderTests`, marked `Not implemented`, which is about the tests and not about access |
| 5 — connector, streams and orders (+ registration, config, trading live validation) | **in progress — 5a and 5b done** | the test lib gained a websocket server (`TestBaseWebSocketServerExtensions`), and the streams are pinned offline by 20 tests across `BookTickerServiceTests`, `ListenKeyResolverTests` and `UserStreamTests`. Four defects found and fixed in the process — see the [run report](2026.09/2026.09.18-step-5.md) | 5c (order lifecycle offline), 5d (registration and config), 5e (live read-only), 5f (live trading, staged) |
## Queued work

Open items only. Two things that used to live here are settled and their reasoning is where it belongs
now: the signing property is pinned by `HttpRequestSignatureExtensionsTests` (the golden value could never
have seen it — it signs a literal, so no query is composed), and the read-side enumeration gaps are closed
by `WireMappingTests` on both venues. Rate-limit handling in the runtime shipped in 1.3.0–1.3.4. The
manifest carries what each of them now defends; the run reports beside this file carry how it went.

| left | why it is still open |
|---|---|
| the history paging fixture is `vacuous` | it asks for one day while claiming a seven-day window and a three-month cap. The *fact* is `pinned` regardless — an offline test drives twenty days through three windows — so this is a misleading test rather than an unguarded fact. Deleting it may be the right fix |
| `x-mbx-used-weight-1m header not present` at `Error` (`Shared/HttpExtensions/HttpRequestRateExtensions.cs:75`) | every response without the header logs one, and Binance does not send it everywhere — a refusal answers without it. Either the absence is normal on those paths and the level is wrong, or the limiter is running blind there. Needs a census of paths against their response headers, and step 5 has to answer the same question for `x-mbx-order-count-*`, so it is cheaper done there |
| `MapOperationCode` folds every negative code to `BadRequest` | an invalid key, an expired timestamp and a malformed parameter are indistinguishable to a caller, and the HTTP status — which would have told `Forbidden` from `BadRequest` — is consulted only where the error body parsed as success. The useful split is by what a caller would do differently — retry, re-sign, stop — which is a decision about the runtime rather than a mapping table |
| decay constants `none` | the ceiling and the water-mark fraction are pinned through the number they compose to; the decay rate and interval are not |
| three `[UNVERIFIED]` markers in the manifest | leftovers from the vocabulary this manifest replaced. Two are substantive: the rate-limit window is *assumed* to be one minute, and a one-way account is *assumed* to report one `positions[]` row per symbol with `positionSide=BOTH` — the write fixture's precondition rests on the second. Both belong to the documentation axis, so step 2 assigns them |
| no test file at all: `HttpRequestLogExtensions`, the filter converters | the filter converters are covered through the exchange-info fixture, which the manifest says plainly — a missing file, not a missing fact. `WebSocketService` and `ListenKeyResolver` left this list on 2026-09-18: both are pinned offline now |

**Step 5 is the work, not this list.** Its offline stream half is done as of 2026-09-18; the order
lifecycle, registration and the live stages are what remains.

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
