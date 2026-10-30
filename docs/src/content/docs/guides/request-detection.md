---
title: Request detection
description: Detect htmx requests, boosted navigation and partial vs full requests, and read htmx 4 request headers (HX-Source, HX-Target, HX-Request-Type) from ASP.NET Core.
---

When one endpoint serves both a normal browser navigation and an htmx request, you need to know which one you are answering. Reach for request detection to return a fragment instead of a full page, to branch on boosted navigation, or to find out which element triggered the request.

Everything lives in the `Htmx` package (`using Htmx;`) as extension methods on `HttpRequest` plus the `HtmxRequestHeaders` and `HtmxElementReference` types.

## Extension methods

All methods are on `Htmx.HtmxHttpExtensions` and accept a nullable `HttpRequest`.

| Method | True when |
|---|---|
| `IsHtmx()` | `HX-Request: true` is present. |
| `IsHtmx(out HtmxRequestHeaders? values)` | Same, and `values` receives the parsed headers (`null` when not htmx). |
| `IsHtmxBoosted()` | An htmx request with `HX-Boosted: true` (a link or form enhanced by `hx-boost`). |
| `IsHtmxNonBoosted()` / `IsHtmxNonBoosted(out values)` | An htmx request that is **not** boosted (a plain `hx-get`, `hx-post`, ...). |
| `IsHtmxHistoryRestoreRequest()` | `HX-History-Restore-Request: true` (back/forward re-fetch). |
| `IsHtmxPartial()` | htmx request with `HX-Request-Type: partial`. |
| `IsHtmxFullRequest()` | htmx request with `HX-Request-Type: full`. |

### Razor Pages

```csharp
using Htmx;

public class ProductsModel : PageModel
{
    public IActionResult OnGet()
    {
        // Return only the list for htmx, the whole page otherwise.
        return Request.IsHtmx() ? Partial("_ProductList", this) : Page();
    }
}
```

### MVC controllers

```csharp
// samples/Htmx4.Sample/Controllers/DemoController.cs
[HttpGet("request-headers")]
public IActionResult RequestHeadersEcho()
{
    var isHtmx = Request.IsHtmx(out var h);
    var isPartial = Request.IsHtmxPartial();
    var isFull = Request.IsHtmxFullRequest();
    var boosted = Request.IsHtmxBoosted();
    var nonBoosted = Request.IsHtmxNonBoosted();
    var restore = Request.IsHtmxHistoryRestoreRequest();
    // ... h.Source, h.SourceElement, h.Target, ...
}
```

:::tip[Pair with `Vary`]
If one URL returns different markup for htmx and non-htmx requests, tell caches. See [Caching](/Htmx.Net/guides/caching/).
:::

## What htmx sends

A button click like this:

```html
<button id="save" hx-post="/items" hx-target="#list">Save</button>
```

produces request headers similar to:

```http
HX-Request: true
HX-Source: button#save
HX-Target: div#list
HX-Request-Type: partial
HX-Current-URL: https://localhost:5001/items
```

`HX-Target` is only sent when a target is set. `HX-Boosted: true` is added for boosted links/forms, `HX-History-Restore-Request: true` for history re-fetches, and `HX-Prompt` carries the answer to an `hx-prompt` (when that extension is in use).

## `HtmxRequestHeaders`

Create it with `new HtmxRequestHeaders(HttpRequest request)` or receive it from `IsHtmx(out var h)`.

| Property | Header | Type / notes |
|---|---|---|
| `Source` | `HX-Source` | `string`, raw `tag#id`. Replaces htmx 2's *request* `HX-Trigger`. |
| `SourceElement` | `HX-Source` | `HtmxElementReference?`, parsed. |
| `Target` | `HX-Target` | `string`, raw `tag#id`; only sent when a target is set. |
| `TargetElement` | `HX-Target` | `HtmxElementReference?`, parsed. |
| `RequestType` | `HX-Request-Type` | `HtmxRequestType`: `Unknown = 0`, `Full = 1`, `Partial = 2`. |
| `CurrentUrl` | `HX-Current-URL` | `string`, the browser URL at request time. |
| `Boosted` | `HX-Boosted` | `bool` |
| `HistoryRestoreRequest` | `HX-History-Restore-Request` | `bool` |
| `Prompt` | `HX-Prompt` | `string`, only with `hx-prompt`. |
| `Trigger`, `TriggerName` | n/a | **Removed**, see below. |

`HtmxRequestHeaders.Keys` exposes the header-name constants (`Boosted`, `CurrentUrl`, `HistoryRestoreRequest`, `Prompt`, `Request`, `RequestType`, `Source`, `Target`) and `Keys.All` returns them as `string[]`. See the [request header reference](/Htmx.Net/reference/request-headers/).

:::caution[Removed in htmx 4]
`HtmxRequestHeaders.Trigger` and `TriggerName` (htmx 2's `HX-Trigger` / `HX-Trigger-Name` request headers) are `[Obsolete(error: true)]` and throw `NotSupportedException`. Use `Source` / `SourceElement`. See the [migration guide](/Htmx.Net/migration/v1-to-v2/).
:::

## `HtmxElementReference`

htmx 4 sends elements as `<tag>#<id>`, or just `<tag>` when the element has no id (tag lowercase, id URI-encoded).

| Member | Meaning |
|---|---|
| `TagName` | e.g. `button` |
| `Id` | the id exactly as sent (still encoded), or `null` |
| `DecodedId` | URI-decoded id |
| `Raw` | the original header value |
| `static bool TryParse(string? value, out HtmxElementReference result)` | parse a header value yourself |
| constructor `(string TagName, string? Id, string Raw)` | it is a record; use it in tests |

```csharp
if (Request.IsHtmx(out var h) && h!.SourceElement is { TagName: "button", Id: "save" })
{
    // the request came from <button id="save">
}

if (HtmxElementReference.TryParse("input#my%20field", out var el))
{
    // el.TagName == "input", el.Id == "my%20field", el.DecodedId == "my field"
}
```

## `HX-Request-Type` and boosted navigation

`HX-Request-Type` is `full` when the request targets `body` or uses `hx-select`, and `partial` otherwise. That is the cleanest way to choose between a full layout and a fragment. Boosted links (`hx-boost`) typically swap the whole `body`, so they arrive as `full` and `IsHtmxBoosted()` is `true`:

```csharp
// samples/Htmx4.Sample/Pages/Boosted.cshtml.cs
public void OnGet() => RequestType = Request.IsHtmx(out var h) ? h!.RequestType.ToString() : "n/a";
```

```razor
boosted = @Request.IsHtmxBoosted()
htmx    = @Request.IsHtmx()
plain   = @Request.IsHtmxNonBoosted()
```

:::note
A boosted request is still an htmx request, so `IsHtmx()` is `true` for it. Use `IsHtmxNonBoosted()` when you specifically want the non-boosted ones.
:::

## Polling

Polling is just a repeating request, so detect it like any other (`Request.IsHtmx()`). `Response.StopPolling()` (status `286`) is gone in htmx 4 and is `[Obsolete(error: true)]`. To stop, return content that no longer contains the polling element. See [Response headers: stopping polling](/Htmx.Net/guides/response-headers/#polling-the-htmx-4-pattern).

---

See the live demos: run `dotnet run --project samples/Htmx4.Sample` and open `/RequestHeaders` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/RequestHeaders.cshtml)) and `/Boosted` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Boosted.cshtml)).

Related: [Response headers](/Htmx.Net/guides/response-headers/), [Caching](/Htmx.Net/guides/caching/), [htmx 4 facts](/Htmx.Net/reference/htmx4-facts/).
