---
title: Triggering events
description: Fire client-side events from ASP.NET Core with WithTrigger and the single htmx 4 HX-Trigger header, including detail payloads, merging and listening.
---

The server can tell the browser "something happened" by firing a DOM event after the swap: refresh a counter, close a dialog, show a toast. `WithTrigger` writes the `HX-Trigger` response header for you, including JSON detail payloads.

```csharp
using Htmx;

Response.Htmx(h => h.WithTrigger("saved"));
// event.detail.value
Response.Htmx(h => h.WithTrigger("demo-scalar", "plain-value"));
Response.Htmx(h => h.WithTrigger("demo-detail", new { id = 42, message = "hello" }));
// multiple events
Response.Htmx(h => h.WithTrigger("a").WithTrigger("b", new { n = 2 }));
```

Signature: `WithTrigger(string eventName, object? detail = null)`. Detail is serialized with `System.Text.Json`.

## What the server sends

| Call | Resulting header |
|---|---|
| `WithTrigger("saved")` | `HX-Trigger: saved` |
| `WithTrigger("demo-scalar", "plain-value")` | `HX-Trigger: {"demo-scalar":"plain-value"}` |
| `WithTrigger("demo-detail", new { id = 42, message = "hello" })` | `HX-Trigger: {"demo-detail":{"id":42,"message":"hello"}}` |
| `WithTrigger("a").WithTrigger("b", new { n = 2 })` | `HX-Trigger: {"a":"","b":{"n":2}}` (one JSON object; events without detail serialize as `""`) |

A single event with no detail collapses to the plain name; anything else becomes one JSON object.

## What htmx does with it

htmx 4 documents event names as top-level JSON keys:

- A **scalar** detail is exposed as `event.detail.value`.
- An **object** detail has its properties copied onto `event.detail`.
- An object may include a `target` key to dispatch the event on another element.

Listen with `hx-on` or a normal listener:

```html
<div hx-on:demo-detail="console.log(event.detail.id, event.detail.message)"></div>

<script>
  document.body.addEventListener("saved", () => alert("Saved!"));
</script>
```

A common pattern is to have an element reload itself when an event fires:

```html
<div hx-get="/cart/summary" hx-trigger="item-added from:body"></div>
```

```csharp
public IActionResult OnPostAdd(int id)
{
    cart.Add(id);
    Response.Htmx(h => h.WithTrigger("item-added", new { id }));
    // 204 is not swapped, headers are still processed
    return NoContent();
}
```

## Behavior to know

- **One header.** htmx 4 supports only `HX-Trigger`. `HX-Trigger-After-Swap` and `HX-Trigger-After-Settle` were removed.
- **Merge.** If `HX-Trigger` is already set (comma-separated names or a JSON object), the existing events are merged in after the ones you set with `WithTrigger`.
- **First wins.** Calling `WithTrigger` twice with the same event name keeps the first detail.

```csharp
// DemoController.cs — merge cases
Response.Headers["HX-Trigger"] = "demo-pre-a, demo-pre-b";
// names + JSON merged
Response.Htmx(h => h.WithTrigger("demo-added", new { n = 1 }));

Response.Htmx(h => h.WithTrigger("demo-dup", new { v = "first" }).WithTrigger("demo-dup", new { v = "second" }));
// -> demo-dup detail is { v: "first" }
```

:::caution[Removed in htmx 4]
`WithTrigger(name, detail, HtmxTriggerTiming)` and the `HtmxTriggerTiming` enum (`Default = 0`, `AfterSettle = 1`, `AfterSwap = 2`) are `[Obsolete(error: true)]` because the after-swap/after-settle headers no longer exist. Drop the timing argument; if you need to run code after the swap, listen for `htmx:after:swap` / `htmx:after:settle` client-side.
:::

:::note
Response headers are ignored on `3xx` responses. Use a `2xx` response such as `NoContent()` or a partial.
:::

---

See the live demo: run `dotnet run --project samples/Htmx4.Sample` and open `/Triggers` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Triggers.cshtml)).

Related: [Response headers](/Htmx.Net/guides/response-headers/), [htmx 4 facts: HX-Trigger](/Htmx.Net/reference/htmx4-facts/#hx-trigger-json-detail), [migration guide](/Htmx.Net/migration/v1-to-v2/).
