---
title: Partials and morph
description: Return multiple fragments with hx-partial, understand htmx 4 swap ordering, and use morph swaps from the server.
---

## `<hx-partial>`

One response can update several targets. From `samples/Htmx4.Sample/Controllers/DemoController.cs`:

```html
<hx-partial hx-target="#part-a" hx-swap="innerHTML"><span>partial A</span></hx-partial>
<hx-partial hx-target="#part-b" hx-swap="beforeend"><li>partial B item</li></hx-partial>
<div id="part-oob" hx-swap-oob="true">OOB updated</div>
<div id="part-main-content">main content <span id="part-nested">nested</span></div>
```

- A partial targets via `hx-target` (or `id`) and defaults to `innerHTML`.
- A response containing **only** partials leaves the main target untouched.

## Ordering (changed from htmx 2)

In htmx 4 the **main content swaps first**; OOB swaps and `<hx-partial>`s run afterward in document order. A partial may therefore target an element that only exists after the main swap (`#part-nested` above). In htmx 2 OOB ran before main.

## Non-200 responses

htmx 4 swaps all responses except `204` and `304` by default, so 422/500 fragments render. Change this with the `noSwap` [config](/Htmx.Net/guides/config/) or `hx-status` (`samples/Htmx4.Sample/Pages/ErrorSwaps.cshtml`).

## Morph swaps

Morphing updates the existing DOM in place, preserving focus, typed input and state:

```csharp
Response.Htmx(h => h.Reswap(HtmxSwap.OuterMorph));   // morph the target itself
Response.Htmx(h => h.Reswap(HtmxSwap.InnerMorph));   // morph the target's children
Response.Htmx(h => h.Reswap(HtmxSwap.OuterSync));    // morph target attributes, then replace children
```

Opt elements out with `morphSkip` / `morphSkipChildren` / `morphIgnore` in [config](/Htmx.Net/guides/config/). With `outerMorph`/`outerHTML`/`outerSync`, the replacement element must keep the same `id`. Demo: `samples/Htmx4.Sample/Pages/Morph.cshtml`.

Other `HtmxSwap` constants: `None` (only OOB and headers processed), `Delete`, `TextContent`, `Before`/`BeforeBegin`, `Prepend`/`AfterBegin`, `Append`/`BeforeEnd`, `After`/`AfterEnd`.
