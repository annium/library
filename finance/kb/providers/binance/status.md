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
- last reconciled: 2026-09-18 — step 1 re-run against the tree on `provider/binance-contract-reconcile`,
  opening the contract pass that 5f's fourth stage is blocked on. Before it: steps 5a through 5e merged;
  5f run live in staged gates, three stages of four green, the fourth stopped by contract drift; step 4's
  residue worked through, four of its six items closed

## Convergence

| step | state | evidence | outstanding |
|---|---|---|---|
| 1 — derive existing state | **converged, re-run 2026-09-18** | re-anchored against the tree after steps 3-5: of 107 line-anchored facts 76 resolved and 31 had moved, no file gone. 21 facts present in the code and absent from the manifest were added, three new `[DEAD]` entries recorded, and the verification census corrected in ~13 places. The manifest asserted the opposite of the code in one place — "Nothing reads `Retry-After`" — and contradicted itself in another, over the decay constants | none on this axis. The four test defects it turned up are queued below for step 5 |
| 2 — collect facts, compute drift | **converged, with two accepted gaps** | all 13 futures pages and 7 spot files snapshotted; request side closed at tier 1 from the official Postman collections; every category given a documentation outcome | **accepted, not open**: the nested user-data-stream payloads (~20 short field names) are `unretrievable` — no available technique reaches them, so waiting changes nothing; and the `avgPrice` question is `contested`, settleable only by a live order. Both are recorded against their entries rather than left as unfinished work |
| 3 — wire types and serialization | **converged** | assessed 2026-09-17 against the manifest by a fresh verifier: field coverage holds in both directions (every documented field read, no field read that is not documented), all five enumeration tables map both ways, the kline indices are pinned by six distinct values. Two branch gaps and one defect remediated the same day — see the run report | none |
| 4 — provider, read paths (+ registration, config, read-only live validation) | **converged** | every read path on both venues driven offline, failure paths included; endpoints pinned; **live read block green on 2026-09-16: 20 tests, 14 passed, 6 skipped, none failed** — and this time with credentials present, so the two signature tests ran rather than skipping | none. The upstream defect found here — an exchange error discarded when the success type is a collection — was fixed in `Annium.Net.Http` 1.1.49 and taken up with the package bump; the test that pinned the loss now pins the reason. The six still skipped are Spot's `UserProviderTests`, marked `Not implemented`, which is about the tests and not about access |
| 5 — connector, streams and orders (+ registration, config, trading live validation) | **in progress — 5a through 5e done, 5f three stages of four** | the test lib gained a websocket server; the streams are pinned by 20 offline tests, the order lifecycle by 13 more, and every registration key by two. Six defects found and fixed in the process — see the [run report](2026.09/2026.09.18-step-5.md) | 5f's fourth stage, which is blocked on a contract question rather than on code - see the row about trigger orders below. **5e validated live on 2026-09-18**: 21 tests, 15 passed, 6 skipped, none failed, run twice - including a new read-only user connector test that connects, takes a listen key, opens the stream and receives the account snapshot without placing anything. The two behaviours that needed a decision have one: a refused leverage change now reaches the caller, and one the exchange accepts without applying is refused by the connector itself; a limit order asked to become a market order stays refused |
## Queued work

Open items only. Two things that used to live here are settled and their reasoning is where it belongs
now: the signing property is pinned by `HttpRequestSignatureExtensionsTests` (the golden value could never
have seen it — it signs a literal, so no query is composed), and the read-side enumeration gaps are closed
by `WireMappingTests` on both venues. Rate-limit handling in the runtime shipped in 1.3.0–1.3.4. The
manifest carries what each of them now defends; the run reports beside this file carry how it went.

| left | where it stands, verified 2026-09-18 |
|---|---|
| **futures trigger orders are refused on the order endpoint** | observed live 2026-09-18, placing a real `STOP_MARKET` through `POST /fapi/v1/order`: *"Order type not supported for this endpoint. Please use the Algo Order API endpoints instead."* The four futures trigger types are now `contested` in the manifest. **This is steps 1-2 work, not step 5's**: which endpoint accepts them, what it takes and what it answers has to be re-derived from documentation, and nothing is amended in code until it is. It blocks 5f's fourth stage and nothing else - the position open/close path itself was proven live the same day |
| the `avgPrice` question waits on one live run | the assertion is in place since 2026-09-18 - every filled order now checks its executed price is above zero (`Tests.Lib/User/UserConnectorTestBase.cs`) - and it has not been through a live trading stage yet. The next approved stage 4 settles the manifest's `contested` row either way |
| `MapOperationCode` folds negative codes to `BadRequest` | **partly true, and deliberately left alone until the documentation pass** (decided 2026-09-18). The user variant carves out `-2018`/`-2019` as `InsufficientBalance` (`HttpRequestUserResultExtensions.cs:95-96`); the market variant does not even do that. The HTTP status is consulted only for *synthetic* errors (network, abort, parse), so a body that parsed as a Binance error ignores it and `-2015` (invalid key) reaches a caller as `BadRequest` rather than `Forbidden` - although both the code and the status said "access". The useful split is by what a caller would do differently - retry, re-sync and retry, reduce size, stop and fix configuration, accept a market refusal - and writing that table needs the real list of codes from the documentation, which the Algo Order API pass reopens anyway. Doing it blind would be inventing a taxonomy and then shipping it to every caller, `crypted`'s order path included |
| three `[UNVERIFIED]` markers in the manifest | confirmed, at `manifest.md:276`, `:321`, `:452`. The first sits on the rate-limit window being assumed one minute, the second on a one-way account reporting one `positions[]` row per symbol with `positionSide=BOTH` - which the write fixture's precondition rests on - and the third on the arithmetic that the shared decay implies 6000/min and so contradicts the futures ceiling of 2400. All three are documentation-axis questions and belong with the Algo Order API pass, which reopens the documentation anyway. The third's *verification* half is closed as of 2026-09-18 - the registered decay is pinned now - which leaves only the question of whether those numbers are the right ones |
| no test file at all: `HttpRequestLogExtensions`, the filter converters | the filter converters are covered through the exchange-info fixture, which the manifest says plainly - a missing file, not a missing fact. `WebSocketService` and `ListenKeyResolver` left this list on 2026-09-18: both are pinned offline now |
| **three test defects, found by step 1 on 2026-09-18 — fix on step 5** | decided the day they were found: they are defects in tests, not in the contract, and step 1-2 specifies rather than repairs. (1) `Tests.Lib/User/UserConnectorReadTestBase.cs:82` asserts `positions.Count.IsGreaterOrEqual(0)` — a count is never negative, so the line cannot fail, and it is the only thing covering positions in the read suite. (2) `WireMappingTests.SmallTables_MapEveryMemberBothWays` on both venues is tautological: `StringToValue` is built as `ValueToString.ToDictionary(x => x.Value, x => x.Key)`, so renaming `"BUY"` to `"buy"` passes it in full. (3) `MarketProviderReadPathTests.LoadContext_GuessesAPrecisionForAssetsTheInstrumentsDoNotDescribe` asserts `8` in both of its cases and never reaches the `Contains("USD") → 2` branch its own summary claims to cover. All three are recorded against their entries in the manifest as well, so a reader meeting the fact meets the weakness with it |
| **the listen key's facts are carried by no test**, found the same day | the resolver has 6 offline tests and every exchange fact inside it is synthetic: a `/listenKey` stub endpoint, 50 ms in place of the `60_000`/`5_000` cadence, no assertion on the HTTP method, and a rate limiter that records nothing — so deleting `.WithRateDelay1M(...)` breaks nothing. The component is covered; the contract it carries is not. Step 5 work, and the cheapest of the four to close is the method |

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
| 2026-09-18 | 1 — derive existing state | *(no report; step 2's report covers the run)* | re-anchored against the tree: 31 of 107 anchors had moved, 21 facts were missing, 3 new `[DEAD]`. **One entry asserted the opposite of the code** (`Retry-After`), and the decay constants were recorded as both `none` and `pinned` in the same document. Four test defects found and queued for step 5. Nothing compared against Binance yet — every fact stays on whatever documentation state it already had |
