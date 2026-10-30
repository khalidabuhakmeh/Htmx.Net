---
title: Getting started
description: Quick-start for Htmx and Htmx.TagHelpers on htmx 4 - install, wire up Program.cs, the htmx-config meta, antiforgery, and build a first end-to-end ASP.NET Core example.
---

Htmx.Net gives ASP.NET Core two packages for **htmx 4**. This page takes you from an empty project to a working htmx interaction with antiforgery protection.

| Package | Purpose |
|---|---|
| `Htmx` | Request detection (`IsHtmx()`, `HtmxRequestHeaders`) and response header helpers (`Response.Htmx(...)`). |
| `Htmx.TagHelpers` | Razor tag helpers (`htmx-config` meta, URL generation, `hx-headers-*`) and the antiforgery script. |

:::caution
These packages target **htmx 4 only** (`htmx.org@4.0.0`). htmx 1/2 users should read the [previous version docs](/Htmx.Net/v1/) and the [migration guide](/Htmx.Net/migration/v1-to-v2/). The [htmx 4 facts](/Htmx.Net/reference/htmx4-facts/) page lists the verified behavior these docs rely on.
:::

## 1. Install

```bash
dotnet add package Htmx
dotnet add package Htmx.TagHelpers
```

Both target `net8.0` and `net10.0`. `Htmx` works in any ASP.NET Core app (minimal APIs, MVC, Razor Pages); `Htmx.TagHelpers` needs Razor (MVC views or Razor Pages).

## 2. Add htmx 4 to your page

Vendor the file (recommended; the sample keeps it in `wwwroot/lib/htmx/htmx.min.js`) or use a CDN:

```html
<script src="https://unpkg.com/htmx.org@4.0.0/dist/htmx.min.js" defer></script>
```

:::tip
Pin the version and add a Subresource Integrity hash. The sample's `_Layout.cshtml` shows the SHA-384 for the vendored `htmx.org@4.0.0` build.
:::

## 3. Configure `Program.cs`

From `samples/Htmx4.Sample/Program.cs`:

```csharp
using Htmx.TagHelpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapHtmxAntiforgeryScript();   // serves /_htmx/antiforgery.js
app.MapRazorPages();
app.MapControllers();

app.Run();
```

## 4. Register the tag helpers

`Pages/_ViewImports.cshtml` (or `Views/_ViewImports.cshtml`):

```razor
@using Htmx
@addTagHelper *, Htmx.TagHelpers
```

## 5. Layout `<head>`

```razor
@using Htmx.TagHelpers
<meta name="htmx-config" includeAspNetAntiforgeryToken="true" />
<script src="~/lib/htmx/htmx.min.js" defer></script>
<script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
```

- The `htmx-config` meta is rendered by the [config tag helper](/Htmx.Net/guides/config/). `includeAspNetAntiforgeryToken` embeds the antiforgery token so htmx requests can send it. Keep it **before** the htmx script.
- The script served by `MapHtmxAntiforgeryScript()` attaches that token to `POST`/`PUT`/`PATCH`/`DELETE` requests. See [Antiforgery](/Htmx.Net/guides/antiforgery/).

## 6. A first end-to-end example

A button that loads a server-rendered fragment, plus a form that saves and fires a client event.

`Pages/Products.cshtml`:

```razor
@page
@model ProductsModel

<h1>Products</h1>

<!-- GET: the URL is generated from the page handler by the URL tag helper -->
<button hx-get hx-page="/Products" hx-page-handler="List" hx-target="#list">Load</button>
<ul id="list"></ul>

<!-- POST: antiforgery token is attached automatically -->
<form hx-post hx-page="/Products" hx-page-handler="Add" hx-target="#list" hx-swap="beforeend"
      hx-on:product-added="this.reset()">
    <input name="name" required />
    <button>Add</button>
</form>
```

`Pages/Products.cshtml.cs`:

```csharp
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ProductsModel : PageModel
{
    private static readonly List<string> Items = ["Chai", "Chang"];

    public void OnGet() { }   // the full page

    public IActionResult OnGetList()
    {
        if (!Request.IsHtmx()) return RedirectToPage();            // direct visits get the full page
        Response.Htmx(h => h.WithVary());                          // cache-safe: Vary: HX-Request
        return Content(string.Concat(Items.Select(i => $"<li>{i}</li>")), "text/html");
    }

    public IActionResult OnPostAdd(string name)
    {
        Items.Add(name);
        Response.Htmx(h => h.WithTrigger("product-added"));        // HX-Trigger -> reset the form
        return Content($"<li>{System.Net.WebUtility.HtmlEncode(name)}</li>", "text/html");
    }
}
```

What happens: clicking **Load** sends `GET /Products?handler=List` with `HX-Request: true`; the `<li>` fragment replaces the list contents. Submitting the form sends `POST` with the antiforgery token, appends the new `<li>` (`beforeend`) and the `product-added` event resets the form.

## Where next

| I want to... | Read |
|---|---|
| Detect htmx, boosted, partial vs full | [Request detection](/Htmx.Net/guides/request-detection/) |
| Steer htmx from the server | [Response headers](/Htmx.Net/guides/response-headers/) |
| Fire client events | [Triggering events](/Htmx.Net/guides/triggering-events/) |
| Change htmx settings | [Config tag helper](/Htmx.Net/guides/config/) |
| Generate URLs from routes | [URL tag helper](/Htmx.Net/guides/url-tag-helper/) |
| Send custom headers | [Headers tag helper](/Htmx.Net/guides/headers-tag-helper/) |
| Protect POST/PUT/PATCH/DELETE | [Antiforgery](/Htmx.Net/guides/antiforgery/) |
| Multi-target and morph swaps | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| Cache correctly | [Caching](/Htmx.Net/guides/caching/) |
| Browse working demos | [Samples](/Htmx.Net/samples/) |

Run the full sample with `dotnet run --project samples/Htmx4.Sample`.
