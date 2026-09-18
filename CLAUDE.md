# CLAUDE.md

## Merges

**Never merge anything without an explicit instruction from the user.** That covers pull requests, branch
merges and fast-forwarding `main` — here and in every other Annium repository. Open the pull request,
report it, and stop there. Approval for one merge is not approval for the next one.

**Never merge before CI has finished and passed.** Not with `--admin`, not because the checks look slow,
not because a local run was green — and not because the user asked for it in passing. If asked to merge
while CI is pending, wait for it and say so. A local suite is not CI: it runs on one machine, with that
machine's containers, network and clock.

**Never push to `main` directly in a repository that works through pull requests.** Every change goes
through one — documentation, skill files and provider KB included. It is easy to forget on a change that
feels like housekeeping: a commit made while standing on `main` after a merge looks the same as any other
until it is pushed, and in `library` a push to `main` releases a version. The umbrella's own `kb/` is the
one exception, and it has a policy of its own above.

**Where a repository has no CI, say what stands in for it.** `crypted` has none — no workflow files, no
runs — so "wait for CI" has nothing to wait for, and the merge rests on a local check instead. Name that
check in the merge, so the reader knows what was and was not verified: `just build-backend` (not
`just build` — the two Blazor sites need the `wasm-tools` workload, which is not installed), `just test`,
and, for anything on the backtest path, a run of **both** configs compared against the recorded reference
in the umbrella's `kb/plans/2026.09/2026.09.12-backtest-optimization-remaining.md`.

## Before pushing

Run **`just ci-check`** — it is what CI's first job runs, and it fails for reasons a build and a test run
never surface: formatting that `format` would change, a tree left dirty by it, and `docs-lint` over every
XML doc comment in the repository. The last one is the one that catches people. Inserting a new member
directly above an existing one puts the new code between that member and its doc comment, and the member
silently loses its documentation - three members in one branch, none of them visible in a green build or a
green test run.

## Language

**Everything written in this repository is in English.** It is public: code, comments, XML docs, commit
messages, skill files, and every document under `finance/kb/` — including run reports, which are the most
tempting exception because they read like notes to oneself. Conversations with the user happen in whatever
language they choose; what lands in the tree does not follow.

The one exception is text that exists *as data*: a `ru` locale fixture, a Cyrillic string in a test of
UTF-8 handling or of a signature over non-ASCII input. Those are inputs a test exercises, not prose, and
translating them would remove the coverage they exist for.
