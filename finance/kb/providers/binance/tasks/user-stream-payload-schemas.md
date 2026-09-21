---
title: The nested user-data-stream payloads cannot be retrieved from the documentation
type: task
status: open
provider: binance
market-type: usd-futures
opened: 2026-09-18
blocks: nothing
---

# The nested user-data-stream payloads cannot be retrieved from the documentation

## What is missing

Roughly twenty short field names — the nested objects inside the account and order stream events, and
since the conditional-order migration the ones inside its update event too — render on the vendor's page
from a schema embed that is present in neither of the files that index the documentation. Every
retrieval tier available reaches the page and not the schema.

They are therefore `unretrievable` on the documentation axis: not unchecked, not undocumented, but
documented somewhere no technique of ours reads.

## Why it is open rather than fixed

In practice it is closed from the other side. Every event this module reads is pinned against a payload
recorded off the wire, and the captures live beside the manifest. What is missing is the vendor saying
so, which matters for exactly one thing: a drift check cannot compare against a document it cannot
fetch, so a field renamed there would reach us as a test failure rather than as a warning.

## What it must not be mistaken for

`unretrievable` and `unchecked` look alike in a summary and mean opposite things. This is not a category
nobody looked at; it is one that was looked at, repeatedly, and could not be read. A later pass that
quietly re-marks it as checked has lost the distinction the marker exists to keep.

## What would settle it

- The vendor publishing a machine-readable index that includes the embed — worth one `curl` per pass,
  because the last such index turned out to exist after an earlier pass had concluded it did not.
- A Postman collection or an SDK of theirs carrying the same schema, which is a different artefact from
  the page and has been the better source before.
- Failing both: keep the captures current, and say in each contract pass that this category rests on
  bytes rather than on documentation.
