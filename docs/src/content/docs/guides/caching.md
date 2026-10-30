---
title: Caching
description: Use Vary with HX-Request and HX-Request-Type via WithVary() when one URL serves full pages and htmx fragments, so browsers, CDNs and proxies never serve a fragment as a page.
---

If the same URL returns a full page to normal requests and a fragment to htmx requests, caches (browser, CDN, reverse proxy) must be told that the response **varies** by request header. Otherwise a cached fragment can be served as a whole page, for example after back navigation or when someone opens the URL directly. That is the whole job of `WithVary()`.

## The bug

```csharp
public IActionResult OnGet() =>
    // same URL, two representations
    Request.IsHtmx() ? Partial("_List") : Page();
```

1. A user clicks an `hx-get` link; the browser caches the **fragment** for `/products`.
2. They open `/products` directly (or hit back). The browser reuses the cached fragment: an unstyled page with no layout.

## The fix: `WithVary()`

```csharp
public IActionResult OnGet()
{
    // Vary: HX-Request
    Response.Htmx(h => h.WithVary());
    return Request.IsHtmx() ? Partial("_List") : Page();
}
```

`WithVary()` appends `HX-Request` to the `Vary` response header. Caches now store htmx and non-htmx responses separately.

Also works in an MVC action:

```csharp
[HttpGet("products")]
public IActionResult Index()
{
    Response.Htmx(h => h.WithVary());
    return Request.IsHtmx() ? PartialView("_List", model) : View(model);
}
```

## Varying on other headers

`WithVary(params string[] headerNames)` appends exactly the headers you name. Names already present in `Vary` are not duplicated, and other `Vary` values you set earlier are preserved.

```csharp
// DemoController.cs
Response.Htmx(h => h.PreventPush().WithVary("HX-Source", "HX-Target"));
```

Rule of thumb: **vary on every request header your response depends on.** If you pick the representation from `HX-Target`, include `HX-Target`.

## HX-Request-Type

If you branch on `IsHtmxPartial()` / `IsHtmxFullRequest()` (the `HX-Request-Type` header), include it in `Vary`:

```csharp
Response.Htmx(h => h.WithVary("HX-Request", "HX-Request-Type"));
return Request.IsHtmxPartial() ? Partial("_List") : Page();
```

:::note
The htmx 4 caching docs only show `Vary: HX-Request`. Adding `HX-Request-Type` follows the general `Vary` rule and is an inference, not an explicit htmx recommendation ([htmx 4 facts §9](/Htmx.Net/reference/htmx4-facts/#9-vary-and-caching)).
:::

## Related behavior

- htmx 4 re-fetches from the server on history navigation (there is no built-in history cache), so correct `Vary` matters for back/forward too. `Request.IsHtmxHistoryRestoreRequest()` tells you such a request.
- Response-level cache attributes still apply: `[ResponseCache(Duration = 60, VaryByHeader = "HX-Request")]` is an alternative to calling `WithVary()` when you use ASP.NET response caching.
- A separate URL per representation (for example `/products` and `/products/list`) avoids the problem entirely.

:::tip
Checking the result in a browser: DevTools > Network > the response headers should show `Vary: HX-Request`.
:::

---

See it live: the `/ResponseHeaders` demo sets `Vary` through `WithVary()` and `WithVary("HX-Source", "HX-Target")` (run `dotnet run --project samples/Htmx4.Sample`; [source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/ResponseHeaders.cshtml)).

Related: [Request detection](/Htmx.Net/guides/request-detection/), [Response headers](/Htmx.Net/guides/response-headers/).
