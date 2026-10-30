---
title: Migrating from v1 to v2 (htmx 2 → htmx 4)
description: Complete map of removed, renamed, and new htmx.NET APIs when upgrading from v1 (htmx 2) to v2 (htmx 4).
---

:::caution
htmx.NET 2.0 is a **breaking major release** that targets **htmx 4 only**. If you need htmx 2 support, stay on the v1 package line.
Many changes are client-side and come from htmx itself. Read the official [htmx 4 upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4) alongside this page.
:::

## Overview

- Both packages (`Htmx`, `Htmx.TagHelpers`) now target htmx 4 semantics.
- Target frameworks changed to `net8.0;net10.0`. `net6.0` and `net7.0` are no longer supported.
- Removed members ship for one release as `[Obsolete(error: true)]` stubs, so the compiler tells you what to change. Members with no htmx 4 equivalent were removed outright.
- The v1 documentation is still available in the "Previous Version" section.

```xml
<!-- before -->
<TargetFramework>net7.0</TargetFramework>
<!-- after -->
<TargetFramework>net8.0</TargetFramework>
```

## Server API (`Htmx` package)

### Request headers (`HtmxRequestHeaders`)

| v1 | v2 | Notes |
|----|----|-------|
| `Trigger` (`HX-Trigger`) | `Source` / `SourceElement` | Compile-error stub. `HX-Source` uses `tag#id` format. |
| `TriggerName` (`HX-Trigger-Name`) | `Source` / `SourceElement` | Compile-error stub. Header removed in htmx 4. |
| `Keys.Trigger`, `Keys.TriggerName` constants | `Keys.Source` (`HX-Source`) | Constants removed. |
| *(none)* | `RequestType`, `Keys.RequestType` | New `HX-Request-Type` header: `Full` or `Partial`. |
| `Target` (element id) | `Target` (`tag#id`) + `TargetElement` | Value is now `tag#id`. Use `TargetElement.Id` for the id. |

New helper type `HtmxElementReference(string TagName, string? Id, string Raw)` with `DecodedId` and `TryParse(string?, out HtmxElementReference)`.
New enum `HtmxRequestType` (`Full`, `Partial`). `HtmxRequestHeaders.Keys.All` (used for CORS `Access-Control-Allow/Expose-Headers`) now lists the htmx 4 request headers.

```csharp
// before
var id = Request.Htmx().Trigger;          // "save-button"
var name = Request.Htmx().TriggerName;
var target = Request.Htmx().Target;       // "results"

// after
var src = Request.Htmx().SourceElement;   // HtmxElementReference? { TagName = "button", Id = "save-button" }
var id = src?.Id;
var raw = Request.Htmx().Source;          // "button#save-button"
var targetId = Request.Htmx().TargetElement?.Id; // Target is now "div#results"
```

```csharp
// new: distinguish partial and full requests
if (Request.IsHtmxPartial()) return PartialView("_List", model);
if (Request.IsHtmxFullRequest()) { /* render full page */ }
var type = Request.Htmx().RequestType; // HtmxRequestType.Partial
```

`HtmxHttpExtensions` gained `IsHtmxPartial(this HttpRequest?)` and `IsHtmxFullRequest(this HttpRequest?)`.

### Response headers (`HtmxResponseHeaders`)

| v1 | v2 | Notes |
|----|----|-------|
| `HtmxTriggerTiming` enum | *removed* (compile-error stub) | `HX-Trigger-After-Swap` / `-After-Settle` no longer exist in htmx 4. |
| `WithTrigger(name, detail, timing)` | `WithTrigger(name, detail)` | The timing overload is a compile-error stub. |
| `Trigger(string)` | `WithTrigger(name, detail)` | Removed. |
| `TriggerAfterSwap(string)` | `WithTrigger(name, detail)` | Removed. Listen to `htmx:after:swap` or similar events client-side if you need timing. |
| `TriggerAfterSettle(string)` | `WithTrigger(name, detail)` | Removed. |
| `Keys.TriggerAfterSwap`, `Keys.TriggerAfterSettle` | *removed* | htmx 4 ignores these headers. |
| `Push(string)` | `PushUrl(string)` | Removed. `HX-Push` was deprecated in v1. |
| `StopPolling()` | return content without the polling element | Compile-error stub. htmx 4 dropped status `286`. |
| `WithVary()` | `WithVary()` and `WithVary(params string[] headerNames)` | The parameterless form adds the htmx 4 request headers. |

```csharp
// before
Response.Htmx(h => h
    .WithTrigger("saved", new { id = 5 }, HtmxTriggerTiming.AfterSwap)
    .Push("/items/5"));

// after
Response.Htmx(h => h
    .WithTrigger("saved", new { id = 5 })
    .PushUrl("/items/5"));
```

```csharp
// before: stop an hx-trigger="every 2s" poller
Response.Htmx(h => h.StopPolling());
return Content("Done");

// after: respond with markup that no longer contains the polling element
// (replace the element that carries hx-trigger="every 2s")
return Content("<div id=\"status\">Done</div>", "text/html");
```

```csharp
// before / after
Response.Htmx(h => h.WithVary());
Response.Htmx(h => h.WithVary(HtmxRequestHeaders.Keys.Request, HtmxRequestHeaders.Keys.RequestType));
```

### `HtmxLocation`

| v1 | v2 | Notes |
|----|----|-------|
| `Handler` | *removed* | `htmx.ajax` has no `handler` option in htmx 4. |
| `Push` | *removed* | Use `HX-Push-Url` via `PushUrl(...)`. |
| `Replace` | *removed* | Use `HX-Replace-Url` via `ReplaceUrl(...)`. |
| *(none)* | `SelectOob` (`selectOOB`) | New. |
| *(none)* | `Transition` (`transition`) | New, view transition for the swap. |

```csharp
// before
new HtmxLocation("/page") { Target = "#main", Push = "false", Handler = "onLoad" };
// after
new HtmxLocation("/page") { Target = "#main", SelectOob = "#toast", Transition = true };
```

### `HtmxSwap`

New constants: `OuterSync` (`outerSync`), `InnerMorph` (`innerMorph`), `OuterMorph` (`outerMorph`), `TextContent` (`textContent`), and the aliases `Before`, `Prepend`, `Append`, `After`. Existing constants are unchanged.

```csharp
// before
Response.Htmx(h => h.Reswap(HtmxSwap.InnerHtml));
// after (new options)
Response.Htmx(h => h.Reswap(HtmxSwap.OuterMorph));
Response.Htmx(h => h.Reswap(HtmxSwap.Append));
```

`HtmxResponseHeaders.Retarget` was reordered in source but is unchanged in behavior.

## TagHelpers (`Htmx.TagHelpers`)

### `<htmx-config>` meta tag helper (`HtmxConfigMetaTagHelper`)

Unset keys are no longer serialized (bug fix); only the values you set appear in the `htmx-config` meta tag.

**Renamed**

| v1 attribute / property | v2 attribute / property | Before → after |
|---|---|---|
| `historyEnabled` / `HistoryEnabled` (`bool?`) | `history` / `History` (`string?`) | `historyEnabled="false"` → `history="false"` |
| `defaultSwapStyle` / `DefaultSwapStyle` | `defaultSwap` / `DefaultSwap` | `defaultSwapStyle="outerHTML"` → `defaultSwap="outerHTML"` |
| `includeIndicatorStyles` / `IncludeIndicatorStyles` | `includeIndicatorCSS` / `IncludeIndicatorCSS` | `includeIndicatorStyles="false"` → `includeIndicatorCSS="false"` |
| `timeout` / `Timeout` | `defaultTimeout` / `DefaultTimeout` | `timeout="5000"` → `defaultTimeout="5000"` |
| `globalViewTransitions` / `GlobalViewTransitions` | `transitions` / `Transitions` | `globalViewTransitions="true"` → `transitions="true"` |

**Removed (no htmx 4 equivalent, or replaced by another mechanism)**

| v1 attribute / property | Notes |
|---|---|
| `historyCacheSize`, `refreshOnHistoryMiss`, `historyRestoreAsHxRequest` | htmx 4 history no longer uses a localStorage snapshot cache. |
| `defaultSwapDelay` | Use `hx-swap="... swap:100ms"` per element. |
| `settlingClass`, `swappingClass`, `addedClass` | htmx 4 has no settle/swap CSS class phases. |
| `selfRequestsOnly` | Replaced by `mode` (default `same-origin`). |
| `allowScriptTags`, `allowEval`, `useTemplateFragments` | Removed. |
| `wsReconnectDelay`, `wsBinaryType` | WebSockets are an extension in htmx 4. |
| `disableSelector`, `disableInheritance` | Inheritance is now explicit (`:inherited`); see `implicitInheritance` below. |
| `withCredentials`, `scrollBehavior`, `getCacheBusterParam` | Removed or moved to per-request config. |
| `attributesToSettle`, `methodsThatUseUrlParams` | Removed. |
| `ignoreTitle`, `scrollIntoViewOnBoost`, `reportValidityOfForms` | Removed (htmx 4 handles these per-swap or by default). |
| `triggerSpecsCache` and types `HtmxTriggerSpecificationCache`, `HtmxTriggerSpecification` | Removed. |
| `inlineStyleNonce` | Removed. (`inlineScriptNonce` is kept.) |
| `responseHandling` and type `HtmxResponseHandling` | Replaced by default swap-on-non-200 behavior plus `noSwap`. |
| `allowNestedOobSwaps` | Removed. |

**Unchanged:** `defaultFocusScroll`, `defaultSettleDelay`, `indicatorClass`, `requestClass`, `inlineScriptNonce`, `includeAspNetAntiforgeryToken`.
**Type change:** `DefaultFocusScroll` is now `bool?` (was `bool`); unset keys are omitted.

**New**

| Attribute | Purpose |
|---|---|
| `implicitInheritance` | Restore htmx 2 implicit attribute inheritance (migration aid). |
| `noSwap` | Status codes that are not swapped (e.g. `204,304`). |
| `prefix` | Custom attribute prefix (replaces `data-hx-` style configuration). |
| `extensions` | Comma-separated extension names to load. |
| `morphIgnore`, `morphSkip`, `morphSkipChildren`, `morphScanLimit` | Morph swap tuning. |
| `mode` | Request mode: `same-origin` (default), `cors`, etc. |
| `metaCharacter` | Modifier character for attribute names. |
| `logAll` | Log all htmx events. |
| `allowEmptySwapAfterOOB` | Allow empty main swap after OOB. |

```html
<!-- before -->
<htmx-config historyEnabled="false" defaultSwapStyle="outerHTML" timeout="5000"
             globalViewTransitions="true" selfRequestsOnly="true" includeAspNetAntiforgeryToken="true" />

<!-- after -->
<htmx-config history="false" defaultSwap="outerHTML" defaultTimeout="5000"
             transitions="true" mode="same-origin" implicitInheritance="false"
             includeAspNetAntiforgeryToken="true" />
```

### URL tag helper (`HtmxUrlTagHelper`)

- **Breaking: binding attributes renamed `hx-*` → `hxn-*`** (htmx + .NET). htmx 4 added a native `hx-action` attribute, and `hx-*` is htmx's namespace, so the helper's server-side URL-binding attributes moved: `hx-action`→`hxn-action`, `hx-controller`→`hxn-controller`, `hx-area`→`hxn-area`, `hx-page`→`hxn-page`, `hx-page-handler`→`hxn-page-handler`, `hx-route`→`hxn-route`, `hx-route-*`→`hxn-route-*`, `hx-host`→`hxn-host`, `hx-protocol`→`hxn-protocol`, `hx-fragment`→`hxn-fragment`, `hx-all-route-data`→`hxn-all-route-data`. C# property names (`Action`, `Controller`, `Page`, …) are unchanged. Verbs (`hx-get`, `hx-post`, …) and `hx-target` stay as-is.

```html
<!-- before -->
<button hx-get hx-page="/Items" hx-page-handler="List">Load</button>
<!-- after -->
<button hx-get hxn-page="/Items" hxn-page-handler="List">Load</button>
```

- A **valueless `hx-get` is no longer rewritten.** In htmx 4 it is valid and means "request the current URL".
- URL generation now keys off the routing attributes: `hxn-page`, `hxn-action`, `hxn-controller`, `hxn-route`.
- The helper supports `:inherited` and `data-hx-` spellings (for example `data-hx-post`, `hx-get:inherited`).

```html
<!-- before (v1): bare hx-get was rewritten from hxn-page -->
<button hx-get hxn-page="/Items">Load</button>
<!-- after: the routing attribute drives generation; the verb gets the URL -->
<button hx-get hxn-page="/Items">Load</button>   <!-- same markup still works when a routing attribute is present -->
<button hx-get>Reload current URL</button>     <!-- valueless, left untouched -->
<button data-hx-post hxn-controller="Items" hxn-action="Save">Save</button>
```

:::caution
`hx-get:inherited` does **not** make descendant elements fire requests in htmx 4. `:inherited` on request verbs is inert for triggering. The helper will still write a URL into that spelling, but it is not a working trigger pattern. Put a direct verb attribute on each element, or use `hx-boost:inherited` for links and forms.
:::

```html
<!-- before (htmx 2: verb inherited by children) -->
<div hx-get="/items" hx-trigger="click"> ... </div>

<!-- after: direct verb per element -->
<button hx-get hxn-page="/Items">Load</button>
<!-- or boost links/forms in a container -->
<nav hx-boost:inherited="true"> <a href="/items">Items</a> </nav>
```

### Headers tag helper (`HtmxHeadersTagHelper`)

New attribute `hx-headers-inherited` (property `Inherited`) emits `hx-headers:inherited`. In htmx 4, inheritance is explicit.

```html
<!-- before: children inherited hx-headers implicitly -->
<div hx-headers-X-Tenant="acme"> ... </div>
<!-- after -->
<div hx-headers-X-Tenant="acme" hx-headers-inherited="true"> ... </div>
<!-- renders: hx-headers:inherited='{"X-Tenant":"acme"}' -->
```

### Antiforgery script

The JavaScript was rewritten for htmx 4's fetch pipeline. **No consumer code changes**: keep using `@Html.HtmxAntiforgeryScript()`, `MapHtmxAntiforgeryScript()`, or `HtmxSnippets`, and re-include the new script (clear caches). Behavior to know:

- The token is injected only for same-origin requests (checked against `document.baseURI`).
- There is **no automatic token refresh**. After a mid-session token rotation, do a full reload (for example `HX-Refresh`).
- `DELETE` requests must use the header mode (htmx 4 serializes DELETE form data to the query string).
- A residual risk remains for `mode: "cors"` configurations that follow redirects; keep `mode` at `same-origin`.

```html
<!-- before and after: unchanged -->
<htmx-config includeAspNetAntiforgeryToken="true" />
@Html.HtmxAntiforgeryScript()
```

## Client-side htmx 4 changes that affect your Razor markup

These come from htmx 4 itself. See the [official upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4).

| Change | Before → after |
|---|---|
| Explicit inheritance with `:inherited`; `hx-disinherit` / `hx-inherit` removed | `<div hx-target="#out">` → `<div hx-target:inherited="#out">` |
| Event renames | `htmx:beforeRequest` → `htmx:before:request`; `htmx:afterSwap` → `htmx:after:swap`; `hx-on::before-request` → `hx-on::before:request` |
| `hx-delete` no longer includes form inputs | `<form hx-delete="/x">` → `<form hx-delete="/x" hx-include="this">` |
| Non-200 responses swap by default (204 and 304 excepted) | Handle 4xx/5xx markup, or set `noSwap` / `hx-status:404="..."` |
| OOB swaps happen after the main swap | Do not depend on OOB content existing during the main swap |
| New `<hx-partial>` and morph swaps | `hx-swap="outerMorph"` or `HtmxSwap.OuterMorph` |
| Removed attributes: `hx-vars`, `hx-params`, `hx-prompt` (now an extension) | `hx-vars="a:1"` → `hx-vals='{"a":1}'` |
| `data-hx-` prefix still works | `data-hx-get` is valid |

```html
<!-- before -->
<button hx-delete="/items/5" hx-confirm="Sure?">Delete</button>
<form hx-delete="/items/5"> <input name="reason"> </form>
<script>document.body.addEventListener("htmx:afterSwap", e => {});</script>

<!-- after -->
<button hx-delete="/items/5" hx-confirm="Sure?">Delete</button>
<form hx-delete="/items/5" hx-include="this"> <input name="reason"> </form>
<script>document.body.addEventListener("htmx:after:swap", e => {});</script>
```

## Coverage checklist

Every public member removed or renamed between `v1-final` and the current source is listed above:
`HtmxRequestHeaders.Trigger`, `TriggerName`, `Keys.Trigger`, `Keys.TriggerName`; `HtmxResponseHeaders.Push`, `Trigger`, `TriggerAfterSettle`, `TriggerAfterSwap`, `WithTrigger(…, HtmxTriggerTiming)`, `Keys.TriggerAfterSettle`, `Keys.TriggerAfterSwap`, `StopPolling`; `HtmxTriggerTiming`; `HtmxLocation.Handler`, `Push`, `Replace`; and all `HtmxConfigMetaTagHelper` properties plus `HtmxTriggerSpecification`, `HtmxTriggerSpecificationCache`, and `HtmxResponseHandling`.
