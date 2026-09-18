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

