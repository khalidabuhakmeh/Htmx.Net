---
title: Response headers
description: "Set htmx 4 response headers fluently with Response.Htmx(): push/replace URL, redirect, refresh, retarget, reswap, reselect, HtmxLocation, HtmxSwap constants, Vary and polling."
---

The server can steer htmx from the response: change the target or swap style, update the browser URL, navigate, refresh, or fire events. `Response.Htmx(...)` gives you a fluent, typed way to set those `HX-*` headers from a Razor Page handler or an MVC action.

```csharp
using Htmx;

Response.Htmx(h => h.PushUrl("/products").Retarget("#list").Reswap(HtmxSwap.OuterMorph));
```

`HtmxHttpExtensions.Htmx(this HttpResponse response, Action<HtmxResponseHeaders> action)` runs your lambda against an `HtmxResponseHeaders` writer. Every helper returns the same instance, so calls chain.

:::caution
htmx does not process these headers on `3xx` responses. Return a `2xx` (for example `Content`, `Partial`, `NoContent`) when you rely on them. For navigation use `Redirect`/`Location` headers on a 2xx response rather than `RedirectToPage`.
:::

## Helper reference

| Helper | Header | Notes |
|---|---|---|
| `PushUrl(string)` | `HX-Push-Url` | Push a URL into history. |
| `PreventPush()` | `HX-Push-Url: false` | Suppress an `hx-push-url` push. |
| `ReplaceUrl(string)` | `HX-Replace-Url` | Replace the current URL, no new history entry. |
| `PreventReplace()` | `HX-Replace-Url: false` | Suppress a URL replace. |
| `Redirect(string)` | `HX-Redirect` | Full-page navigation (`location.href`). |
| `Refresh()` | `HX-Refresh: true` | Full page reload. |
| `Retarget(string)` | `HX-Retarget` | CSS selector that overrides the target. |
| `Reswap(string)` | `HX-Reswap` | Swap style (and modifiers), e.g. `HtmxSwap.OuterMorph`. |
| `Reselect(string)` | `HX-Reselect` | CSS selector choosing which part of the response to swap. |
| `Location(string)` | `HX-Location` | htmx-style navigation to a path, no full reload. |
| `Location(HtmxLocation)` | `HX-Location` (JSON) | Navigation with options. |
| `WithTrigger(...)` | `HX-Trigger` | See [Triggering events](/Htmx.Net/guides/triggering-events/). |
| `WithVary()` / `WithVary(params string[])` | `Vary` | See [Caching](/Htmx.Net/guides/caching/). |

`HtmxResponseHeaders.Keys` holds the header-name constants (`Location`, `PushUrl`, `Redirect`, `Refresh`, `ReplaceUrl`, `Reselect`, `Reswap`, `Retarget`, `Trigger`); `Keys.All` returns them as `string[]`. See the [response header reference](/Htmx.Net/reference/response-headers/).

### Examples from the sample

```csharp
// DemoController.cs — resp/{name}
Response.Htmx(h => h.PushUrl("/ResponseHeaders?pushed=1").WithVary());
Response.Htmx(h => h.PreventPush().WithVary("HX-Source", "HX-Target"));
Response.Htmx(h => h.ReplaceUrl("/ResponseHeaders?replaced=1"));
Response.Htmx(h => h.Redirect("/Boosted"));
Response.Htmx(h => h.Refresh());
Response.Htmx(h => h.Retarget("#resp-retarget-target"));
Response.Htmx(h => h.Reswap(HtmxSwap.BeforeEnd));
Response.Htmx(h => h.Reselect("#resp-pick"));
```

### A typical validation flow

```csharp
public IActionResult OnPost(ProductInput input)
{
    if (!ModelState.IsValid)
    {
        // Show the form again in place,
        // even if the button targeted something else.
        Response.Htmx(h => h.Retarget("#product-form").Reswap(HtmxSwap.OuterHtml));
        // 422 bodies swap by default in htmx 4
        return Partial("_Form", input);
    }

    Response.Htmx(h => h.PushUrl("/products").WithTrigger("product-saved"));
    return Partial("_Row", input);
}
```

## `HtmxLocation` and `HX-Location`

`Location("/path")` sends the path as a plain string. Pass an `HtmxLocation` to also send options (serialized as JSON, `null` members omitted). The `Path` is required.

```csharp
// Location.cshtml.cs
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

On the wire the header looks like `HX-Location: {"path":"/Location?handler=Landing","target":"#loc-dest","swap":"innerHTML",...}`.

:::note
`handler` is not a documented htmx 4 option and `HtmxLocation.Handler` was removed. `Source` and `Event` serialize into the header, but htmx's docs do not define useful semantics for `event`; treat them as pass-through ([htmx 4 facts](/Htmx.Net/reference/htmx4-facts/#2-response-headers)).
:::

## `HtmxSwap` constants

String constants for `Reswap(...)`, `HtmxLocation.Swap`, or your own markup.

| Constant | Value |
|---|---|
| `InnerHtml` | `innerHTML` (default) |
| `OuterHtml` | `outerHTML` |
| `OuterSync` | `outerSync` |
| `InnerMorph` / `OuterMorph` | `innerMorph` / `outerMorph` |
| `BeforeBegin` / `Before` | `beforebegin` / `before` |
| `AfterBegin` / `Prepend` | `afterbegin` / `prepend` |
| `BeforeEnd` / `Append` | `beforeend` / `append` |
| `AfterEnd` / `After` | `afterend` / `after` |
| `Delete` | `delete` |
| `None` | `none` (OOB swaps and headers still processed) |
| `TextContent` | `textContent` |

`Reswap` accepts modifiers too: `Response.Htmx(h => h.Reswap(HtmxSwap.InnerHtml + " transition:true"))`. Morph swaps are covered in [Partials and morph](/Htmx.Net/guides/partials-and-morph/).

## `WithVary()`

`WithVary()` appends `HX-Request` to `Vary`; `WithVary("HX-Source", ...)` appends the given names without duplicating existing values. Use it when one URL returns different content to htmx and non-htmx callers. Full discussion in [Caching](/Htmx.Net/guides/caching/).

## Polling: the htmx 4 pattern

`Response.StopPolling()` (`HtmxHttpExtensions.StopPolling`) sent status `286`. htmx 4 no longer honors it, so it is `[Obsolete(error: true)]`. There is no status-code replacement in htmx 4. Instead, a poller keeps going until the response replaces it with content that has no poller:

```html
<div id="poll" hx-get="/demo/poll?n=0" hx-trigger="load delay:1s" hx-swap="outerHTML">0 polling</div>
```

```csharp
// DemoController.cs
[HttpGet("poll")]
public IActionResult Poll(int n = 0, int stop = 3)
{
    n++;
    // no hx-trigger in the reply => polling ends
    if (n >= stop)
        return Frag($"<div id=\"poll\">{n} stopped</div>");

    return Frag($"<div id=\"poll\" hx-get=\"/demo/poll?n={n}&stop={stop}\" hx-trigger=\"load delay:1s\" hx-swap=\"outerHTML\">{n} polling</div>");
}
```

:::caution
A plain `hx-trigger="every 1s"` timer keeps firing while its element stays in the DOM, so stop it by swapping the element away (use `outerHTML`) or removing it.
:::

See also the [migration guide](/Htmx.Net/migration/v1-to-v2/) for removed helpers.

---

See the live demos: run `dotnet run --project samples/Htmx4.Sample` and open `/ResponseHeaders` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/ResponseHeaders.cshtml)), `/Location` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Location.cshtml)) and `/Polling` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Polling.cshtml)).
