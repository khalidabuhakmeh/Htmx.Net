---
title: URL tag helper
description: Generate hx-get/hx-post/hx-put/hx-patch/hx-delete URLs from Razor Pages, MVC controllers, areas and named routes with HtmxUrlTagHelper, including htmx 4 explicit inheritance.
---

Hard-coded URLs like `hx-get="/products/7"` break when routes change. `HtmxUrlTagHelper` brings ASP.NET Core's routing to htmx: you give it a page, controller/action, area or named route and it writes the generated URL into the htmx verb attribute you put on the element. It behaves like the built-in anchor tag helper, but for `hx-get`, `hx-post`, `hx-put`, `hx-patch` and `hx-delete`.

:::note
The binding attributes use the `hxn-` prefix (htmx + .NET), e.g. `hxn-page`, `hxn-controller`, `hxn-action`. `hx-*` is htmx's own namespace and htmx 4 added a native `hx-action` attribute, so the library no longer uses `hx-*` for its server-side URL binding. The verbs (`hx-get`, `hx-post`, …) and `hx-target` are still plain htmx.
:::

```razor
<!-- samples/Htmx4.Sample/Pages/UrlHelper.cshtml -->
<button hx-get hxn-page="/UrlHelper" hxn-page-handler="Fragment" hx-target="#url-result-box">Page</button>
<button hx-get hxn-controller="Demo" hxn-action="Echo" hxn-route-id="7" hx-target="#url-result-box">MVC</button>
<button hx-get hxn-route="demo-echo" hxn-route-id="42" hxn-route-p="rv" hx-target="#url-result-box">Named route</button>
<button hx-get hxn-area="Admin" hxn-controller="Admin" hxn-action="Ping" hx-target="#url-result-box">Area</button>
<button hx-post hxn-page="/UrlHelper" hxn-page-handler="Post" hx-target="#url-result-box">POST</button>
```

Rendered output (example):

```html
<button hx-get="/UrlHelper?handler=Fragment" hx-target="#url-result-box">Page</button>
<button hx-get="/demo/echo/7" hx-target="#url-result-box">MVC</button>
```

The matching handlers:

```csharp
// UrlHelper.cshtml.cs
public IActionResult OnGetFragment() => Content("page handler Fragment", "text/html");
public IActionResult OnPostPost() => Content("page handler Post ok", "text/html");

// DemoController.cs
[HttpGet("echo/{id?}", Name = "demo-echo")]
public IActionResult Echo(string? id, string? p) => /* ... */;
```

## Attributes

| Attribute | Property | Purpose |
|---|---|---|
| `hxn-page` | `Page` | Razor Page path, e.g. `/Products/Index` |
| `hxn-page-handler` | `PageHandler` | Page handler name (`OnGetFragment` -> `Fragment`) |
| `hxn-controller` | `Controller` | MVC controller name |
| `hxn-action` | `Action` | MVC action name |
| `hxn-area` | `Area` | MVC area (also valid with pages) |
| `hxn-route` | `Route` | Named route |
| `hxn-route-{key}` | `RouteValues` | Route/query values: `hxn-route-id="7"` -> `id=7`. Case-insensitive dictionary. |
| `hxn-host` | `Host` | Makes the URL absolute with this host |
| `hxn-protocol` | `Protocol` | Scheme for an absolute URL (`https`) |
| `hxn-fragment` | `Fragment` | URL `#fragment` |

Other members: `Generator` (the injected `IHtmlGenerator`), `ViewContext` (injected), `Order` (`-1000`, so it runs before other tag helpers) and `Process(...)`. The helper only runs when `hxn-page`, `hxn-action`, `hxn-controller` or `hxn-route` is present.

### Rules

- Put **exactly one** htmx verb on the element. More than one throws `InvalidOperationException`.
- Pick **one** link style: route, action/controller, or page/handler. Mixing them throws `InvalidOperationException`.
- An existing **non-empty** verb value wins; the helper does nothing.
- Binding attributes with no verb on the element do nothing (there is nowhere to write the URL).

:::note[Valueless `hx-get` is left alone]
In htmx 4 an empty `hx-get` is valid: it requests the **current URL**. The helper therefore no longer claims every verb attribute. A valueless `hx-get` with **no** routing attribute is left untouched; with a routing attribute, the generated URL fills it in.
:::

:::caution[Naming]
`hxn-page`, `hxn-action`, `hxn-controller`, `hxn-route*`, `hxn-area`, `hxn-host`, `hxn-protocol` and `hxn-fragment` are htmx.NET conventions, not htmx attributes. htmx never sees them: the helper consumes them at render time.
:::

## Spellings: `:inherited` and `data-hx-`

The helper recognizes `hx-get`, `hx-get:inherited`, `data-hx-get`, `data-hx-get:inherited` (and likewise for post, put, patch, delete) and writes the URL into whichever spelling is present.

## Inheritance in htmx 4

htmx 4 turned implicit inheritance off. A child sees a parent's attribute only when the parent writes it with `:inherited` (or `htmx.config.implicitInheritance` is `true`).

```razor
<!-- samples/Htmx4.Sample/Pages/Inherited.cshtml -->
<section hx-target:inherited="#inh-out" hx-headers-X-Demo="from-parent" hx-headers-inherited="true">
    <button hx-get="/demo/inherited/echo?p=inh">child uses parent's target and headers</button>
</section>
```

| Attribute | Inherits with `:inherited`? |
|---|---|
| `hx-target`, `hx-swap`, `hx-headers`, `hx-include`, ... | Yes |
| `hx-get`, `hx-post`, `hx-put`, `hx-patch`, `hx-delete` | **No**: they do not make descendants send requests |

:::caution[`hx-get:inherited` does not trigger descendant requests]
Only a **direct** verb attribute makes an element fire a request. Writing `hx-get:inherited` on a container does **not** make its children request that URL. The URL tag helper will still write a generated URL into `hx-get:inherited` if you ask it to, but that is not a working trigger pattern. Put the verb (and `hxn-page`/`hxn-controller`/...) on each element that should request, and share the *context* (`hx-target:inherited`, `hx-headers-...` with `hx-headers-inherited`) from a parent. For links and forms use `hx-boost:inherited`.
:::

```razor
<section hx-target:inherited="#out" hx-headers-X-Tenant="@tenant" hx-headers-inherited="true">
    <button hx-get hxn-controller="Orders" hxn-action="List">Orders</button>
    <button hx-get hxn-controller="Invoices" hxn-action="List">Invoices</button>
</section>
<div id="out"></div>
```

The server can verify what the child inherited through [request detection](/Htmx.Net/guides/request-detection/): `h.TargetElement` reports `div#out`, and `Request.Headers["X-Tenant"]` carries the header. See the [Headers tag helper](/Htmx.Net/guides/headers-tag-helper/).

## `hx-delete` and form values

htmx 4 serializes `DELETE` data into the query string and does not include enclosing form inputs automatically. Add `hx-include` when you need them:

```razor
<form>
    <input name="id" value="7" />
    <button hx-delete hxn-page="/ErrorSwaps" hxn-page-handler="Delete"
            hx-include="closest form" hx-target="#del-out">Delete</button>
</form>
```

For antiforgery on `DELETE` see the [Antiforgery guide](/Htmx.Net/guides/antiforgery/#delete-needs-header-mode).

---

See the live demos: run `dotnet run --project samples/Htmx4.Sample` and open `/UrlHelper` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/UrlHelper.cshtml)) and `/Inherited` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Inherited.cshtml)).

Related: [Headers tag helper](/Htmx.Net/guides/headers-tag-helper/), [Config tag helper](/Htmx.Net/guides/config/), [htmx 4 facts: attribute model](/Htmx.Net/reference/htmx4-facts/#6-attribute-model-and-empty-hx-get).
