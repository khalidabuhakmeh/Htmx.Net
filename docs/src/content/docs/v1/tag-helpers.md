---
title: Htmx.TagHelpers (v1.x)
description: URL generation, Headers and Config tag helpers, and antiforgery in htmx.NET v1.x.
---

:::caution[Previous version]
This documents **htmx.NET v1.x** for **htmx 1.x / 2.x**. The current version targets **htmx 4** and has breaking changes. See the [Migration guide](/Htmx.Net/migration/v1-to-v2/) and the current docs.
:::

:::note
The full v1 source is available at the [`v1-final` git tag](https://github.com/khalidabuhakmeh/Htmx.Net/tree/v1-final). The frozen legacy sample lives in `test/Sample` (see its README).
:::


## Getting Started

Install the `Htmx.TagHelpers` [NuGet package](https://www.nuget.org/packages/Htmx.TagHelpers/) to your ASP.NET Core project. Targets .NET Core 3.1+ projects.

```console
dotnet add package Htmx.TagHelpers
```

Make the Tag Helpers available in your project by adding the following line to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, Htmx.TagHelpers
```

## URL Generation

You'll generally need URL paths pointing back to your ASP.NET Core backend. Luckily, `Htmx.TagHelpers` mimics the url generation included in ASP.NET Core. This makes linking HTMX with your ASP.NET Core application a seamless experience.

```html
<div hx-target="this">
    <button hx-get
            hx-page="Index"
            hx-page-handler="Snippet"
            hx-swap="outerHtml">
        Click Me (Razor Page w/ Handler)
    </button>
</div>

<div hx-target="this">
    <button hx-get
            hx-controller="Home"
            hx-action="Index"
            hx-route-id="1">
        Click Me (Controller)
    </button>
</div>

<div hx-target="this">
    <button hx-post
            hx-route="named">
        Click Me (Named)
    </button>
</div>
```

The URL tag helper supports the following routing attributes:

| Attribute | Description |
|---|---|
| `hx-action` | The name of the action method. |
| `hx-controller` | The name of the controller. |
| `hx-area` | The name of the area. |
| `hx-page` | The name of the Razor Page. |
| `hx-page-handler` | The name of the Razor Page handler. |
| `hx-route` | Name of the route (mutually exclusive with action/controller/page). |
| `hx-route-{key}` | Additional route parameter values (e.g., `hx-route-id="1"`). |
| `hx-protocol` | The protocol for the URL (e.g., `"https"`). |
| `hx-host` | The host name. |
| `hx-fragment` | The URL fragment name. |

These work with any of the htmx method attributes: `hx-get`, `hx-post`, `hx-put`, `hx-patch`, and `hx-delete`.

## Headers Tag Helper

The `HtmxHeadersTagHelper` lets you build the `hx-headers` JSON attribute from individual prefixed attributes, which can be cleaner than writing JSON by hand:

```html
<button hx-post="/api/data"
        hx-headers-X-Custom-Header="my-value"
        hx-headers-Authorization="Bearer token123">
    Submit
</button>
```

This produces:

```html
<button hx-post="/api/data" hx-headers='{"X-Custom-Header":"my-value","Authorization":"Bearer token123"}'>
    Submit
</button>
```

## Htmx.Config

An additional `htmx-config` tag helper is included that can be applied to a `meta` element in your page's `head` that makes creating HTMX configuration simpler. For example, below we can set the `historyCacheSize`, default `indicatorClass`, and whether to include ASP.NET Core's anti-forgery tokens as an additional element on the HTMX configuration.

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta name="htmx-config" 
          historyCacheSize="20"
          indicatorClass="htmx-indicator"
          includeAspNetAntiforgeryToken="true"
          />
    <!-- additional elements... -->
</head>
```

The resulting HTML will be.

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta name="htmx-config" content='{"indicatorClass":"htmx-indicator","historyCacheSize":20,"antiForgery":{"formFieldName":"__RequestVerificationToken","headerName":"RequestVerificationToken","requestToken":"<token>"}}' />
    <!-- additional elements... -->
</head>
```

### Supported Configuration Attributes

The config tag helper supports the full set of [htmx configuration options](https://htmx.org/docs/#config):

| Attribute | Type | Description |
|---|---|---|
| `historyEnabled` | `bool` | Whether htmx history is enabled. |
| `historyCacheSize` | `int` | Number of history entries to cache (default: `10`). |
| `refreshOnHistoryMiss` | `bool` | Issue full page refresh on history misses. |
| `defaultSwapStyle` | `string` | Default swap style (default: `"innerHTML"`). |
| `defaultSwapDelay` | `int` | Default swap delay in milliseconds (default: `0`). |
| `defaultSettleDelay` | `int` | Default settle delay in milliseconds (default: `100`). |
| `includeIndicatorStyles` | `bool` | Whether indicator styles are loaded. |
| `indicatorClass` | `string` | CSS class for indicators (default: `"htmx-indicator"`). |
| `requestClass` | `string` | CSS class applied during requests (default: `"htmx-request"`). |
| `settlingClass` | `string` | CSS class applied during settling (default: `"htmx-settling"`). |
| `swappingClass` | `string` | CSS class applied during swapping (default: `"htmx-swapping"`). |
| `addedClass` | `string` | CSS class applied to newly added elements (default: `"htmx-added"`). |
| `selfRequestsOnly` | `bool` | Only allow requests to the same domain. |
| `allowScriptTags` | `bool` | Whether script tags are processed (default: `true`). |
| `allowEval` | `bool` | Whether eval is allowed (default: `true`). |
| `useTemplateFragments` | `bool` | Use HTML template tags for parsing. |
| `wsReconnectDelay` | `string` | WebSocket reconnect delay strategy (default: `"full-jitter"`). |
| `wsBinaryType` | `string` | WebSocket binary type (default: `"blob"`). |
| `disableSelector` | `string` | CSS selector for elements to skip htmx processing. |
| `timeout` | `int` | Default timeout for HTTP requests in milliseconds. |
| `scrollBehavior` | `string` | Scroll behavior (default: `"smooth"`). |
| `defaultFocusScroll` | `bool` | Whether to scroll focused elements into view. |
| `getCacheBusterParam` | `bool` | Add cache-buster param to GET requests. |
| `globalViewTransitions` | `bool` | Enable global view transitions. |
| `ignoreTitle` | `bool` | If true, htmx won't update the document title. |
| `scrollIntoViewOnBoost` | `bool` | Scroll boosted targets into viewport. |
| `disableInheritance` | `bool` | Completely disable attribute inheritance. |
| `inlineScriptNonce` | `string` | CSP nonce for inline scripts. |
| `inlineStyleNonce` | `string` | CSP nonce for inline styles. |
| `withCredentials` | `bool` | Whether to send credentials with AJAX requests. |
| `allowNestedOobSwaps` | `bool` | Process out-of-band swaps on nested elements. |
| `includeAspNetAntiforgeryToken` | `bool` | Include ASP.NET Core antiforgery token in the config. |

## HTMX and Anti-forgery Tokens

You can set the attribute `includeAspNetAntiforgeryToken` on the `htmx-config` element. Then you'll need to include this additional JavaScript in your web application. We include the attribute `__htmx_antiforgery` to track the event listener was added already. This keeps us from accidentally re-registering the event listener.

```javascript
if (!document.body.attributes.__htmx_antiforgery) {
    document.addEventListener("htmx:configRequest", evt => {
        let httpVerb = evt.detail.verb.toUpperCase();
        if (httpVerb === 'GET') return;
        let antiForgery = htmx.config.antiForgery;
        if (antiForgery) {
            // already specified on form, short circuit
            if (evt.detail.parameters[antiForgery.formFieldName])
                return;

            if (antiForgery.headerName) {
                evt.detail.headers[antiForgery.headerName]
                    = antiForgery.requestToken;
            } else {
                evt.detail.parameters[antiForgery.formFieldName]
                    = antiForgery.requestToken;
            }
        }
    });
    document.addEventListener("htmx:afterOnLoad", evt => {
        if (evt.detail.boosted) {
            const parser = new DOMParser();
            const html = parser.parseFromString(evt.detail.xhr.responseText, 'text/html');
            const selector = 'meta[name=htmx-config]';
            const config = html.querySelector(selector);
            if (config) {
                const current = document.querySelector(selector);
                // only change the anti-forgery token
                const key = 'antiForgery';
                htmx.config[key] = JSON.parse(config.attributes['content'].value)[key];
                // update DOM (usually unnecessary)
                current.replaceWith(config);
            }
        }
    });
    document.body.attributes.__htmx_antiforgery = true;
}
```

You can access the snippet in two ways. The first is to use the `HtmxSnippets` static class in your views.

```
<script>
@Html.Raw(HtmxSnippets.AntiforgeryJavaScript)
</script>
```

A simpler way is to use the `HtmlExtensions` class that extends `IHtmlHelper`.

```
@Html.HtmxAntiforgeryScript()
```

This html helper will result in a `<script>` tag along with the previously mentioned JavaScript. **Note: You can still register multiple event handlers for `htmx:configRequest`, so having more than one is ok.**

Note that if the `hx-[get|post|put]` attribute is on a `<form ..>` tag _and_ the `<form>` element has a `method="post"` (and also an empty or missing `action=""`) attribute, the ASP.NET Tag Helpers [will add the Anti-forgery Token](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-7.0#antiforgery-in-aspnet-core) as an `input` element and you do not need to further configure your requests as above. You could also use [`hx-include`](https://htmx.org/attributes/hx-include/) pointing to a form, but this all comes down to a matter of preference.

Additionally, and **the recommended approach** is to use the `HtmxAntiforgeryScriptEndpoint`, which will let you map the JavaScript file to a specific endpoint, and by default it will be `/_htmx/antiforgery.js`.

```c#
app.UseAuthorization();
// registered here
app.MapHtmxAntiforgeryScript();
app.MapRazorPages();
app.MapControllers();
```

You can also pass a custom path and choose between minified or non-minified output:

```c#
app.MapHtmxAntiforgeryScript("/custom/path.js", minified: false);
```

You can now configure this endpoint with caching, authentication, etc. More importantly, you can use the script in your `head` tag now by applying the `defer` tag, which is preferred to having JavaScript at the end of a `body` element.

```html
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <meta
        name="htmx-config"
        historyCacheSize="20"
        indicatorClass="htmx-indicator"
        includeAspNetAntiforgeryToken="true"/>
    <title>@ViewData["Title"] - Htmx.Sample</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true"/>
    <script src="~/lib/jquery/dist/jquery.min.js" defer></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js" defer></script>
    <script src="https://unpkg.com/htmx.org@@1.9.2" defer></script>
    <!-- this uses the static value in a script tag -->
    <script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
</head>
```

