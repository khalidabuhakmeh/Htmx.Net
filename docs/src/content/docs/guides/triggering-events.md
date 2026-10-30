---
title: Triggering events
description: Fire client-side events from the server with WithTrigger and the single HX-Trigger header.
---

```csharp
Response.Htmx(h => h.WithTrigger("saved"));
Response.Htmx(h => h.WithTrigger("demo-scalar", "plain-value"));          // event.detail.value
Response.Htmx(h => h.WithTrigger("demo-detail", new { id = 42, message = "hello" }));
Response.Htmx(h => h.WithTrigger("a").WithTrigger("b", new { n = 2 }));   // multiple
```

From `samples/Htmx4.Sample/Controllers/DemoController.cs` (`/demo/trigger/{kind}`).

| Call | Resulting header |
|---|---|
| `WithTrigger("name")` | `HX-Trigger: name` (plain) |
| `WithTrigger("name", detail)` | `HX-Trigger: {"name": <detail JSON>}` |
| several calls | one JSON object with all events |

## Behavior

- **One header.** htmx 4 only supports `HX-Trigger`. `HX-Trigger-After-Swap` / `HX-Trigger-After-Settle` were removed.
- **Merge.** If `HX-Trigger` is already set (comma-separated names or JSON), new events are merged into it.
- **First wins.** Calling `WithTrigger` twice with the same event name keeps the first detail.
- **Detail forms.** A scalar becomes `event.detail.value`; an object's properties are copied onto the event detail; an object may include `target` to dispatch on another element.

:::caution[Removed]
`WithTrigger(name, detail, HtmxTriggerTiming)` and `HtmxTriggerTiming` (`Default`, `AfterSettle`, `AfterSwap`) are `[Obsolete(error: true)]`. Drop the timing argument; listen for the event after the swap in client code if needed.
:::

Listen client-side:

```html
<div hx-on:demo-detail="console.log(event.detail.id)"></div>
```
