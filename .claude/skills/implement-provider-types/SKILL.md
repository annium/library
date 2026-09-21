---
name: implement-provider-types
description: Step 3 of implement-provider — the wire types and the code that reads and writes them, built from the contract and pinned by tests. Nothing here touches the exchange. Use as step 3 of implement-provider, or standalone when the user says "wire the types", "write the converters", "step 3", or asks for the serialization layer of a provider to be built or checked - in any language.
user-invocable: true
---

# Implement Provider — Types and Serialization

Turn the contract into types, and into the code that reads and writes the provider's wire format —
**with the tests that pin each fact it covers**. This is the layer where someone else's API becomes
this codebase's vocabulary, and the only layer where a single renamed field is both invisible and
total.

The format is not necessarily JSON. A provider may speak protobuf, FIX, SBE, msgpack, or a positional
text encoding, and the step is the same: transcribe the contract, and prove the transcription.

**Measured against the contract, never against a sibling implementation.** The neighbouring venue's
converter is the most available reference and the wrong one — it encodes that venue's contract, and
where the two agree it teaches nothing while where they differ it teaches something false. If the
contract does not say it, it is not established; go back to step 2 rather than to the file next door.

## Why this exists

A converter is the narrowest place in the system and the least defended by ordinary care. Reading a
field under the wrong name gives no error, no warning, and no failing build — it gives a default value
that flows outward and is read as data. A zero price, an empty collection, a status that maps to the
first enumeration member.

Everything downstream is written against what this layer produces, so a defect here is discovered by
its consequences rather than by its cause: the provider reads an empty list and concludes the account
is flat, the connector publishes an order at a price of zero. By then the search starts in the wrong
file.

The tests are not an accompaniment to this step; they are most of it.

## Safety

- **Nothing here touches the exchange.** This layer reads the contract and writes code and fixtures.
  No test block is run, no credentials are needed, no live call is made. A fixture is a recorded
  answer, not a fresh one — if you need a fresh one, that is step 2's live probe or step 4's, and it
  is gated there.
- **Fixtures are evidence and are committed.** A payload invented to make a test pass is not a
  fixture; it pins what we imagine rather than what the venue sends. Where a real payload does not
  exist yet, say so and get one, rather than writing a plausible one.
- **Credential files are never read for their values**, here or anywhere.

## The documents

- The contract — `manifest.md` for the provider — is the input, and it is also an output: every fact
  this step pins moves on the verification axis, and that move is recorded here rather than assumed.
- `status.md` records where the step stands. It is the answer to "where does this provider stand",
  and this skill is not.
- A run report is written at the end, under the provider's dated folder. It describes a run and is
  immutable; the living state stays in `status.md`.

## Phase 0 — establish the tree

**Read the code before believing the bookkeeping, in both directions.** A step recorded as *not
started* whose code exists is the expensive direction: it invites rewriting what is already written
and deleting tests that exist nowhere else. A step recorded as *done* whose code is missing is the
cheap one, because the next thing that runs says so.

This is not hypothetical and it is not rare — the bookkeeping lags the tree by however long it has
been since anyone reconciled them. Enumerate what exists: the types, the converters, their tests,
their fixtures. That inventory is the step's actual starting point, and the difference between it and
`status.md` is itself a finding worth writing down.

## Phase 3a — transcribe

Work category by category, in the contract's own order, so that coverage is checkable rather than
remembered. For each category: the types that carry it, the converter that reads it, the converter
that writes it where anything writes it.

Two rules that decide most of the design:

- **A name in the contract is the name in the code**, unless the domain already has a word for it. A
  helpful rename at this layer costs the reader the ability to grep for the wire name, which is the
  one string that appears in the documentation, the capture and the failure.
- **A value the venue does not send is not a default; it is an absence.** Decide explicitly what the
  type says about it — nullable, an option, a sentinel with a name — and record the decision in the
  contract. Defaults chosen implicitly become facts nobody wrote down.

## Phase 3b — the two directions

Coverage is two separate questions and only one of them is usually asked.

1. **Every field in the contract is read by the code.** The direction everyone checks.
2. **Every field read by the code is in the contract.** The direction that catches what we invented —
   a field read under a name the venue never had, which works because the venue never sends it and
   the absence reads as a default.

Do both, field by field, and record that both were done. A verifier that only ever runs direction one
will report full coverage over a converter reading three fields that do not exist.

**Enumerations map every documented value in both directions.** And watch how the mapping is built:
where one direction is generated by inverting the other, a test that round-trips through both proves
only that inversion works. Rename a value on the wire and the test still passes in full, because both
sides moved together. Pin the literals themselves in at least one test — the string the venue sends,
written out — and mutation-check it by renaming one.

**Positional payloads have their indices pinned.** There the index *is* the contract and nothing else
protects it: no name, no shape, no type error. Pin with values that are distinguishable from each
other, so that a permutation of the indices fails; a fixture where three of the positions are `0`
pins three positions at once and none of them individually.

## Phase 3c — the branches, not the converters

**A converter with a test is not a converter whose branches run.** "Pinned per converter, every one
having its own test with real fixtures" was true of a manifest and still hid two gaps, because a
fixture exercises the branch it happens to contain.

Three shapes to go looking for, all of which have been found this way:

- **A value the code synthesizes under a condition.** Where the contract records a conditional, the
  test carries both cases. One real instance: a timestamp set from the payload when the status is one
  value and to zero otherwise, with every fixture on hand carrying the other status — so only the zero
  arm had ever run, and collapsing the condition either way was invisible.
- **A drop rule.** "This field is read" does not state "an entry the venue will not accept is dropped",
  and a field list will never ask for it. **A drop rule is a fact, not a field**: write the absence and
  the drop behaviour into the contract as their own entries, then pin them.
- **A whole payload refused.** Where the code declines to hand over something incomplete rather than
  handing it over without a limit, that refusal is a fact. The alternative failure arrives much later
  and in another layer, wearing a costume — a ban issued against code that supposedly respects the
  limit it never read.

**Expect a negative branch to be unpinned and broken at the same time.** The two are not alternatives:
nothing ran it, so nothing told anyone it was wrong. A drop rule failed on first pinning because the
envelope read the array into a collection of non-nullable elements and kept the converter's honest
`null` — the rule was written and discarded one line below, and the first thing the next layer does is
read a field off every entry. The neighbouring branch in the same file did it correctly, so the
discrepancy lived between two adjacent cases of one `switch`.

Budget for a fix inside this step. A fact at `none` that turns out to be wrong was found by review,
and **review is not repeatable** — repairing it without a test returns it to exactly the state it was
found in.

## Phase 3c2 — constraints reach the model, or are recorded as dropped

A venue publishes the rules an order must satisfy, and this layer decides which of them the domain gets
to know. **A constraint read and discarded is the worst of the three options** — worse than one never
read, because the code demonstrably had it. Its cost is paid elsewhere and looks like nothing to do
with this layer: the caller cannot compute what it may ask for, so it learns the bound by being
refused, and the refusal names a rule rather than a number. That is hours spent in the wrong file, and
it has happened.

So, per constraint: it reaches the model, or the contract carries an entry saying it was dropped
deliberately and what a caller is expected to do instead. "The converter reads the four filters we
needed" is not a coverage statement; the question is what the venue publishes, not what we consumed.

Three rules for the ones that do reach it:

- **Do not tidy an asymmetric constraint into a symmetric one.** Where a venue states one inequality
  per side, it is not stating a band on both — and reading it as one makes the model refuse orders the
  venue accepts, locally, with nothing on the wire to explain it. The tidy reading is the one to
  distrust, because it is the one that looks like the other venue's.
- **Decide required versus optional by whether absence is representable.** A constraint whose absence
  says something — no bound — is optional, and a record missing it stays usable. One whose absence
  leaves nothing to compute with is required, and a record missing it is dropped entirely. Getting this
  backwards either invents a bound nobody stated or silently deletes a tradable record.
- **Model it in the domain's vocabulary where the venues disagree on the wire's.** Two market types of
  one venue spelling the same constraint differently have no shared wire name to borrow, and a record
  named after one of them teaches the reader that venue's spelling. This is the exception to *a name in
  the contract is the name in the code*, and it is narrow: it applies where there is no single contract
  name, not where one is merely inconvenient.

**Pin the mapping per end, with values that differ.** A fixture whose bounds repeat — the same number
on both sides, or one number reused for both ends — passes just as well when the sides are transposed.
Four different numbers is the whole test.

## Mutation-check every fact you claim to have pinned

Break the thing, watch the test fail, put it back. Without it a test is an assertion that some code
ran. Two failure modes to know, because both produce a *passing* mutant run, which is worse than a
failing one — it certifies a test that guards nothing:

- **Check that the mutant built.** Removing a line often makes something unused, and a repository with
  analysers as errors then fails the build while the runner, given a no-build flag, happily reports the
  previous binary as green. Read the build result before the test result. Where a mutation cannot
  compile as written, change a value rather than deleting a use: invert a constant, return the wrong
  field, swap two names.
- **Restore by reversing the edit, not by reverting the file** — unless the fix is already committed.
  Reverting returns the file to the last commit, which on uncommitted work throws the fix away along
  with the mutation, silently, and the next run then tests unfixed code and passes.

And one thing a survivor can mean besides a missing test: **the mutated line changed no outcome.** A
guard that cannot fire, a branch whose arms agree, a default that is unreachable — these survive every
mutation because there is nothing there to break. Before writing a test to kill a survivor, ask whether
the original line was doing anything; sometimes the finding is that it was not, and the answer is to
remove it and say why the line it was modelled on stays.

## Phase 3d — write back

Every fact this step covers moves from `none` to `pinned`, **in the contract as well as in the code**.
A test written and not recorded leaves the manifest understating what is defended, which is the same
defect as overstating it, pointed the other way — and the next reconcile pass trusts the document.

Record the *rules* and not only the fields. The drop rule, the refusal, the conditional and which arm
each fixture drives: these are the entries a field-shaped manifest has no slot for, and they are the
ones that were wrong.

Then the run report: what was assessed, what each check found, what was changed, and the test count
before and after. Point by point against the checklist, so that "it holds" is a claim with a shape.

## Done

- Every field in the contract is read; every field read is in the contract, checked in both
  directions and recorded as such.
- Enumerations map every documented value both ways, with the literals pinned somewhere that renaming
  one fails.
- Positional payloads have their indices pinned by distinguishable values.
- Every conditional in the contract has both arms driven by a test; every drop rule and every refusal
  is an entry in the contract and is pinned.
- Every fact this step covers reads `pinned` in the manifest, and the claim survives a mutation check.
- Tests green, build clean.

**→ GATE.** Present what moved and what was found. The next step builds read paths on these types,
and a transcription defect carried past this gate is discovered by its consequences.

## What this step does not do

- **It does not call the exchange.** Live validation is step 4's read-only half and step 5's trading
  half, each gated where it belongs.
- **It does not decide what is true about the provider.** That is the contract's job, settled in steps
  1 and 2. A question that arises here and cannot be answered from the contract goes back there rather
  than being settled by the most plausible reading of a neighbouring implementation.
- **It does not build read paths, endpoints or registration.** Those belong to step 4, together with
  the configuration they need.
