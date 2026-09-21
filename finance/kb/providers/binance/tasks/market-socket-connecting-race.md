---
title: A market socket that connects too quickly may have reported itself connecting last
type: task
status: fixed-unproven
provider: binance
market-type: both
opened: 2026-09-21
blocks: nothing
---

# A market socket that connects too quickly may have reported itself connecting last

## What was wrong

`WebSocketService`'s constructor started the connection and only then reported the connector as
connecting:

```csharp
_socket.Connect(new Uri(config.WsApi, config.WsUriPath));
_statusReporter.Connecting();
```

`Connect` returns as soon as the attempt is under way. A connection completing between those two
statements is reported **connected** by the socket's own thread and then overwritten here —
permanently, because a socket already connected raises no second event. The connector then reports
itself connecting for as long as it lives, with a socket that works, which from outside is
indistinguishable from a venue that is merely slow.

Fixed on 2026-09-21 by reporting connecting first. The two user streams beside it always had the order
right, which is why only the market side ever showed the symptom.

## Why this is still a task

**The fix is correct; that it is the cause is not proven.** What was observed is a CI run of
`BookTickerServiceTests.Data_IsRaised` hanging for its full sixty seconds waiting for connected, while
the test server had already accepted a connection — exactly the state the old ordering produces. But the
window is between two statements, and it did not reproduce locally: 180 constructions, under a
single-core runtime and under two dozen busy loops, all green.

So the entry stays open as a **watch** rather than a repair. If the same test hangs again, the ordering
was not the cause and the search has to start elsewhere.

## What would settle it

- **The next occurrence, if there is one.** The status wait now fails with what it waited for and what
  the monitor actually reports, instead of a bare cancelled task. A recurrence that says "waited for
  Connected and the monitor is Connecting" means something else holds the connector there; one that says
  it never reported anything at all points at the socket rather than the reporter.
- **Silence.** Enough CI runs without it is the other answer, and the weaker one: a race that fired once
  in four runs and stops firing is evidence, but only as much as the count behind it.

## What is already in place

- the ordering fix, with the reasoning in a comment at the site;
- a regression net — thirty repeated constructions, each waiting for connected on its own deadline. It
  is a net and says so: a single construction crosses the window rarely;
- the diagnostic above, in `StatusMonitorTestExtensions.WaitStatusAsync`.
