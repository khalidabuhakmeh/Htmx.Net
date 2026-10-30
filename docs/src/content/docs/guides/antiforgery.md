---
title: Antiforgery
description: Send the ASP.NET Core antiforgery token with htmx 4 requests, and the security limits to know about.
---

## Setup

```cshtml
<meta name="htmx-config" includeAspNetAntiforgeryToken="true" />
<script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
```

```csharp
builder.Services.AddAntiforgery();
app.UseAntiforgery();
app.MapHtmxAntiforgeryScript();   // default path "/_htmx/antiforgery.js"
```

| API | Purpose |
|---|---|
| `includeAspNetAntiforgeryToken="true"` | `htmx-config` attribute (`IncludeAntiForgery`) that embeds the token and field/header name in `htmx-config`. |
| `app.MapHtmxAntiforgeryScript(string? path = null, bool minified = true)` | Maps the script endpoint (ETag/304 supported). Returns `IEndpointConventionBuilder`. |
| `HtmxAntiforgeryScriptEndpoints.Path` | The mapped path; use in the `<script src>`. |
| `HtmxSnippets.AntiforgeryJavaScript` / `AntiforgeryJavaScriptMinified` / `GetAntiforgeryJavaScript(bool minified)` | The raw script text, e.g. for inlining or custom hosting. |
| `@Html.HtmxAntiforgeryScript(bool minified = true)` | `IHtmlHelper` extension (`HtmlExtensions`) that renders the script inline (returns `IHtmlContent`). |

The script listens to `htmx:config:request` and attaches the token to non-safe methods (POST/PUT/PATCH/DELETE) as a header or form field. GET/HEAD/OPTIONS/TRACE never receive it. See `samples/Htmx4.Sample/Pages/Antiforgery.cshtml`.

## Security notes

1. **Safe methods are skipped.**
2. **Same-origin guard.** The token is attached only when the request target resolves to the page origin. The target is resolved against `document.baseURI` (what `fetch()` uses), so a `<base href>` cannot smuggle a cross-origin request through.
3. **No automatic token refresh.** The script injects the token; it does not refresh it from responses. htmx 4 boost swaps only `<body>` and never merges `<head>`, and parsing response text to refresh would allow token-config poisoning from untrusted fragment or cross-origin responses. If you rotate the token mid-session (e.g. sign-in/out) while using `hx-boost`, trigger a **full page reload**, for example with `Response.Htmx(h => h.Refresh())` (`HX-Refresh`).
4. **DELETE requires header mode.** htmx 4 serializes DELETE form data into the query string, which ASP.NET antiforgery validation does not inspect. Use header-mode antiforgery for DELETE.
5. **Residual risk with `mode: "cors"`.** The guard checks only the initial URL. If a same-origin endpoint 307/308-redirects to another origin and you opted into `htmx.config.mode: "cors"`, the browser may re-send the token (body or custom header) to the foreign origin. The default `mode: "same-origin"` blocks this. Mitigate by avoiding open redirects, rotating tokens, and validating `Origin`/`Referer` server-side.
