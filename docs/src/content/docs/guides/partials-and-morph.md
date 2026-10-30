---
title: Partials and morph
description: Return multiple fragments with hx-partial and OOB swaps, understand htmx 4 swap ordering, handle 4xx/5xx swaps, and use morph swaps from ASP.NET Core.
---

Real pages rarely update one box at a time. In htmx 4 a single response can update several targets with `<hx-partial>` and out-of-band swaps, error responses swap by default, and morph swaps update the DOM in place without losing focus or typed input. This guide shows each pattern from Razor Pages and MVC.

## `<hx-partial>`

One response can update several targets. Server markup (a Razor partial or a controller action returning HTML):

```html
<hx-partial hx-target="#part-a" hx-swap="innerHTML"><span>partial A</span></hx-partial>
<hx-partial hx-target="#part-b" hx-swap="beforeend"><li>partial B item</li></hx-partial>
<hx-partial hx-target="#part-nested"><span>partial into nested target</span></hx-partial>
<div id="part-oob" hx-swap-oob="true">OOB updated</div>
<div id="part-main-content">main content <span id="part-nested">nested</span></div>
```

- A partial targets via `hx-target` (or `id`) and defaults to `innerHTML`.
- A response containing **only** partials leaves the main target untouched.
- `hx-swap-oob` still works for single-element replacements by id.

In a Razor Page:

```csharp
// samples/Htmx4.Sample/Pages/Partials.cshtml.cs  (OnGetOnly returns only partials)
public IActionResult OnGetOnly() => Content(
    "<hx-partial hx-target=\"#part-a\"><span>only partials</span></hx-partial>", "text/html");
```

```razor
@* _OrderSaved.cshtml : a reusable multi-target response *@
<hx-partial hx-target="#order-count" hx-swap="innerHTML">@Model.Count</hx-partial>
<hx-partial hx-target="#order-list" hx-swap="beforeend"><li>@Model.Latest</li></hx-partial>
<div id="toast" hx-swap-oob="true">Saved!</div>
```

:::note
In prose, write the tag as `<hx-partial>` in backticks; inside code fences nothing needs escaping.
:::

## Ordering (changed from htmx 2)

In htmx 4 the **main content swaps first**; OOB swaps and `<hx-partial>` swaps then run in document order. A partial can therefore target an element that exists only after the main swap (`#part-nested` above). In htmx 2, OOB ran before main. Don't rely on the old order ([htmx 4 facts §8](/Htmx.Net/reference/htmx4-facts/#8-hx-partial-and-oob-ordering)).

## Non-200 responses swap by default

htmx 4 swaps **all** responses except `204` and `304`. So a `422` validation fragment renders where you expect, with no extra setup:

```csharp
// samples/Htmx4.Sample/Controllers/DemoController.cs
[HttpGet("status/{code:int}")]
public IActionResult Status(int code) => code switch
{
    422 => Frag("<div class=\"error\">Validation failed: name is required</div>", 422),
    500 => Frag("<div class=\"error\">Server exploded</div>", 500),
    204 => NoContent(),        // not swapped
    304 => StatusCode(304),    // not swapped
    _   => NotFound()
};
```

```csharp
public IActionResult OnPostValidate(string? name)
{
    if (string.IsNullOrWhiteSpace(name))
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return Partial("_Errors");   // swapped by htmx 4
    }
    return Partial("_Saved");
}
```

Change this behavior with the `noSwap` setting in the [config tag helper](/Htmx.Net/guides/config/) (`noSwap="@(new object[] { 204, 304, 422 })"` stops 422 from swapping) or per element with `hx-status`. To redirect error content elsewhere, use [`Retarget`/`Reswap`](/Htmx.Net/guides/response-headers/).

:::caution
Upgrading from htmx 2? Error responses used to be ignored. If a `4xx`/`5xx` page body now appears in the UI, return `204`, add the status to `noSwap`, or return an intentional fragment.
:::

## Morph swaps

Morphing updates the existing DOM in place, preserving focus, typed input, scroll and CSS transitions:

```csharp
Response.Htmx(h => h.Reswap(HtmxSwap.OuterMorph));   // morph the target itself
Response.Htmx(h => h.Reswap(HtmxSwap.InnerMorph));   // morph the target's children
Response.Htmx(h => h.Reswap(HtmxSwap.OuterSync));    // morph target attributes, then replace children
```

You can also set it client-side: `hx-swap="outerMorph"`. The sample's MVC action:

```csharp
// samples/Htmx4.Sample/Controllers/DemoController.cs  (morph/{kind})
case "outer":
    Response.Htmx(h => h.Reswap(HtmxSwap.OuterMorph));
    return Frag($"<div id=\"morph-outer-box\">{inputs}</div>");   // same id as the target
case "inner":
    Response.Htmx(h => h.Reswap(HtmxSwap.InnerMorph));
    return Frag(inputs);                                          // children only
```

:::caution
With `outerMorph` / `outerHTML` / `outerSync`, the replacement element must keep the same `id` (and the response should contain the element itself). With `innerMorph` return only the children.
:::

Opt elements out of morphing with the config attributes `morphIgnore`, `morphSkip` and `morphSkipChildren` ([Config tag helper](/Htmx.Net/guides/config/)), or in markup:

```html
<input hx-morph-skip /> <!-- element left alone by morph -->
<div hx-morph-skip-children>...</div>
```

### All `HtmxSwap` constants

`InnerHtml`, `OuterHtml`, `OuterSync`, `InnerMorph`, `OuterMorph`, `TextContent`, `Delete`, `None`, `Before`/`BeforeBegin`, `Prepend`/`AfterBegin`, `Append`/`BeforeEnd`, `After`/`AfterEnd`. `None` still processes OOB swaps and response headers. Full table: [Response headers](/Htmx.Net/guides/response-headers/#htmxswap-constants).

---

See the live demos: run `dotnet run --project samples/Htmx4.Sample` and open `/Partials` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Partials.cshtml)), `/Morph` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Morph.cshtml)) and `/ErrorSwaps` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/ErrorSwaps.cshtml)).

Related: [Response headers](/Htmx.Net/guides/response-headers/), [Config tag helper](/Htmx.Net/guides/config/), [htmx 4 facts](/Htmx.Net/reference/htmx4-facts/).
