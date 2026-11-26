---
title: Antiforgery
description: Send the ASP.NET Core antiforgery token with htmx 4 requests using the htmx-config tag helper, MapHtmxAntiforgeryScript, HtmxSnippets and Html.HtmxAntiforgeryScript, plus the security limits to know about.
---

ASP.NET Core rejects unsafe requests (POST, PUT, PATCH, DELETE) that lack a valid antiforgery token. htmx requests are not form posts, so something must attach the token. htmx.NET does that with a small script that reads the token from `htmx-config` and decorates each same-origin, non-safe request. You keep using the standard `[AutoValidateAntiforgeryToken]`, `[ValidateAntiForgeryToken]` and `app.UseAntiforgery()`.

## Setup

`Program.cs`:

```csharp
using Htmx.TagHelpers;

builder.Services.AddAntiforgery();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
// default path "/_htmx/antiforgery.js"
app.MapHtmxAntiforgeryScript();
app.MapRazorPages();
```

`_Layout.cshtml` `<head>`:

```razor
@using Htmx.TagHelpers
<meta name="htmx-config" includeAspNetAntiforgeryToken="true" />
<script src="~/lib/htmx/htmx.min.js" defer></script>
<script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
```

Then write ordinary htmx markup:

```razor
<!-- no body form: token goes in the header -->
<button hx-post="/demo/af/check" hx-target="#af-out">POST header</button>

<!-- form body: the token is added as a hidden form field -->
<form hx-post="/demo/af/check" hx-target="#af-out">
    <input name="note" value="form-post" />
    <button type="submit">POST form</button>
</form>
```

```csharp
// DemoController.cs
[Route("demo/af")]
[AutoValidateAntiforgeryToken]
public class AntiforgeryDemoController : Controller
{
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", Route = "strict")]
    public IActionResult Strict() => Content("validated");

    [HttpPost("validate-attr"), ValidateAntiForgeryToken]
    public IActionResult ValidateAttr() => Content("validated");
}
```

A raw `fetch("/demo/af/strict", { method: "POST" })` sends no token and gets `400`.

## API

| API | Purpose |
|---|---|
| `includeAspNetAntiforgeryToken="true"` | `htmx-config` attribute (`HtmxConfigMetaTagHelper.IncludeAntiForgery`) that embeds the token, header name and form field name in `htmx-config`. |
| `app.MapHtmxAntiforgeryScript(string? path = null, bool minified = true)` | Extension on `IEndpointRouteBuilder` (class `HtmxAntiforgeryScriptEndpoints`). Maps the script with ETag/304 support; returns `IEndpointConventionBuilder`. |
| `HtmxAntiforgeryScriptEndpoints.Path` | The mapped path (`"/_htmx/antiforgery.js"`); use it in `<script src>`. |
| `HtmxSnippets.AntiforgeryJavaScript` | Raw script text. |
| `HtmxSnippets.AntiforgeryJavaScriptMinified` | Minified script text. |
| `HtmxSnippets.GetAntiforgeryJavaScript(bool minified)` | Choose a variant at runtime. |
| `@Html.HtmxAntiforgeryScript(bool minified = true)` | `HtmlExtensions` extension on `IHtmlHelper`; returns `IHtmlContent` with the script inline. |

### Custom hosting and inlining

```csharp
app.MapHtmxAntiforgeryScript("/assets/htmx-af.js", minified: false)
   // it is an IEndpointConventionBuilder
   .RequireHost("app.example.com");
```

```razor
@* Inline instead of a separate request (for strict hosting or CSP with nonces, add your own <script nonce> wrapper using HtmxSnippets) *@
@Html.HtmxAntiforgeryScript()

<script nonce="@nonce">@Html.Raw(HtmxSnippets.GetAntiforgeryJavaScript(minified: true))</script>
```

## What the server sees

The script listens to `htmx:config:request` and, for POST/PUT/PATCH/DELETE to the page origin:

- **No form body** (e.g. a bare `hx-post` button): adds the header (`RequestVerificationToken` by default).
- **Form body**: adds the hidden form field (`__RequestVerificationToken`), unless the form already carries one.

GET/HEAD/OPTIONS/TRACE never receive the token. In header mode a rendered token field is removed from the request (the header is authoritative). The `Check` action in the sample reports `token-source: header | form | none` so you can see which mode ran.

## Security fix in 2.0.0-preview.2

2.0.0-preview.1 could leak a rendered token into the URL (`<form hx-get>`, `<form hx-delete>`) and to other origins (`mode: "cors"`, cross-origin `<base href>`). 2.0.0-preview.2 fixes this as described below. Update, and use header mode (the default) for DELETE. The 1.x line received the equivalent fix in 1.13.0; see the [v1 docs](/Htmx.Net/v1/tag-helpers/#htmx-and-anti-forgery-tokens).

## Security notes

1. **Safe methods are skipped.**
2. **Same-origin guard.** The token is attached only when the request target resolves to the page origin. The target is resolved against `document.baseURI` (what `fetch()` uses), so a `<base href>` cannot smuggle a cross-origin request through.
3. **No automatic token refresh.** The script injects the token; it does not refresh it from responses. htmx 4 boost swaps only `<body>` and never merges `<head>`, and parsing response text to refresh would allow token-config poisoning from untrusted fragment or cross-origin responses. If you rotate the token mid-session (e.g. sign-in/out) while using `hx-boost`, trigger a **full page reload**, for example `Response.Htmx(h => h.Refresh())` (`HX-Refresh`).
4. <a id="delete-needs-header-mode"></a>**DELETE requires header mode.** htmx 4 serializes DELETE form data into the query string, which ASP.NET antiforgery validation does not inspect and where a token must never go. In form-field mode (no header name configured) the script strips the token field, sends nothing, and logs `[htmx.net] Antiforgery token not sent for <METHOD> ...` to the console. The same applies to any method htmx 4 sends as URL parameters (it matches `/GET|DELETE/`, unanchored, so a custom `hx-method="XGET"` also fails closed). Use header mode for DELETE. In header mode `<form hx-delete>` works: the header is set and the form copy is removed.
5. **Rendered token fields are stripped when they must not be sent.** htmx 4 collects a server-rendered `__RequestVerificationToken` hidden input into every request, and moves GET/DELETE values into the URL. The script removes that field on safe-method and cross-origin requests (including a cross-origin `<base href>`), so the token never appears in a URL (logs, history, `Referer`) or reaches another origin.
6. **Residual risk with `mode: "cors"`.** The guard checks only the initial URL. If a same-origin endpoint 307/308-redirects to another origin and you opted into `htmx.config.mode: "cors"`, the browser may re-send the token (body or custom header) to the foreign origin. The default `mode: "same-origin"` blocks this. Mitigate by avoiding open redirects, rotating tokens, and validating `Origin`/`Referer` server-side.

:::caution
If a page overrides `htmx-config` through a layout section, include `includeAspNetAntiforgeryToken="true"` again, or the token will not be present. See [Config tag helper](/Htmx.Net/guides/config/#page-scoped-config-with-a-layout-section).
:::

:::tip
You can build the script's own endpoint into tests with `HtmxSnippets.AntiforgeryJavaScript`; the sample serves the non-minified variant separately to exercise both.
:::

---

See the live demo: run `dotnet run --project samples/Htmx4.Sample` and open `/Antiforgery` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Antiforgery.cshtml)).

Related: [Config tag helper](/Htmx.Net/guides/config/), [htmx 4 reference: `htmx:config:request`](/Htmx.Net/reference/htmx4-facts/#5-events-and-event-detail), [Getting started](/Htmx.Net/getting-started/).

**From the blog:** [HTMX Boosts and ASP.NET Core Anti-Forgery Tokens](https://khalidabuhakmeh.com/posts/htmx-boosts-and-aspnet-core-anti-forgery-tokens/) and [HTMX Requests with ASP.NET Core Anti-Forgery Tokens](https://khalidabuhakmeh.com/posts/htmx-requests-with-aspnet-core-anti-forgery-tokens/).
