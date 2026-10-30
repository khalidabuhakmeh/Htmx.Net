---
title: Response header reference
description: Response headers written by Htmx.Net and the HtmxResponseHeaders members that set them.
---

| Header | `HtmxResponseHeaders` key const | Helper |
|---|---|---|
| `HX-Push-Url` | `Keys.PushUrl` | `PushUrl(url)`, `PreventPush()` (`false`) |
| `HX-Replace-Url` | `Keys.ReplaceUrl` | `ReplaceUrl(url)`, `PreventReplace()` (`false`) |
| `HX-Redirect` | `Keys.Redirect` | `Redirect(url)` |
| `HX-Refresh` | `Keys.Refresh` | `Refresh()` |
| `HX-Location` | `Keys.Location` | `Location(string)`, `Location(HtmxLocation)` |
| `HX-Retarget` | `Keys.Retarget` | `Retarget(selector)` |
| `HX-Reswap` | `Keys.Reswap` | `Reswap(HtmxSwap.*)` |
| `HX-Reselect` | `Keys.Reselect` | `Reselect(selector)` |
| `HX-Trigger` | `Keys.Trigger` | `WithTrigger(name[, detail])` |
| `Vary` | n/a | `WithVary()`, `WithVary(params string[])` |

`HtmxResponseHeaders.Keys.All` returns every key above.

**Not emitted (removed in htmx 4):** `HX-Trigger-After-Swap`, `HX-Trigger-After-Settle`, status `286` (`StopPolling()` is an obsolete error). Details: [Response headers](/Htmx.Net/guides/response-headers/), [Triggering events](/Htmx.Net/guides/triggering-events/), and the htmx-level [facts](/Htmx.Net/reference/htmx4-facts/).
