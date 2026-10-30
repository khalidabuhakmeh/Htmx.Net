---
title: Request detection
description: Detect htmx requests and read htmx 4 request headers with IsHtmx, HtmxRequestHeaders and HtmxElementReference.
---

Extension methods on `HttpRequest` live in `Htmx.HtmxHttpExtensions` (`using Htmx;`).

| Method | True when |
|---|---|
| `IsHtmx()` / `IsHtmx(out HtmxRequestHeaders? values)` | `HX-Request: true` is present. The `out` overload gives the parsed headers (`null` when not htmx). |
| `IsHtmxBoosted()` | An htmx request with `HX-Boosted: true` (link/form enhanced by `hx-boost`). |
| `IsHtmxNonBoosted()` (+ `out` overload) | An htmx request that is *not* boosted. |
| `IsHtmxHistoryRestoreRequest()` | `HX-History-Restore-Request: true` (back/forward navigation re-fetch). |
| `IsHtmxPartial()` | htmx request with `HX-Request-Type: partial`. |
| `IsHtmxFullRequest()` | htmx request with `HX-Request-Type: full`. |

```csharp
// samples/Htmx4.Sample/Controllers/DemoController.cs
var isHtmx = Request.IsHtmx(out var h);
var isPartial = Request.IsHtmxPartial();
var isFull = Request.IsHtmxFullRequest();
var boosted = Request.IsHtmxBoosted();
var nonBoosted = Request.IsHtmxNonBoosted();
var restore = Request.IsHtmxHistoryRestoreRequest();
```

## `HtmxRequestHeaders`

Constructed from a request (`new HtmxRequestHeaders(request)`) or obtained from `IsHtmx(out var h)`.

| Property | Header | Notes |
|---|---|---|
| `Source` | `HX-Source` | Raw `tag#id` string (replaces htmx 2's request `HX-Trigger`). |
| `SourceElement` | `HX-Source` | Parsed `HtmxElementReference?`. |
| `Target` | `HX-Target` | Raw `tag#id` string; only sent when a target is set. |
| `TargetElement` | `HX-Target` | Parsed `HtmxElementReference?`. |
| `RequestType` | `HX-Request-Type` | `HtmxRequestType.Full`, `Partial`, or `Unknown`. |
| `CurrentUrl` | `HX-Current-URL` | Browser URL at request time. |
| `Boosted` | `HX-Boosted` | `bool`. |
| `HistoryRestoreRequest` | `HX-History-Restore-Request` | `bool`. |
| `Prompt` | `HX-Prompt` | Only sent by the `hx-prompt` extension. |

`HtmxRequestHeaders.Keys` holds the header-name constants and `Keys.All` lists them. See the [request header reference](/Htmx.Net/reference/request-headers/).

:::caution[Removed in htmx 4]
`HtmxRequestHeaders.Trigger` and `TriggerName` (htmx 2 `HX-Trigger` / `HX-Trigger-Name` request headers) are `[Obsolete(error: true)]` and throw `NotSupportedException`. Use `Source` / `SourceElement`.
:::

## `HtmxElementReference`

htmx 4 sends elements as `<tag>#<id>` (just `<tag>` if the element has no id; tag lowercase, id URI-encoded).

| Member | Meaning |
|---|---|
| `TagName` | e.g. `button` |
| `Id` | the id as sent (encoded), or `null` |
| `DecodedId` | URI-decoded id |
| `Raw` | the original header value |
| `HtmxElementReference.TryParse(string?, out HtmxElementReference)` | parse a header value |

```csharp
if (Request.IsHtmx(out var h) && h!.SourceElement is { TagName: "button", Id: "save" })
{
    // request came from <button id="save">
}
```

## `HX-Request-Type`

`full` is sent when the target is `body` or `hx-select` is used; `partial` otherwise. Use it to choose between a full layout and a fragment. If a URL's representation depends on it, add it to `Vary` (see [Caching](/Htmx.Net/guides/caching/)).
