# htmx.NET

![HTMX Logo](https://github.com/khalidabuhakmeh/Htmx.Net/raw/main/shared/htmx-logo.png)

[![Htmx](https://img.shields.io/nuget/v/Htmx?label=Htmx)](https://www.nuget.org/packages/Htmx/)
[![Htmx.TagHelpers](https://img.shields.io/nuget/v/Htmx.TagHelpers?label=Htmx.TagHelpers)](https://www.nuget.org/packages/Htmx.TagHelpers/)

> [!IMPORTANT]
> **htmx.NET 2.x targets [htmx 4](https://four.htmx.org/) only.** It is a breaking change from the
> 1.x line (which supports htmx 1.x/2.x). See the **[Migration guide](https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/)**.
> Staying on htmx 1/2? Use htmx.NET **1.x** and the [previous-version docs](https://khalidabuhakmeh.github.io/Htmx.Net/v1/).

> [!NOTE]
> **2.0.0 is in preview.** Install it with `--prerelease` and kick the tires — then please share
> feedback on [GitHub issues](https://github.com/khalidabuhakmeh/Htmx.Net/issues) before the stable release.

Server-side ASP.NET Core helpers that make working with [htmx](https://htmx.org/) from `HttpRequest`
and `HttpResponse` simpler, plus Tag Helpers for URL generation, `htmx-config`, and antiforgery.

**📖 Full documentation: [khalidabuhakmeh.github.io/Htmx.Net](https://khalidabuhakmeh.github.io/Htmx.Net/)**

## Install

While 2.x is in preview, add `--prerelease`:

```console
dotnet add package Htmx --prerelease
dotnet add package Htmx.TagHelpers --prerelease
```

Requires **htmx 4** on the client:

```html
<script src="https://unpkg.com/htmx.org@4.0.0/dist/htmx.min.js" defer></script>
```

## Quick start

**Detect htmx requests** and return a partial instead of a full page:

```csharp
// in a Razor Page or controller
return Request.IsHtmx()
    ? Partial("_Form", this)
    : Page();
```

Read typed htmx request headers (the `HX-Source` element reference, request type, and more):

```csharp
if (Request.IsHtmx(out var headers))
{
    var source = headers.SourceElement;   // tag#id, e.g. button#save
    var kind   = headers.RequestType;     // Full or Partial
}
```

**Set htmx response headers** with a fluent API:

```csharp
Response.Htmx(h => h
    .PushUrl("/new-url")
    .Retarget("#result")
    .Reswap(HtmxSwap.OuterMorph)
    .WithTrigger("saved", new { id = 42 }));
```

**Tag Helpers** — add to `_ViewImports.cshtml`:

```razor
@addTagHelper *, Htmx.TagHelpers
```

Generate ASP.NET Core URLs straight onto htmx attributes, and configure htmx + antiforgery:

```html
<meta name="htmx-config" includeAspNetAntiforgeryToken="true" defaultSwap="innerHTML" />

<button hx-get hxn-page="Index" hxn-page-handler="Snippet" hx-target="#result">
    Load
</button>
```

```csharp
// Program.cs — expose the antiforgery integration script
app.MapHtmxAntiforgeryScript();
```

## Documentation

- **[Getting started](https://khalidabuhakmeh.github.io/Htmx.Net/getting-started/)**
- **[Guides](https://khalidabuhakmeh.github.io/Htmx.Net/guides/request-detection/)** — request detection, response headers, triggers, config, Tag Helpers, antiforgery, caching, partials & morph
- **[Migration guide (v1 → v2)](https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/)**
- **[Previous version (v1.x)](https://khalidabuhakmeh.github.io/Htmx.Net/v1/)**
- **Samples:** [`samples/Htmx4.Sample`](samples/Htmx4.Sample) demonstrates every feature with integration and Playwright E2E tests.
- **More samples:** [Twelve Days of htmx-mas](https://github.com/khalidabuhakmeh/twelve-days-of-htmx-mas) — a Christmas-themed ASP.NET Core app with 12 htmx demos.

> New to htmx with ASP.NET Core? See the [JetBrains guide to getting started with htmx for ASP.NET Core developers](https://www.jetbrains.com/dotnet/guide/tutorials/htmx-aspnetcore/).

## License

Copyright (c) Khalid Abuhakmeh. Licensed under the MIT License — see [License.md](License.md).
