---
title: Request header reference
description: Request headers read by Htmx.Net and the HtmxRequestHeaders members that expose them.
---

| Header | `HtmxRequestHeaders` key const | Member | Type |
|---|---|---|---|
| `HX-Request` | `Keys.Request` | `Request.IsHtmx()` | `bool` |
| `HX-Boosted` | `Keys.Boosted` | `Boosted`, `IsHtmxBoosted()`, `IsHtmxNonBoosted()` | `bool` |
| `HX-Current-URL` | `Keys.CurrentUrl` | `CurrentUrl` | `string` |
| `HX-History-Restore-Request` | `Keys.HistoryRestoreRequest` | `HistoryRestoreRequest`, `IsHtmxHistoryRestoreRequest()` | `bool` |
| `HX-Prompt` | `Keys.Prompt` | `Prompt` (needs `hx-prompt` extension) | `string` |
| `HX-Source` | `Keys.Source` | `Source`, `SourceElement` | `string`, `HtmxElementReference?` |
| `HX-Target` | `Keys.Target` | `Target`, `TargetElement` | `string`, `HtmxElementReference?` |
| `HX-Request-Type` | `Keys.RequestType` | `RequestType`, `IsHtmxPartial()`, `IsHtmxFullRequest()` | `HtmxRequestType` (`Unknown`, `Full`, `Partial`) |

`HtmxRequestHeaders.Keys.All` returns every key above.

**Removed in htmx 4:** `HX-Trigger` (request) and `HX-Trigger-Name`; `HtmxRequestHeaders.Trigger`/`TriggerName` are obsolete errors. Use `HX-Source`. Details: [Request detection](/Htmx.Net/guides/request-detection/) and the htmx-level [facts](/Htmx.Net/reference/htmx4-facts/).
