---
title: URL tag helper
description: Generate hx-get/hx-post URLs from Razor Pages, MVC controllers and named routes with HtmxUrlTagHelper.
---

`HtmxUrlTagHelper` turns routing attributes into a URL and writes it into the htmx verb attribute you already put on the element (`hx-get`, `hx-post`, `hx-put`, `hx-patch`, `hx-delete`). Example from `samples/Htmx4.Sample/Pages/UrlHelper.cshtml`:

```cshtml
<button hx-get hx-page="/UrlHelper" hx-page-handler="Fragment" hx-target="#url-result-box">Page</button>
<button hx-get hx-controller="Demo" hx-action="Echo" hx-route-id="7" hx-target="#url-result-box">MVC</button>
<button hx-get hx-route="demo-echo" hx-route-id="42" hx-route-p="rv" hx-target="#url-result-box">Named route</button>
<button hx-get hx-area="Admin" hx-controller="Admin" hx-action="Ping" hx-target="#url-result-box">Area</button>
<button hx-post hx-page="/UrlHelper" hx-page-handler="Post" hx-target="#url-result-box">POST</button>
```

| Attribute | Property | Purpose |
|---|---|---|
| `hx-page` | `Page` | Razor Page path |
| `hx-page-handler` | `PageHandler` | Page handler name |
| `hx-controller` | `Controller` | MVC controller |
| `hx-action` | `Action` | MVC action |
| `hx-area` | `Area` | MVC area |
| `hx-route` | `Route` | Named route |
| `hx-route-{key}` | `RouteValues` | Route/query values, e.g. `hx-route-id="7"` |
| `hx-host` | `Host` | Absolute URL host |
| `hx-protocol` | `Protocol` | Absolute URL scheme |
| `hx-fragment` | `Fragment` | URL `#fragment` |

An existing non-empty verb value wins over the generated URL.

## Spellings: `:inherited` and `data-hx-`

The helper recognizes `hx-get`, `hx-get:inherited`, `data-hx-get`, `data-hx-get:inherited` (and likewise for post/put/patch/delete) and writes the URL into whichever spelling is present.

:::caution[`hx-get:inherited` does not trigger requests in htmx 4]
In htmx 4 only a **direct** verb attribute makes an element fire a request. `:inherited` on a verb does **not** make descendants issue requests. The helper will still write the URL into `hx-get:inherited`, but that is not a working trigger pattern. Put the verb on each element that should request, or use `hx-boost:inherited` for links and forms.
:::

`hx-target:inherited`, `hx-headers:inherited` etc. do inherit; only verbs do not.

## `hx-delete` and form values

In htmx 4, `hx-delete` no longer automatically includes enclosing form inputs (DELETE form data is serialized to the query string). Add `hx-include="closest form"` or similar when you need them. See `samples/Htmx4.Sample/Pages/ErrorSwaps.cshtml`.

For tag helper attributes in the antiforgery context, see [Antiforgery](/Htmx.Net/guides/antiforgery/). For headers, see [Headers tag helper](/Htmx.Net/guides/headers-tag-helper/).
