---
title: Response headers
description: Set htmx 4 response headers with Response.Htmx(), including HtmxLocation, HtmxSwap constants and Vary.
---

```csharp
using Htmx;

Response.Htmx(h => h.PushUrl("/products").Retarget("#list").Reswap(HtmxSwap.OuterMorph));
```

`Response.Htmx(Action<HtmxResponseHeaders>)` gives a fluent `HtmxResponseHeaders`. Every helper returns the same instance.

| Helper | Header | Notes |
|---|---|---|
| `PushUrl(string)` | `HX-Push-Url` | Push URL into history. |
| `PreventPush()` | `HX-Push-Url: false` | Suppress a history push. |
| `ReplaceUrl(string)` | `HX-Replace-Url` | Replace current URL. |
| `PreventReplace()` | `HX-Replace-Url: false` | Suppress URL replace. |
| `Redirect(string)` | `HX-Redirect` | Full page navigation. |
| `Refresh()` | `HX-Refresh: true` | Full page reload. |
| `Retarget(string)` | `HX-Retarget` | CSS selector overriding the target. |
| `Reswap(string)` | `HX-Reswap` | Swap style, e.g. `HtmxSwap.OuterMorph`. |
| `Reselect(string)` | `HX-Reselect` | CSS selector choosing response content. |
| `Location(string)` | `HX-Location` | htmx-style navigation to a path (no full reload). |
| `Location(HtmxLocation)` | `HX-Location` (JSON) | Navigation with options. |
| `WithTrigger(...)` | `HX-Trigger` | See [Triggering events](/Htmx.Net/guides/triggering-events/). |
| `WithVary()` / `WithVary(params string[])` | `Vary` | See below. |

Response headers are not processed on 3xx responses; return 2xx.

Sample: `samples/Htmx4.Sample/Controllers/DemoController.cs` (`resp/{name}`):

```csharp
Response.Htmx(h => h.PushUrl("/ResponseHeaders?pushed=1").WithVary());
Response.Htmx(h => h.PreventPush().WithVary("HX-Source", "HX-Target"));
```

## `HtmxLocation`

```csharp
// samples/Htmx4.Sample/Pages/Location.cshtml.cs
Response.Htmx(h => h.Location(new HtmxLocation
{
    Path = "/Location?handler=Landing",
    Target = "#loc-dest",
    Swap = HtmxSwap.InnerHtml,
    Select = "#loc-incoming",
    SelectOob = "#loc-oob-incoming:#loc-oob",
    Values = new Dictionary<string, object> { ["from"] = "hx-location", ["n"] = 1 },
    Headers = new Dictionary<string, string> { ["X-Demo"] = "loc" },
    Transition = true
}));
```

| Property | Type | JSON key |
|---|---|---|
| `Path` (required) | `string` | `path` |
| `Source` | `string?` | `source` |
| `Event` | `string?` | `event` |
| `Target` | `string?` | `target` |
| `Swap` | `string?` | `swap` |
| `Select` | `string?` | `select` |
| `SelectOob` | `string?` | `selectOOB` |
| `Values` | `IDictionary<string, object>?` | `values` |
| `Headers` | `IDictionary<string, string>?` | `headers` |
| `Transition` | `bool?` | `transition` |

`handler` is not supported by htmx 4 and has been removed. `Source`/`Event` serialize into the header but htmx's documentation does not define useful semantics for `event`.

## `HtmxSwap` constants

`InnerHtml` (`innerHTML`), `OuterHtml` (`outerHTML`), `OuterSync` (`outerSync`), `BeforeBegin`/`Before`, `AfterBegin`/`Prepend`, `BeforeEnd`/`Append`, `AfterEnd`/`After`, `Delete`, `None`, `InnerMorph`, `OuterMorph`, `TextContent`. See [Partials and morph](/Htmx.Net/guides/partials-and-morph/).

## `WithVary()`

`WithVary()` appends `HX-Request` to `Vary`; `WithVary("HX-Source", ...)` appends the given names without duplicating existing values. Use it when the same URL returns different content for htmx and non-htmx requests ([Caching](/Htmx.Net/guides/caching/)).

## Polling: `StopPolling()` is obsolete

htmx 4 dropped the `286` convention, so `Response.StopPolling()` is `[Obsolete(error: true)]`. To stop polling, return content that no longer contains the poller (e.g. `hx-swap="outerHTML"` replacing the polling element with a plain element). See `samples/Htmx4.Sample/Pages/Polling.cshtml`.

`HtmxResponseHeaders.Keys` holds the constants; `Keys.All` lists them. See the [response header reference](/Htmx.Net/reference/response-headers/).
