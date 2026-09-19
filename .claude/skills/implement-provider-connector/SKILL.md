---
name: implement-provider-connector
description: Step 5 of implement-provider — the connector: streams, the order lifecycle, registration and configuration, ending in live validation that places real orders on a real account. Runs in gated stages, each approved separately. Use as step 5 of implement-provider, or standalone when the user says "implement the <exchange> connector", "finish the <exchange> connector", "step 5", or asks for the streams or the order lifecycle to be finished - in any language.
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
- **A stage that fails does not clean up after itself.** The fixture's teardown runs at the end of a
  passing test; an assertion that throws part-way, or a runner that cancels the job, leaves whatever the
  stage opened in place. So: **verify the account after every failed stage, and keep a standalone tool
  that closes anything open** — a reduce-only market order for the position, a cancellation for each
  store that holds orders. Write that tool before the first trading stage, not after the first time it
  is needed.
- **A rejection test proves less than it looks.** Where the module folds every venue error code into one
  status - and that is the usual shape - "the exchange refused" is all a test can assert: an invalid key, an
  expired timestamp and a malformed parameter are indistinguishable at that level. Check the module's
  mapping before writing a live test whose meaning depends on *which* refusal came back.
- **A command that printed nothing may not have run.** Diagnosing a live stage means reading test output,
  and output arrives filtered through whatever wrapped the command. A wrapper that is absent, a pipe that
  buffers, a runner that caps the whole execution and kills it before it writes its log — each produces an
  empty result that reads exactly like a hang. Check the exit code before concluding anything about the
  code under test, and prefer a runner's own deadline over an external one, so a timeout comes back as a
  named failing test rather than as silence.
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

## Phase 5a2 — capture what the venue answers, before writing anything that reads it

**Run this the moment step 2 hands over a response shape it could not retrieve, and run it first.** The
instinct is to leave live work until the end; when the documentation has no response schema, that
sequencing is wrong and expensive. A converter written from an imagined shape compiles, passes the tests
its author wrote for it, and is discovered to be wrong only by a live run — at which point the tests
defend the mistake.

So: **the first task of a family the documentation does not describe is a logged probe, not the last.**

What a probe is: a small test in the read block — or the write block, approved as its own stage, when a
state has to be created to be observed — that performs the calls and **stores every raw answer as a
file**, then asserts only that the calls succeeded. It is an instrument, not a regression test.

- Store the answers **in the provider KB, beside the manifest**, with a note saying what each is. They
  become the source the converters are written from and the fixtures the tests use, and a reader can
  check both against bytes the exchange really sent.
- **Assert the status, not just that a body came back.** An error body is not empty either, so a probe
  that only checks for non-emptiness will report findings drawn from a refusal.
- Log levels below warning frequently do not reach a test log. Write the answers to files rather than
  logging them, or the probe runs, passes, and tells you nothing.
- Capture **the same object from every endpoint that returns it**. Answers to a placement, a listing and
  a history query are routinely *not* the same shape, and the differences are what the ingestion turns
  on.
- Where a state change is what you need to see, create the smallest one that produces it and undo it in
  a `finally`. A conditional order placed far from the market, a minimum-notional position opened and
  closed — these cost fees and answer questions no document can.

**Then write the converters from the captured files**, and use those same files as the test fixtures. A
fixture that is a real answer asserts what the venue does; an invented one asserts what its author
imagined.

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

- the control-frame sends were **fire-and-forget** (`SendTextAsync(...).GetAwaiter()`, result never
  observed), so a subscribe that failed to go out was invisible — and a connector subscribed to nothing
  looks exactly like one whose symbol is quiet. **Decided: a control frame that does not send is reported
  on the error channel**, because that channel is the only place a consumer can see it. The one exception
  is teardown, where a failed send is the expected consequence of the socket being gone — and where
  reporting is not merely noisy but throws, the reporter having been unbound. Whatever venue you are on,
  check the send's result *and* check what the teardown path does with it.
- the listen-key resolver **switches timer cadence** on the first confirmed key, and on a *changed* key it
  cleared the key and raised reset **without switching back** — while the failure branch beside it did
  switch back. **Decided: a changed key leaves the resolver in exactly the state it was in before its first
  key, timer included.** Anything else means the stream is closed by the reset and the replacement key is
  not asked for until a full confirm interval later - the keep-alive period rather than the retry period,
  which on a real venue is minutes to half an hour. A test catches it only if the two intervals differ — with both set to the same value
  the bug is invisible, which is why the first version of the test passed.

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

**A command reports what happened, and "nothing happened" is not success.** Two decisions from the first
run, both about a caller being told OK and acting on it:

- `SetLeverageAsync` returned `UserResult.Ok()` whatever the exchange answered, documented as
  fire-and-forget over the context reload. It does not any more: a refusal reaches the caller. And where
  the exchange *accepts* a leverage change without applying it, the connector compares what came back
  against what was asked and **refuses on its own** — a venue that answers OK and leaves the leverage
  where it was is indistinguishable from success to anything that only reads the status. Check every
  command of every venue for this shape: an answer that carries the resulting state is an answer worth
  comparing against the request.
- a modify of a Limit order into a Market order **stays refused**. The cancel-and-reinit path keys on the
  type of the order that exists, the query builder on the type being asked for, and such a request falls
  between them. Turning it into a silent cancel-and-replace would mean a caller asking to amend and
  getting a new order at a new place in the queue instead. The refusal is the contract.

## Checks that pay for themselves on every venue

Five questions to put to any venue, because each has produced a silent defect. Silent is the word that
matters: none of these announce themselves, and every one looks like correct behaviour from the inside.

**1. Is there more than one store?** A venue may keep a family of orders somewhere the ordinary
list does not reach — conditional orders are the usual case, but any family the venue treats specially
is a candidate. Ask the account for its open orders through every endpoint that has one and compare.
A connector reading a single store reports an account with none of that family on it, **as a shorter
list rather than as an error**.

**2. Does identity survive a state change?** When an order in one store becomes an order in another, ask
whether the venue carries the client id across. If it does, the two are one order and must be counted
once — and the record that knows the outcome is the one to keep. If it does not, the connector needs a
correspondence of its own, and that is a design decision rather than a detail.

**3. Which end does `limit` truncate?** Two endpoints of one venue can disagree. Measure it: ask for a
small page and look at the timestamps. An endpoint that returns the *oldest* n to a loader asking for
the latest page hands back a full page of real records, and nothing downstream can tell.

**4. Does the answer to a command carry what the caller needs?** A command's acknowledgement may report
that something happened without reporting its result — a fill without its price is the recurring case.
Where it does not, find where the value does arrive (a query, a stream event) and **assert it there**.
Moving an assertion to where the venue answers is not weakening it: a venue that stops reporting the
value at all still fails, and the property asserted becomes the stronger one — that the value *reached a
caller*, rather than that one particular response carried it.

**5. Does a converter that drops records have a nullable element type to drop into?** A converter
returning null for a record to omit is only as good as the collection reading it. Read into a collection
of *nullable* elements and filter; a non-nullable element type keeps the null and hands a caller an entry
with nothing in it. This has now been the same defect twice in one module.

## Mutation-checking, and the two ways it lies

Every fix gets a mutation check: break the thing again, watch the test fail, put it back. It is the only
evidence that a test defends anything. Both failure modes below produced a *passing* mutant run — the
worst possible outcome, since it certifies a test that guards nothing.

**1. Check that the mutant built.** Removing a line often makes a field, a method or a parameter unused,
and a repository with analysers as errors then fails the build. The test runner, given `--no-build`,
happily runs the *previous* binary and reports green. Read the build result before the test result; a
mutation that does not compile has not been tested, it has been skipped. Where the mutant cannot compile
as written, change the value rather than deleting the use — invert a constant, return the wrong field,
swap two names.

**2. Restore by reversing the edit, never by reverting the file** — unless the fix is already committed.
`git checkout <file>` returns it to the last commit, which on uncommitted work throws away the fix along
with the mutation, silently. The next run then tests unfixed code and passes, because the test was
written against the unfixed behaviour a moment earlier. Commit first, or undo the exact edit.

Both of these are the same shape as the rule about a command that printed nothing: **the instrument
failed, and its failure looked like a result.**

## Phase 5d — registration and configuration

Done here, as part of building, not deferred: the factory wiring, the keys, the configuration shapes, the
endpoints.

Two checks that have caught real defects in this module and cost nothing to repeat:

- **Every disposable the factory creates is in the box.** Both checks below found something on the first
  run, which is why they are worth repeating rather than reading past. On the first venue through this step
  the listen key resolver and the user stream were built by the factory and never added: the connector only unhooks their
  *events*, so after its teardown the socket kept reconnecting, the resolver kept POSTing a keep-alive
  forever, and both stayed bound to the monitor — which then never reads clean again. Fixed. Note the
  ordering: the stream is disposed before the resolver it listens to.
- **Endpoints live in one place per venue.** One venue keeping a URI path as a literal inside a mapping
  profile while its twin keeps it in `Endpoints` is the asymmetry that hides a drift - and it was there on
  the first pair of venues through this step, in both directions: one kept its websocket paths inline while
  its twin kept them in `Endpoints`, and that twin kept its listen key path inline in the factory instead.

Neither check has an offline test behind it, and that is not an oversight: the factory resolves real
endpoints from `Endpoints`, so building a connector through it reaches the exchange. What defends these is
reading them again, here, on every venue — the point of listing them as checks rather than as tests.

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

## When the exchange refuses the contract

Expect this shape, because it has already happened: a stage fails not because the connector is wrong but
because the venue no longer accepts what the manifest records as `confirmed` - an order type refused on the
endpoint the contract names, and with it the whole family of order types that share it.

What to do, in order:

1. **Check the account first.** A stage that failed part-way may have left a position open. Verify it -
   a throwaway read-only connection reporting positions and orders is enough - rather than trusting the
   fixture's teardown to have run.
2. **Record it where the contract lives**, with the refusal quoted and dated: the documentation axis moves
   to `contested`, the verification axis to `live` *negatively* - what is observed is the refusal.
3. **Hand it back to steps 1-2 and stop.** Which endpoint accepts it, what it takes and what it answers is
   a documentation question. Guessing an API from one error message is how a manifest fills with fiction.
4. **Say what the stage still proved.** A stage that fails at its third action has validated the first two,
   and that is worth recording rather than losing in the failure.

## What this step does not do

- It does not decide whether a dead venue path is revived. If a provider's user connector is a stub —
  as one venue's is in this module, with every command throwing — that is a **decision to put to the user**, not a gap to
  fill silently. Reviving it is a step-5 run of its own.
- It does not change the contract. A disagreement with the exchange found here goes back to steps 1-2;
  this step ports the contract, it does not amend it.
- It does not run the write block to "see what happens". Every live call is a stage with a purpose and
  an approval.
