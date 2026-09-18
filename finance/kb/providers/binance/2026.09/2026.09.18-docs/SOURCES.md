# Snapshot sources — 2026-09-18

What this run read. The report beside it was written from these files, not from the live sites.

The headline of this snapshot is not a file but a discovery: **`llms-full.txt` exists**, and it closes
the gap the 2026-09-01 run recorded as unretrievable. See "The catalog, and how it was finally read".

## spot — tier 1 (upstream git repository)

Repository `github.com/binance/binance-spot-api-docs`, pinned at
**`828ca74b809cfedbd5602df328b5f706368d483b`** (master, 2026-09-18). Previous run:
`a0057759f1cbcab812af44b75309d72866a57561`.

Fetched with `curl -sSL https://raw.githubusercontent.com/binance/binance-spot-api-docs/<sha>/<path>`.

| file | bytes | vs 2026-09-01 |
|---|---|---|
| `spot/CHANGELOG.md` | 132622 | **changed** (+23 lines) |
| `spot/enums.md` | 5244 | identical |
| `spot/errors.md` | 20321 | identical |
| `spot/filters.md` | 13528 | identical |
| `spot/rest-api.md` | 181189 | identical |
| `spot/user-data-stream.md` | 13170 | identical |
| `spot/web-socket-streams.md` | 22958 | identical |

Six of seven byte-identical across seventeen days, and the seventh changed only in SBE and FIX
retirement notices — neither of which this module touches. Spot is quiet.

## usd-futures — tier 2 (docs site, markdown source)

Base `https://developers.binance.com/en/docs/products/derivatives-trading-usds-futures/`, fetched by
appending `.md` to the page path. No upstream repository exists for the derivatives documentation, so
no revision can be pinned; the fetch date is the only anchor, which is precisely why the snapshot is
kept.

All 13 pages in the product section, same list as the previous run:

| file | bytes | vs 2026-09-01 |
|---|---|---|
| `usd-futures/change-log.md` | 108462 | **changed** (+29 lines) |
| `usd-futures/error-code.md` | 19771 | **changed** (+4 lines: `-5047`) |
| `usd-futures/general-info.md` | 19261 | identical |
| `usd-futures/user-data-streams.md` | 11936 | identical |
| `usd-futures/common-definition.md` | 6399 | identical |
| `usd-futures/faq_stp-faq.md` | 15341 | identical |
| `usd-futures/websocket-api-general-info.md` | 13414 | identical |
| `usd-futures/websocket-market-streams_Important-WebSocket-Change-Notice.md` | 4303 | identical |
| `usd-futures/websocket-market-streams_Live-Subscribing-Unsubscribing-to-streams.md` | 4683 | identical |
| `usd-futures/websocket-market-streams_Connect.md` | 2613 | changed (formatting) |
| `usd-futures/quick-start.md` | 2032 | changed (formatting) |
| `usd-futures/websocket-market-streams_How-to-manage-a-local-order-book-correctly.md` | 1114 | changed (formatting) |
| `usd-futures/Introduction.md` | 110 | changed (formatting) |

Both retrieval checks were run and both passed: no response began `<!doctype html>`, and no two files
came back with the same byte count.

## One page reached by following a link out of the changelog

`usd-futures/coin-futures_Important-CM-UM-Integration-Notice.md`, 12109 bytes, from
`https://developers.binance.com/en/docs/products/derivatives-trading-coin-futures/Important-CM-UM-Integration-Notice.md`.

It lives under the **COIN-M** product, not USDⓈ-M, and is reachable from the USDⓈ-M changelog only
through one link inside the 2026-06-10 entry. It carries changes to `POST /fapi/v1/order`,
`PUT /fapi/v1/order`, `DELETE /fapi/v1/order` and `GET /fapi/v1/klines` — all of them ours — and it is
where the `avgPrice` question was finally settled.

Second time this exact shape has produced this run's most consequential material, after the WebSocket
migration notice on the previous run. The rule holds: **follow the links out of the changelog**, and do
not assume a page filed under another product is about another product.

## usd-futures per-endpoint reference — the gap, now closed

`usd-futures/api-reference.md`, 34687 bytes. Extracted from
**`https://developers.binance.com/en/docs/llms-full.txt`** — 8229421 bytes, sha256
`aed9aa2c739cea2452f6b00263563f5009c6a64f6ad81d84a32f4e87dc3490e9`, fetched 2026-09-18 — lines
204286-205569, being the three USDⓈ-M sections of its "API Reference" chapter: REST API, WebSocket API
and WebSocket Market Streams.

This is the material the 2026-09-01 run recorded as unreachable after trying five catalog path shapes.
It was reachable the whole time, in a file nobody had looked for.

## The catalog, and how it was finally read

The rendered catalog pages still cannot be fetched. Every form tried on this run returned the same
65475-byte HTML shell with a `200`:

- `/catalog/core-trading-derivatives-trading-usd-s-m-futures/api/rest-api.md`
- `…/api/rest-api/~schemas.md`
- `…/api/rest-api/new-algo-order.md`

and the fragment form `…/api/rest-api#new-algo-order` returned the empty `202` the site gives a plain
page fetch. So the previous run's conclusion about *those URLs* was right.

What it missed is that the site publishes two machine-readable indexes for language models:

| file | bytes | what it is |
|---|---|---|
| `llms.txt` | 172557 | the page index — every documentation path, plus an "API Reference" chapter listing **every endpoint** by method and path |
| `llms-full.txt` | 8229421 | the documentation set as one text file: 833 documents, plus the API reference with per-endpoint descriptions |

`llms.txt` alone would have answered this run's blocking question in one fetch: it lists
`POST /fapi/v1/algoOrder` under "Futures (USDⓈ-M) REST API". The previous run knew `llms.txt` existed
— its own notes name it as where the 13 page paths came from — and read it as a page index only. The
"API Reference" chapter is 450 lines further down the same file.

**`llms.txt` is stored in this snapshot** so the next run diffs the endpoint inventory rather than
re-deriving it. An endpoint appearing or disappearing there is a contract change with no changelog
entry required.

`llms-full.txt` itself is **not** stored: 8.2 MB, overwhelmingly about products this module never
touches, and the skill's own rule is that volume nobody diffs is volume that hides the diff. What is
stored is the extract, its line range, and the hash of the file it came from — enough to reproduce.

## Request schemas — tier 1 (upstream git repository)

Repository `github.com/binance/binance-api-postman`, pinned at
**`367b396861b8debd5475444f3e0ce4caaf8dfe78`** (master, pushed 2026-09-16). Previous run:
`bf7c41820ddef7684a5b861c485791ade747e8a2`.

**The repository was restructured since.** The paths recorded by the 2026-09-01 run —
`usd-futures/postman-usds-futures.json` and `spot/postman-spot.json` — now return `404: Not Found`, a
14-byte body. Collections live under `collections/` with product names:

| file in this snapshot | upstream path | bytes |
|---|---|---|
| `usd-futures/postman-usds-futures.json` | `collections/Binance Derivatives Trading USDS Futures API.json` | 185030 |
| `spot/postman-spot.json` | `collections/Binance Spot API.json` | 171587 |

Local filenames are kept as they were so the diff against the previous snapshot works; the upstream
path is recorded here because that is the part that moved.

Worth noting for whoever reads this next: a 404 from `raw.githubusercontent.com` is a **14-byte
success**, not an error. `curl -sSL` writes it to the output file and exits 0. The size check caught
it here; nothing else would have.

The futures collection carries all six algo endpoints with full parameter lists, which is what closes
the request side of this run's blocking finding at tier 1.

Still true, and carried over: the collections hold **no response examples**, so they close the request
side and nothing else.

## Rejected as a source: the Binance MCP server

Unchanged from the previous run. `https://agent.binance.com/mcp/agentic` is an account-connected
agent, not a documentation server, and Binance's own page says "Never paste the MCP endpoint into an AI
chat and ask it to install the server." Wrong instrument, and one the vendor warns against installing
this way.

## Gaps remaining in this snapshot

**One, reduced from two.**

The nested payloads of the user-data-stream events are **still unretrievable**. `ORDER_TRADE_UPDATE`'s
order object, `ACCOUNT_UPDATE`'s balance and position entries, and now `ALGO_UPDATE`'s payload all
render through a `<SchemaEmbed>` component pointing at
`/catalog/core-trading-derivatives-trading-usd-s-m-futures/api/ws-streams/~schemas#<name>`. The
component is in the markdown; the schema it embeds is not, in either `llms.txt` or `llms-full.txt`.
So the ~20 short field names this module reads (`sp`, `ap`, `R`, `wb`, `cw`, `bc`, `pa`, `ep`, `up`, …)
remain verified against nothing.

The response-schema gap for futures REST is **closed** — `api-reference.md` carries the per-endpoint
material, where the 2026-09-01 run had only a tier-3 reading.
