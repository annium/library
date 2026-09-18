---
name: implement-provider-contract
description: Establish an exchange provider's wire contract in two gated steps — first derive what the code currently assumes, then collect the provider's actual API facts and compute the drift between them. Step two must be complete before anything is built on it. This is the drift check. Use as steps 1 and 2 of implement-provider, or standalone when the user says "check the provider contract", "check the API for drift", or asks to reconcile a provider against the exchange documentation - in any language - or when an exchange test fails for a reason that might not be our defect.
user-invocable: true
---

# Implement Provider — Contract

Produce and keep current the **wire contract** for one exchange provider: every fact about someone
else's API that this module depends on. The contract is the target every other layer is measured
against, so it runs first and re-converges first.

It is **two steps with a gate between them**, and they answer different questions. Step 1 asks what
*we* believe; step 2 asks what is *true*. Run together they blur: a fact read in the documentation
while the code is open reads as agreement more often than it should. Derived first, in the code's own
terms and confirmed by the user, the comparison in step 2 has something fixed to push against.

Assessing the contract against the exchange's documentation **is** the drift check. There is no
separate skill for it, and there should not be: a drift report that is not written back into a target
is a document nobody reads twice.

## Why this exists

This module encodes several hundred facts it does not own — endpoint paths, parameter names, JSON
property names, filter type strings, status spellings, numeric error codes, the positional layout of a
kline. None of them announce themselves when they change.

When the exchange moves one, a test fails, and the failure looks exactly like our own defect. The
natural response — read our code, find nothing wrong, read it again — costs hours and sometimes ends
in "fixing" correct code. This check exists so the first question after such a failure is answerable
in minutes: **did the thing we depend on change?**

Run it *before* validating against the exchange, not after things start failing.

## Safety

- **Never run a test block here.** Nothing in these two steps calls the exchange: this layer reads code
  and reads documentation. There is no environment variable to set or avoid — the gate that existed was
  removed deliberately, and the block's trait is now the only thing separating a routine run from a
  trading one.
- `test.env` holds real credentials. Never read them for their values, never print them.

## The documents

```
kb/providers/<provider>/
  manifest.md                     the contract — this layer owns it
  status.md                       convergence and history — this layer writes its own rows
  <YYYY.MM>/<YYYY.MM.DD>-contract.md      the run's report, immutable
  <YYYY.MM>/<YYYY.MM.DD>-docs/            the documentation this run read
```

One manifest per exchange, with a shared section and one per market type, because the divergences
between market types are the drift-prone part and are only visible when both halves sit on the same
page.

## Phase 0 — establish the tree

Clean and on `main` level with `origin/main`, or on `feature/<provider>`. Anything else stops the run;
report the actual state and ask. Never stash, never force, never hard-reset.

## Step 1 — derive the existing state from the code

**On a first run** there is no manifest. Derive the manifest from the code: sweep the provider's tree and
record every fact that belongs to the exchange, anchored to `file:line`, in the categories below.
Everything derived here is `unchecked` on the documentation axis — it comes from our code and has been
compared to nothing — and whatever the code's own tests already defend on the verification axis. Set
`checked_against: never`. This is an inventory, not yet a baseline.

The two axes are the parent skill's; a manifest written with `[UNVERIFIED]` markers instead would be
speaking a vocabulary this one replaced. Where such markers survive in an existing manifest they are
leftovers, not examples - re-express each on the two axes as you meet it.

**On every later run**, verify the manifest still describes *this* code before comparing it to anything
external:

- **Every anchor resolves to what it claims.** Line numbers move with every edit, and a manifest
  pointing at the wrong line is worse than none — it will be trusted. Fix the ones that moved.
- **Every assumption in the code has an entry.** Sweep for new ones. This gap is the blind spot the
  document exists to close, and it opens quietly: a converter gains a field, nobody updates the manifest,
  and that field is now outside every future drift check.
- **Every entry still corresponds to live code.** An entry for something deleted becomes `[DEAD]` or
  goes.

**Record the verification state as you go.** This step reads the code, so it is the step that can see
what tests exist. For each fact: `pinned` (an offline test fails if it changes), `gated` (covered only
by a provider-facing suite that is skipped by default, so it proves nothing on an ordinary run),
`vacuous` (a test names it and cannot fail), or `none`. Look for `vacuous` deliberately — an assertion
over a possibly-empty collection, a wait that swallows its own timeout, a negative check with no
positive control. It is worse than `none`, because the effort looks spent.

On a greenfield provider the derivation is **empty**, and saying so explicitly is the result: an empty
derivation and an unasked question look identical a month later.

Delegate this to a fresh subagent given the manifest and the tree. A reviewer who wrote the manifest
will read what it meant to say.

**→ GATE.** Present the derived state and stop. The user confirms it describes the implementation
before it is compared against anything external. Every fact is `unchecked` on the documentation axis —
it is what we believe, not what is true, and the distinction is the whole point of deriving it
separately. Present the verification census with it: how many facts nothing tests, and how many are
`gated` or `vacuous`, which look tested and are not.

## Step 2 — collect the provider's facts, and compute the drift

### Phase 2a — snapshot the documentation

**Completeness is this step's gate condition, not an aspiration.** Every category covered, every entry
given an outcome. A step-3 transcription measured against a half-checked contract inherits the
unchecked half as silent assumption, and nothing downstream will ever question it. A source that
cannot be retrieved is a **gap**, recorded as one, and it **blocks** — "the important parts were
checked" is precisely the reasoning that leaves the unchecked parts unchecked forever.

Do not read the documentation live and compare it against memory. **Fetch it, store it, and diff it
against the previous run's snapshot.** Memory of a vendor's API is exactly the thing that drifts, so a
check that relies on it is checking the wrong artefact.

Retrieve in tiers, best first, and record which tier each file came from:

| tier | source | how |
|---|---|---|
| 1 | the vendor's own docs repository | `curl -sSL https://raw.githubusercontent.com/<org>/<repo>/<ref>/<path>` — and pin the commit SHA |
| 2 | a docs site that serves markdown | try `curl -sSL "<page-url>.md"` — many documentation sites serve their source this way even when the page itself is a protected single-page app that returns an empty `202` to a plain fetch |
| 3 | `WebFetch`, only where neither works | **lossy**: it returns a model's reading of the page, not the page. Mark every file so retrieved, and say so in the report |

**Do not build the process around tier 1.** Most providers publish no repository at all, and the check
must be as good without one — the snapshot in our repository is what supplies the history, and it does
that whichever tier filled it.

Where a vendor publishes separately per market type, fetch each. A rename on one venue and not the
other produces a failure that looks venue-specific, and therefore looks like ours.

Record the page paths that worked, and the ones that did not, in the manifest's documentation section.
Locating a vendor's real page paths is a substantial part of a first run's cost and there is no reason
to pay it twice.

Scope the snapshot to what the manifest references, plus changelogs. A full documentation set is mostly
about things we do not use, and volume nobody diffs is volume that hides the diff.

#### Verify what you actually fetched

**A `200` with a body is not proof you got the page you asked for.** A documentation site backed by a
single-page app answers an unknown path with its HTML shell, status `200`, and a plausible-looking
body. On the first run through this skill, five different endpoint paths came back **byte-identical** -
the same 65 KB shell each time - before anyone noticed.

Two checks, both cheap, and neither optional:

- Reject any response whose body begins `<!doctype html>`. A markdown source never does.
- Compare sizes across the batch. Identical byte counts for pages that should differ means you
  collected the same fallback several times over.

Whatever fails these is **a gap, not coverage**. Record it in `SOURCES.md` — which pages could not be
retrieved and what was tried — and say so in the report. A category checked against the wrong document
is worse than one openly skipped, because it will be counted as verified.

#### Follow the links out of the changelog

**The changelog is not the documentation.** On the first run through this skill a WebSocket migration
whose deadline had already passed was not a changelog entry at all: it lived on its own page, reachable
only through one link inside the changelog.

So: read the changelog, then extract its internal links and fetch those too. A vendor announcing
something large tends to write it up separately and link to it, which is exactly the shape a
changelog-only check misses.

**And do not let that lesson displace its opposite.** The same run followed the link, found the notice,
reported it — and missed a second migration of comparable weight sitting in the changelog itself as an
ordinary bullet list. Having learned that the important things hide outside the changelog is not a
reason to read the changelog less closely. Both are now covered mechanically by the sweeps in 2b, which
is the only reliable answer to "which of these did I skim".

#### Look for the machine-readable index before reading anything by hand

Before treating a documentation site as prose to be read, check whether it publishes itself as data.
The conventional paths are `<site>/llms.txt` — a page index — and `<site>/llms-full.txt`, the whole
documentation set as one file. Both are worth one `curl` each.

On the second run through this skill these turned out to exist for a vendor the first run had recorded
as having an unreachable API reference, after that run tried five catalog URL shapes and correctly
concluded none of them served markdown. The index had been fetched by that same run and read as a list
of page paths; the chapter listing **every endpoint by method and path** was four hundred lines further
down the same file.

So: fetch the index, and read *all* of it, including the parts that do not look like page paths. An
endpoint inventory is the cheapest drift check that exists — **store it and diff it across runs**,
because an endpoint appearing or disappearing there is a contract change that needs no changelog entry
to happen.

A full-text dump is usually too large to store whole, and storing it would defeat the scoping rule
above. Store the **extract** the manifest needs, with the source file's byte count, hash and line
range, which is enough to reproduce.

Write `SOURCES.md` beside the files: for each, the URL, the tier, the fetch date, the size, and the
pinned revision where there is one.

**A 404 from a raw-content host is a small successful file, not an error.** `curl -sSL` writes
`404: Not Found` — 14 bytes — to the output and exits 0. Vendors restructure documentation
repositories, and the path that worked last run is exactly the one nobody re-checks. The size column in
`SOURCES.md` is what catches this; nothing else will.

### Phase 2b — diff, category by category

#### Read the corpus through our facts, not the other way round

**This is the part that failed, twice, on the same provider.** Both times the finding was in a file the
run had already fetched and stored, and both times it was missed by reading that file looking for what
seemed important. The second one — a whole family of order types moved to a different endpoint, its
deadline months in the past — sat in the changelog as an ordinary bullet list among hundreds. It was
eventually found by placing a real order and being refused by the exchange, which is the most expensive
way a contract question can be answered.

The lesson is not "read more carefully". Reading someone else's corpus and asking *what here matters to
us* is an unbounded judgment over tens of thousands of lines, it cannot be audited, and what it finds
depends on what happened to look dramatic. Our facts, by contrast, are a **bounded, enumerable set** —
that is what the manifest is. So invert the direction: for each fact of ours, go and find where their
documentation confirms it.

Three sweeps produce that, and all three are mechanical. Run them **before** reading anything by
judgment.

**1. Grep the snapshot for every wire literal in the manifest.** Order type strings, status spellings,
endpoint paths, header names, filter type names, JSON property names. For each literal, read every hit
and its surrounding entry. The counts stay small enough to be honest about: on the run that introduced
this rule, the literal that mattered had **nine** hits in a changelog of more than a hundred kilobytes,
one of them the announcement a whole-file read had missed. A literal found nowhere in the snapshot is
`undocumented` — and now *measured* as such rather than assigned by someone's impression.

Watch the case: a header or field spelled one way in code and another in the documentation reads as
zero hits, which is a far stronger claim than "spelled differently". Re-grep case-insensitively before
recording anything as absent.

**2. Sweep the changelogs by date.** Extract every line carrying a date together with one of
*effective*, *deprecated*, *retired*, *migrated*, *removed*, *no longer*; sort; read everything dated
after the previous `checked_against`, and on a first run everything within the lifetime of our code.
This catches what sweep 1 cannot: an announcement phrased without naming any literal we hold. It is
also small — 44 lines on the run that introduced it.

**3. Diff the endpoint inventory**, where the vendor publishes one (see 2a). Catches an endpoint added
or withdrawn with no changelog entry at all.

**Then, and only then, diff the snapshot against the previous run's and read what moved.** That diff is
the weakest of the four nets and must not be mistaken for the strong one: it sees only what changed
since we last looked, so it is blind to everything the *baseline* got wrong — and a baseline is read
once and trusted forever. The finding described above was not in the diff. It was already in the
baseline, correctly fetched and never read.

On a first run there is no diff at all. Say so in the report: this run established the baseline rather
than measured a change, which makes sweeps 1 and 2 the entire check rather than a supplement to it.

#### Every outcome carries its evidence

Walk the manifest in order. Every entry gets exactly one outcome:

| outcome | meaning |
|---|---|
| **unchanged** | still as recorded. Say so — knowing what held is half the value |
| **changed** | record the old assumption, the new documented fact, and every `file:line` carrying the old one |
| **deprecated** | still works, announced for removal at a date **still in the future** |
| **new** | the exchange added something we do not use and arguably should — a filter, an order type, a field carrying information we currently derive |
| **undocumented** | we depend on something the documentation does not state |
| **contested** | two sources disagree — record both readings and what would settle it, and pick neither |

**An entry may only be `unchanged`, `changed`, `deprecated` or `new` if it carries a citation into the
stored snapshot** — `<snapshot-file>:<line>`, naming the text the outcome was read from. Without one
the entry is `unchecked`, whatever the reader believes. This is the rule that makes a skipped fact
visible: today a fact nobody looked at and a fact confirmed line by line look identical in the
manifest, and that is exactly how a whole category gets counted as verified.

It is the documentation-axis twin of `vacuous` on the verification axis, and deserves the same
suspicion for the same reason — **the effort looks spent**. A `confirmed` with no citation behind it
is worse than an honest `unchecked`, because nothing will ever question it again.

The citations are a by-product of sweep 1, not extra work: the grep that finds the literal is already
standing on the line that proves it.

An entry whose value is **assembled** from parts — a base and a path, a template and a substitution —
is one fact, not two, and it is the assembled value that must be recorded and checked. Recording the
parts separately hides the composition, and composition is where these go wrong: this provider's
websocket route was correct in both the base and the path and wrong once joined.

**Every date in the documentation is read against today's date.** An announced removal is only
`deprecated` while its date is ahead of us. Once that date has passed the entry is `changed`, and
almost certainly blocking: the thing was withdrawn and we did not move.

This has now happened twice on one provider — deadlines four and nine months in the past — and both
notices read in the future tense, because both were written before the date they announced. **A vendor
never rewrites an announcement once it takes effect.** "Will migrate on <date>" stays on the page
forever, and a reader supplying no present date will parse it as upcoming work indefinitely.
Documentation states dates; only the reader supplies the present. That is what sweep 2 mechanises, and
why it is a sweep rather than an instruction to be careful.

Each outcome sets the fact's **documentation** state. Do not touch its verification state here: this
step compares us against the provider, and nothing it learns changes what our tests defend.

**The undocumented ones are the finding, not the footnote.** They were inferred from observed
behaviour, they can change with no changelog entry, and no future drift check will catch them in
advance. Flag every one, including those that still hold, and say what would happen if it stopped
holding. A check that only reports changes will never mention them, and they are the facts most likely
to break silently.

Read categories in this order, most consequential first: endpoints and auth, then request parameters,
then enumerations and error codes, then response fields, then filters, then rate limits, then timing.

### Phase 2c — report

Write `kb/providers/<provider>/<YYYY.MM>/<YYYY.MM.DD>-contract.md`, beside the snapshot it was written from. Immutable once written.

**The report is written against the stored snapshot, never against the live site.** That is what lets a later reader check the reasoning instead of taking it on trust, against a page that has since moved.

Group by outcome, severity first — a changed endpoint or error code outranks a new optional field.

For each **changed** entry, state **what will actually happen in our code**: an order rejected, a field
silently null, a filter unrecognised so the instrument is dropped entirely, a status folded into
`UnknownError`. That consequence is what makes the finding actionable and what tells the reader whether
it blocks the exchange run. A diff alone does not.

End with a plain statement: does anything found block running against the exchange?

### Phase 2d — write back

Update `manifest.md` **in place**: correct the changed facts, adjust markers, set `checked_against` to
today, and append one line to `status.md`'s reconcile history pointing at the report.

Where drift implies work in layers 2–5, write it into `status.md`'s rows for those layers as their
remediation spec. Do not fix it here. This layer owns the contract; the fixes belong to the layers that
carry it, and mixing them costs the ability to ask what the exchange changed, separately from what we
did about it.

**Commit the manifest edit, the status edit, the report and its snapshot together, in one commit.** A
manifest changed without the snapshot it was changed from leaves a fact with no evidence behind it.
Code fixes go in separate commits, so the manifest's history stays a history of the exchange rather
than of our repairs.

### Phase 2e — gate

Present: what was checked, what held, what drifted with its consequence, and whether anything blocks
the exchange run. Stop. Remediation of the other layers is the parent's business and the user's call.

## Manifest categories

The sweep and the diff both walk these, in this order:

1. **Endpoints** — every base URL, path and HTTP method, per market type.
2. **Auth and signing** — the algorithm, exactly what is signed, header names, timestamp source,
   validity window, and any stream-key lifecycle including which HTTP method extends it.
3. **Request parameters** — every parameter sent per endpoint, with hard-coded values called out. A
   hard-coded value is an assumption about the exchange wearing the costume of a constant.
4. **Response fields** — every JSON property read, per response type, including positional arrays,
   where the index *is* the contract and nothing protects it.
5. **Filters and limits** — instrument filters by their exact type strings, which field each populates,
   and what happens when one is absent. Absence behaviour is frequently the thing that changed.
6. **Enumerations** — every string value mapped to a domain enum, in both directions, with literals.
7. **Error and status codes** — every numeric code mapped and every HTTP status treated specially.
8. **Rate limiting** — header names, configured ceilings, decay, and whether the arithmetic is
   self-consistent.
9. **Timing and lifecycle** — intervals, page sizes, query windows, keepalive cadences, session limits.
10. **Hard-coded exchange facts** — magic numbers and defaults that mirror an exchange limit, wherever
    they sit.

For each entry also record, where it applies: `[DIVERGES]` between market types, `[DUPLICATED]` across
files, `[DEAD]` if unreachable from production. All three change what a later change costs, and all
three are invisible from any single file.

## What these steps do not do

- They do not call the provider. A fact reaches the `live` verification state in steps 4 and 5, from an
  actual response, and carries the date it was seen.
- It does not resolve a disagreement between two sources by picking one. Two sources that contradict
  each other are **unresolved**, recorded as such, until a third settles it — a search summary claiming
  a change the changelog does not contain is not a finding.
- It does not fix drift. It specifies it.
- It does not judge our code's correctness — only whether it matches an external fact. A correct
  implementation of a changed API and an incorrect implementation of an unchanged one are different
  problems, and conflating them is how a drift check turns into an argument.

## When a drift check is not the answer

If a test fails and the manifest says the relevant fact is unchanged and documented, the failure is ours.
Do not keep re-reading the exchange's documentation hoping to find an excuse. This check narrows the
search; it is not somewhere to hide.
