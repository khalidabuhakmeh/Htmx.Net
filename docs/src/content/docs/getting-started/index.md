---
title: Getting started
description: Install Htmx and Htmx.TagHelpers for htmx 4 and wire them into an ASP.NET Core app.
---

Htmx.Net gives ASP.NET Core two packages for **htmx 4**:

| Package | Purpose |
|---|---|
| `Htmx` | Request detection (`IsHtmx()`, `HtmxRequestHeaders`) and response header helpers (`Response.Htmx(...)`). |
| `Htmx.TagHelpers` | Razor tag helpers (`htmx-config` meta, URL generation, `hx-headers-*`) and the antiforgery script. |

:::caution
These packages target **htmx 4 only** (`htmx.org@4.0.0`). htmx 1/2 users should read the [previous version docs](/Htmx.Net/v1/) and the [migration guide](/Htmx.Net/migration/v1-to-v2/).
:::

## Install

```bash
dotnet add package Htmx
dotnet add package Htmx.TagHelpers
```

Targets: `net8.0` and `net10.0`.

## Add htmx 4 to your page

Vendor the file (recommended; the sample vendors it to `wwwroot/lib/htmx/htmx.min.js`) or use a CDN:

```html
<script src="https://unpkg.com/htmx.org@4.0.0/dist/htmx.min.js" defer></script>
```

## Minimal setup

`Program.cs` (from `samples/Htmx4.Sample/Program.cs`):

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

app.MapHtmxAntiforgeryScript();   // serves the antiforgery script
app.MapRazorPages();
app.MapControllers();

app.Run();
```

`_ViewImports.cshtml`:

```cshtml
@using Htmx
@addTagHelper *, Htmx.TagHelpers
```

`_Layout.cshtml` `<head>` (from `samples/Htmx4.Sample/Pages/Shared/_Layout.cshtml`):

```cshtml
@using Htmx.TagHelpers
<meta name="htmx-config" includeAspNetAntiforgeryToken="true" />
<script src="~/lib/htmx/htmx.min.js" defer></script>
<script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
```

- The `htmx-config` meta is rendered by the [config tag helper](/Htmx.Net/guides/config/). `includeAspNetAntiforgeryToken` embeds the antiforgery token so htmx requests can send it.
- The script served by `MapHtmxAntiforgeryScript()` attaches that token to `POST`/`PUT`/`PATCH`/`DELETE` requests. See [Antiforgery](/Htmx.Net/guides/antiforgery/).

## First handler

```csharp
public IActionResult OnPostSave()
{
    if (!Request.IsHtmx(out var htmx)) return RedirectToPage();
    Response.Htmx(h => h.WithTrigger("saved"));
    return Partial("_Saved");
}
```

## Next

- [Request detection](/Htmx.Net/guides/request-detection/)
- [Response headers](/Htmx.Net/guides/response-headers/)
- [Samples](/Htmx.Net/samples/)
