---
name: implement-provider-connector
description: Step 5 of implement-provider — the connector: streams, the order lifecycle, registration and configuration, ending in live validation that places real orders on a real account. Runs in gated stages, each approved separately. Use as step 5 of implement-provider, or standalone when the user says "доведи коннектор <exchange>", "ступень 5", "implement the <exchange> connector", or asks for the streams or the order lifecycle to be finished.
user-invocable: true
---

# Implement Provider — Connector

Step 5. The connector is where a provider stops being a reader and starts being a participant: it holds
sockets open, keeps a key alive, and sends instructions that cost money. Everything before it could be
re-run at will. This step cannot.

**This is a draft written before the step was first run.** That is deliberate and it is the rule: the
skill is written first and the work goes through it, so that what is left behind describes what was
actually done rather than what someone remembered a week later. Where the work contradicts this document,
**correct the document first**, then continue from the corrected version. The correction is the finding.

## Why this exists

The step has two halves that fail in opposite ways.

The **offline half** is ordinary work that nobody has done: as of this draft not a single line of
`WebSocketService`, `BookTickerService`, `ListenKeyResolver` or `UserStream` is executed by any offline
test. They are reached only by live tests that assert a ticker arrived — which says nothing about
reconnect, resubscribe, unsubscribe, teardown or error reporting. Four classes carry the connection
lifecycle of every stream this module runs, and they are pinned by nothing.

The **live half** places orders. Its failure mode is not a red test: it is an order left open on a real
account, a position closed that someone else opened, or a run that traded because a trait was missing.

Keep the two apart in your head and in the run. Everything that *can* be settled offline is settled
offline first, because the live stages are where mistakes are expensive and where a rerun is not free.

## Safety — this step trades

Read the parent skill's safety section first; it governs. What follows is specific to this step.

- **Never run `just test-finance-write` on your own.** Not to check a fixture, not because the previous
  stage passed, not because the user approved the *step*. Each stage is approved on its own, in the call
  it runs in.
- **The fixture closes positions as cleanup.** `UserConnectorTestBase` cancels every open order on the
  account and market-closes every non-zero position, both on entry and on teardown. On an account that
  has anything of its own, that is not cleanup — it is the fixture trading. Before the first live
  trading stage, confirm with the user, one at a time:
  - the account's **position mode** is one-way (`EnsureOneWayPositionMode` throws with instructions
    rather than setting it — deliberately, because setting it is the user's decision);
  - **no position exists** on the test symbol;
  - **no open orders** exist that the user wants to keep;
  - **margin is sufficient** for the sizes the fixture uses.
- **Run the trading suite alone.** Nothing else against the same account concurrently — not a live read
  block, not a second venue, not a host.
- **A rejection test proves less than it looks.** `MapOperationCode` folds every negative Binance code to
  `BadRequest`, so "the exchange refused" is all a test can assert — an invalid key, an expired timestamp
  and a malformed parameter are indistinguishable at that level. Do not write a live test whose meaning
  depends on *which* refusal came back until that mapping is split.
- **Deadlines and tokens** are the parent's rule and apply here in full: every exchange-facing test
  carries `[Fact(Timeout = …)]`, every wait under it takes the test's token. A live test that hangs does
  not fail — it cancels the job, and *cancelled* does not read as broken.

## The documents

- `finance/kb/providers/<provider>/manifest.md` — the contract. Every fact this step exercises moves on
  the verification axis here, not only in code.
- `finance/kb/providers/<provider>/status.md` — where the step stands, and the open list it inherits.
- `finance/kb/providers/<provider>/<YYYY.MM>/<YYYY.MM.DD>-step-5.md` — the run report, written as the run
  goes rather than at the end. Immutable once written.

## Phase 0 — establish the tree

The parent's preflight: on `main` clean and level, or on `feature/<provider>` clean. Anything else is a
stop. Assessment is read-only; remediation branches first.

Then **assess before building**. This step is a reconciler like the others: measure what exists against
the target, and remediate only the drift. On a module that already has a connector, most of the code is
there and the gap is in what pins it.

## Phase 5a — the fixtures that do not exist yet

Two are missing, and both are prerequisites rather than nice-to-haves. Build them first; without them
the offline phases below cannot be written at all.

**An offline websocket server for the test lib.** ✅ Built: `Infrastructure/TestBaseWebSocketServerExtensions`
— `this.RunWebSocketServer()` returns a `TestWebSocketServer` whose `WaitConnectionAsync` hands out each
accepted connection in turn, and a `TestWebSocketConnection` that reads the frames the service sent
(`WaitMessageAsync`, `Received`), sends frames back (`SendAsync`) and drops the connection on purpose
(`Drop`). Connections are queued rather than passed to a callback, because the properties worth pinning are
about the *sequence* of them: a client that reconnects produces a second connection, and the assertion is
about what arrives on it. Everything in 5b depends on it.

**An offline HTTP fixture for the commands.** This one exists in pattern — `UserProviderReadPathTests`
drives the read paths against a local server through `TestBaseHttpServerExtensions`. The same pattern
transfers to the five commands, and it reaches two branches the live block structurally cannot.

## Phase 5b — the streams, offline

Each of these is a property of the connection lifecycle, and each is unpinned as of this draft:

- **Subscribe** sends one control frame per new topic, and only for topics not already tracked.
- **Unsubscribe** sends the frame and stops tracking.
- **Reconnect resubscribes** the whole tracked set — the single most important one, because a silent
  failure here is a connector that looks connected and delivers nothing.
- **Teardown unhooks before unbinding**, in that order; the source records why.
- **Errors reach the connector's error channel**, not a log line. A connector that fails silently is
  indistinguishable from one that is merely reconnecting.
- **A payload that does not parse is dropped and the stream survives.**

**What the fixture found on its first run, recorded here because the next venue will meet it too.** The
market `WebSocketService` reported itself *connected* before re-sending its tracked topics, and kept the
tracked set in a bare `HashSet`. Two consequences, both of which a test hits immediately and a live run
hides:

- a subscribe issued on the connected signal raced the connect-time resubscribe, and the same topic went
  out twice. The exchange tolerates that, so nothing ever complained;
- the set is written from the caller's thread on every subscribe and from the socket's thread on every
  reconnect, with no lock at all.

Both are fixed: the set is guarded, and **connected is reported last**, so that the signal means *connected
and resubscribed*. That ordering is what makes a stream test deterministic — waiting on the server having
accepted the socket is not enough, because the client finishes its handshake after that. Expect the same
shape in every venue's stream service and check it before writing the first test against one.

Two more things to look at rather than assume, both visible in the source at the time of this draft:

- the control-frame sends are **fire-and-forget** (`SendTextAsync(...).GetAwaiter()`, result never
  observed). A subscribe that fails is invisible. Decide whether that is the intended contract, and pin
  whichever answer you reach.
- the listen-key resolver **switches timer cadence** on the first confirmed key, and on a *changed* key
  it clears the key and raises reset without switching back. Whether that is right is a question for the
  contract, not for taste.

For the listen key specifically: fetch, confirm, change, failure-before-first-success, failure-after,
and disposal mid-flight. The resolver reports status on every one of those transitions, and the status is
what the connector's consumers see.

## Phase 5c — the order lifecycle, offline

The live block covers the happy paths of four commands and nothing else. Offline, cover what it cannot:

- **the `NotConnected` early return** on every command — five paths, none pinned;
- **a failed query build** — the processor has its own tests, the connector's handling of its failure has
  none;
- **any command with no live coverage at all** (at the time of this draft, `SetLeverageAsync`, including
  the flooring of leverage to `int32`);
- **branches the live tests structurally miss** — e.g. a modify of a non-Limit order is a cancel-and-
  reinit rather than a modify, and the live tests only ever modify limits;
- **the post-command reload behaviour** — what a success and a failure each request.

Then the ingestion side, which is the connector's other half: what a stream message does to orders,
positions, assets and trades; which status transitions write a change and which write a delete; what a
snapshot does to accumulated state.

## Phase 5d — registration and configuration

Done here, as part of building, not deferred: the factory wiring, the keys, the configuration shapes, the
endpoints.

Two checks that have caught real defects in this module and cost nothing to repeat:

- **Every disposable the factory creates is in the box.** At the time of this draft two are added with an
  explicit comment and two are not; verify rather than read.
- **Endpoints live in one place per venue.** One venue keeping a URI path as a literal inside a mapping
  profile while its twin keeps it in `Endpoints` is the asymmetry that hides a drift.

## Phase 5e — live, read-only

The connector connecting, subscribing, and delivering, against the real exchange. This re-validates what
step 4 already did plus the stream path. It is a gate of its own: approved separately, and it does not
open on step 5's offline work having passed.

## Phase 5f — live, trading — one stage at a time

Each stage is its own gate with its own approval, and the pre-flight above is confirmed **before the
first one**, not once per session.

A sensible ordering, cheapest and most reversible first:

1. a rejected order — nothing is placed that could survive the test;
2. a limit order placed and cancelled — reversible, no position;
3. a modify — still no position;
4. a position opened and closed — the only stage that carries real exposure.

After each stage, before the next: check the account is as the fixture believes it is. A stage that
failed part-way can leave an order open, and the next stage's fixture will "clean up" — which is to say,
trade.

## Done

- Every fact this step exercises is `pinned`, and every one an approved live stage observed is `live`,
  dated — in the manifest as well as in the code.
- Every stream event the provider sends is handled, or explicitly recorded as ignored and why.
- Status transitions map to the domain's vocabulary; errors reach the error channel.
- The open items this step inherited from step 4 are answered or re-recorded with a reason — notably the
  used-weight header question, which this step has to answer anyway for the order-count headers.
- Tests green; each live stage passed and approved in turn.
- `status.md` says where the step stands, and the run report says how it went.

## What this step does not do

- It does not decide whether a dead venue path is revived. If a provider's user connector is a stub —
  as spot's is, with every command throwing — that is a **decision to put to the user**, not a gap to
  fill silently. Reviving it is a step-5 run of its own.
- It does not change the contract. A disagreement with the exchange found here goes back to steps 1-2;
  this step ports the contract, it does not amend it.
- It does not run the write block to "see what happens". Every live call is a stage with a purpose and
  an approval.
