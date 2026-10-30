---
title: Caching
description: Use Vary with HX-Request and HX-Request-Type when one URL serves full pages and htmx fragments.
---

If the same URL returns a full page to normal requests and a fragment to htmx requests, caches (browser, CDN, proxy) must know the response varies. Otherwise a cached fragment can be served as a whole page, e.g. after back navigation.

```csharp
public IActionResult OnGet()
{
    Response.Htmx(h => h.WithVary());   // Vary: HX-Request
    return Request.IsHtmx() ? Partial("_List") : Page();
}
```

`WithVary()` adds `HX-Request`. To vary on other headers:

```csharp
Response.Htmx(h => h.WithVary("HX-Source", "HX-Target"));  // no duplicates if already present
```

## HX-Request-Type

If you branch on `IsHtmxPartial()` / `IsHtmxFullRequest()` (the `HX-Request-Type` header), include it in `Vary`:

```csharp
Response.Htmx(h => h.WithVary("HX-Request", "HX-Request-Type"));
```

The htmx 4 caching docs only show `Vary: HX-Request`; adding `HX-Request-Type` follows the general rule "vary on every request header your response depends on".

Note htmx 4 re-fetches from the server on history navigation (no built-in history cache), so correct `Vary` matters for back/forward too.
