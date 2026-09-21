---
name: implement-provider-reads
description: Step 4 of implement-provider — the provider layer's read paths (instruments, candles, account, orders, trades), their registration and configuration, and live validation that reads and never writes. Use as step 4 of implement-provider, or standalone when the user says "build the read paths", "finish the provider layer", "step 4", or asks for a provider's reads to be wired or validated against the real exchange - in any language.
user-invocable: true
---

# Implement Provider — The Read Paths

Build the provider layer: instruments and their limits, candles, the account, orders, trades — the
requests that ask the exchange a question and change nothing. **With their tests, their endpoints,
their registration and their configuration**, and ending in live validation that reads only.

This is the first step that talks to the exchange, and the half of validation that cannot cost money.
That is why it comes before the connector rather than after it: everything the trading step will rely
on — signing, clock, endpoint composition, rate limiting, error mapping — is exercised here first,
where the worst outcome is a failed request.

## Why this exists

A read path is easy to write and easy to get wrong in ways nothing reports. It asks for a page and
gets one; the page is full, so it looks right. It sends a parameter the venue ignores. It receives an
error and returns an empty collection, and the caller concludes there is nothing there.

The failures share a shape: **the wrong answer and the boring answer are the same bytes.** An empty
list means "nothing matched", "your window was wrong", "you were refused", and "we discarded the
error" — and only one of those is worth acting on. This step exists to make each of them distinct
before anything trades on the difference.

## Safety

- **The read block only.** Nothing in this step places, modifies or cancels anything. The trading
  block belongs to step 5 and is gated stage by stage there. If a fixture here would need an order to
  exist, that is a step 5 question, not a reason to place one.
- **Reading is not free.** A live read spends the account's rate allowance and counts against the same
  ceiling the trading block later needs. Run the read block alone, and leave a gap before anything
  else goes at the same account — a starved run does not report rate limits, it reports failures that
  read like defects.
- **Credential files are never read for their values, never printed, never committed.** A test that
  needs credentials takes them from the environment the project already defines.
- **Confirm before the first live run** that credentials are present and scoped to reading. A key with
  trading permission is not needed by anything here.

## The documents

- The contract — `manifest.md` — is the input: every endpoint, parameter, cap and error this step
  calls is a fact established in steps 1 and 2. It is also an output: facts this step exercises move
  to `pinned`, and facts a live read observes move to `live`, dated.
- `status.md` records where the step stands, and this skill does not.
- A run report goes under the provider's dated folder at the end.

## Phase 0 — establish the tree

Enumerate what exists before writing anything: the provider classes, the read paths already there,
the test project, the fixtures. **If the test project does not exist, it is created as part of this
step** — deferring it is how a layer ends up with live validation and no offline coverage, which then
reads as "tested" in every summary.

## Phase 4a — endpoints, registration and configuration

Done **here**, as each path is built. A read path whose endpoint is configured in a later step is a
read path that cannot be tested in this one — and it will be tested by the later step's fixture, which
is not the same thing and does not say so.

**Assert the composed value, not the configured parts.** Configuration that reads as correct can
compose into something else. One real instance: a route held in the base address was discarded at
composition, because joining a base with a path drops the base's own path whenever the path begins
with a separator — so settings that looked right produced a decommissioned URL. A test on the parts
passes. A test on the composed URL catches it. **Where a value is assembled before use, the assembled
value is the fact**, and that is what the test names.

The same applies to anything registered under a key: assert that resolving the key yields the thing,
not that the registration line exists.

## Phase 4b — drive every path offline

Against a local server answering recorded fixtures, so that the shape under test is the code and not
the day's market. For each path, four questions, and the last two are the ones that get skipped:

1. **The request.** Every parameter the contract names, spelled as the contract spells it, and nothing
   the contract does not name. Assert the composed request, not the builder's inputs.
2. **The answer.** The full payload maps to the domain, including the fields nobody looks at.
3. **The failure.** An exchange error reaches the caller as an error. This is where an upstream defect
   was found: an error was discarded whenever the success type was a collection, so a refusal arrived
   as an empty list and every caller read it as "nothing there". Drive at least one refusal per path
   family and assert on what the caller can act on.
4. **The emptiness.** A genuinely empty answer is distinguishable from the failure above. If the two
   produce the same value, the path has one outcome where it needs two.

**Paging and windows match the documented caps, and the fixture spans more than one of them.** A
single-window fixture pins that one request is made; it says nothing about the cursor, which is the
part that breaks. Make the fixture span at least three windows so the cursor is exercised between
them, and make the pages non-uniform.

**Which end a page cap truncates is a fact to measure, not to assume.** Two endpoints of the same
venue can disagree: one page limit returning the oldest rows and another the newest. A loader asking
for "the latest page" and receiving the earliest one notices nothing, because a full page of real data
is exactly what it expected — the defect is invisible until the account outgrows a single page, and
then it is silent and permanent. Where recency is what is wanted, bound by time rather than by count,
and pin the window.

**Fix what the census says nothing watches, and pin it in the same change.** Steps 1 and 2 leave a
list of facts at `none`. A fact at `none` that turns out to be wrong was found by review, and review
is not repeatable — repairing it without a test returns it to the state it was found in, where the
next drift is invisible again.

**Every test carries a deadline and threads the cancellation token.** Nothing these tests wait on ends
by itself: an exchange unreachable from where the test runs answers nothing, and a wait with no
deadline lasts as long as whoever is watching. Check this by enumeration over the files, not by
memory.

## Phase 4c — live, read-only

Cheapest and least authenticated first, so that a failure is attributable:

1. the clock — the one request that needs nothing and proves reachability;
2. public reference data — instruments and their limits;
3. public market data — candles, a book, a ticker;
4. the signature itself, on the smallest authenticated read there is;
5. authenticated reads — account, open orders, recent trades.

Nothing here places an order. That is what makes it the safe half of validation.

**Confirm the answers will be recorded before running a stage that exists to observe one.** A live read
whose response body is never logged leaves a trace full of questions and no answers, and the green test
then proves only that the path completed — so "we checked" becomes unsupportable a day later when
someone asks what the venue said. Log the body at the level the run uses, or store it as a file, and
bound what is logged so a large payload cannot drown the trace.

**Expect the first live run to fail on something the offline suite could not see**, because that is
its purpose. What it found once: a path pinned at a version the provider had moved on from — marked
in the contract as an oddity and never checked, so every offline fixture agreed with the code and
both were wrong about the world. Fix, then pin, so the same failure cannot return silently.

**Where a reference payload lists everything, census it rather than reading one record.** A reference
response — the catalogue of instruments and their rules — answers questions no document does, and
answers them over the whole population rather than the one example a capture holds: which constraints
every tradable record carries, which are absent, how many distinct values a field takes, whether a
value the documentation shows as typical is in fact universal. It costs one request and a short script,
it settles design questions that would otherwise be decided by assumption — required or optional,
uniform or not — and the numbers belong in the contract, dated, because they are a measurement and will
decay.

One caution that makes it evidence rather than an impression: **count over what the code would accept**,
not over the whole payload. A population that includes records the provider filters out answers a
different question than the one being asked.

**A live run passing is not convergence.** This step was once called converged on the strength of a
green live run while its checklist still had three items open. The live run answers one question —
does the venue accept what we send — and the checklist answers the rest. Run both, and report them
separately.

## Phase 4d — write back

- Facts exercised offline read `pinned`. Facts a live read observed read `live`, **dated** — the date
  is what makes the claim decay honestly.
- A fact the live run settled that the documentation could not is worth its own entry: it is the kind
  that no later reading of the docs will re-derive.
- The run report: what was built, what each check found, what the live stages did, and the test count
  before and after.

## Done

- Every read path driven offline, including its failure path and its empty path, with the two
  distinguishable.
- Every endpoint called with exactly the contract's parameters; every composed value asserted as
  composed.
- Paging and windows match the documented caps and are pinned by a fixture spanning several of them.
- Registration and configuration for everything this step touches live in this step, and resolving a
  key is what the test asserts.
- Every test carries a deadline and threads the token, verified by enumeration.
- Tests green offline; the read-only live stages pass; both reported.
- The manifest carries `pinned` and dated `live` for what this step established.

**→ GATE.** Present the offline result and the live result as two separate claims. Step 5 trades, and
it trades through the endpoints, signing and error mapping this step just proved.

## What this step does not do

- **It does not place, modify or cancel anything.** If a question can only be answered by an order
  existing, it is step 5's question.
- **It does not build streams, the sync cycle or the order lifecycle.** Those are step 5, with their
  own registration and their own gated live stages.
- **It does not decide what is true about the provider.** A question that cannot be answered from the
  contract goes back to steps 1 and 2.
