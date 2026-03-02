# Htmx.Net

![HTMX Logo](https://github.com/khalidabuhakmeh/Htmx.Net/raw/main/shared/htmx-logo.png)

> [!IMPORTANT]  
> **This package works with HTMX 1.x and 2.x**

This package is designed to add server-side helper methods for `HttpRequest` and `HttpResponse`. This makes working with [htmx](https://htmx.org/) server-side concepts simpler. You should also consider reading about [Hyperscript](https://hyperscript.org/), an optional companion project for HTMX.

> [!NOTE]
> **If you're new to HTMX, check out this series on [getting started with HTMX for ASP.NET Core developer](https://www.jetbrains.com/dotnet/guide/tutorials/htmx-aspnetcore/) which also includes a sample project and patterns that you might find helpful.**

## Htmx Extension Methods

### Getting Started

Install the `Htmx` [NuGet package](https://www.nuget.org/packages/Htmx/) to your ASP.NET Core project.

```console
dotnet add package Htmx
```

### HttpRequest

Using the `HttpRequest`, we can determine if the request was initiated by Htmx on the client.

```c#
httpContext.Request.IsHtmx()
```

This can be used to return a full-page response or a partial-page render.

```c#
// in a Razor Page
return Request.IsHtmx()
    ? Partial("_Form", this)
    : Page();
```

We can also retrieve the other header values htmx might set.

```c#
Request.IsHtmx(out var values);
```

Read more about the other header values on the [official documentation page](https://htmx.org/reference/#request_headers).

#### Additional Request Checks

There are additional extension methods for common htmx request checks:

```c#
// Was the request boosted via hx-boost?
Request.IsHtmxBoosted()

// Is this a non-boosted htmx request? (htmx request that is NOT boosted)
Request.IsHtmxNonBoosted()

// Also available with out parameter for header values
Request.IsHtmxNonBoosted(out var values)

// Is this a history restore request after a local cache miss?
Request.IsHtmxHistoryRestoreRequest()
```

#### HtmxRequestHeaders

When using the `out` overload of `IsHtmx` or `IsHtmxNonBoosted`, the `HtmxRequestHeaders` object provides typed access to all htmx request headers:

| Property | Type | Description |
|---|---|---|
| `Boosted` | `bool` | Whether the request was made via `hx-boost`. |
| `CurrentUrl` | `string` | The current URL of the browser when the request was made. |
| `HistoryRestoreRequest` | `bool` | Whether the request is for history restoration after a local cache miss. |
| `Prompt` | `string` | The user response to an `hx-prompt`. |
| `Target` | `string` | The `id` of the target element, if it exists. |
| `Trigger` | `string` | The `id` of the triggered element, if it exists. |
| `TriggerName` | `string` | The `name` of the triggered element, if it exists. |

#### Browser Caching

As a special note, please remember that if your server can render different content for the same URL depending on some other headers, you need to use the Vary response HTTP header. For example, if your server renders the full HTML when Request.IsHtmx() is false, and it renders a fragment of that HTML when Request.IsHtmx() is true, you need to add Vary: HX-Request. That causes the cache to be keyed based on a composite of the response URL and the HX-Request request header — rather than being based just on the response URL.

```c#
// in a Razor Page
if (Request.IsHtmx())
{
  Response.Headers.Add("Vary", "HX-Request");
  return Partial("_Form", this)
}

return Page();
```

You can also use the `WithVary()` helper on response headers (see below).

### HttpResponse

We can set Http Response headers using the `Htmx` extension method, which passes an action and `HtmxResponseHeaders` object.

```c#
Response.Htmx(h => {
    h.PushUrl("/new-url")
     .WithTrigger("cool")
});
```

Read more about the HTTP response headers at the [official documentation site](https://htmx.org/reference/#request_headers).

#### Response Header Methods

The `HtmxResponseHeaders` object provides a fluent API for setting all htmx response headers:

| Method | Header | Description |
|---|---|---|
| `Location(string)` | `HX-Location` | Client-side redirect without full page reload. |
| `Location(HtmxLocation)` | `HX-Location` | Client-side redirect with additional options (target, swap, values, etc.). |
| `PushUrl(string)` | `HX-Push-Url` | Push a new URL into the browser history stack. |
| `PreventPush()` | `HX-Push-Url: false` | Prevent the browser history from being updated. |
| `Redirect(string)` | `HX-Redirect` | Client-side redirect to a new location. |
| `Refresh()` | `HX-Refresh` | Trigger a full client-side page refresh. |
| `ReplaceUrl(string)` | `HX-Replace-Url` | Replace the current URL in the browser location bar. |
| `PreventReplace()` | `HX-Replace-Url: false` | Prevent the browser URL from being updated. |
| `Reswap(string)` | `HX-Reswap` | Specify how the response will be swapped. Use `HtmxSwap` constants. |
| `Reselect(string)` | `HX-Reselect` | CSS selector to choose which part of the response is swapped. |
| `Retarget(string)` | `HX-Retarget` | CSS selector that changes the default target of the returned content. |
| `WithTrigger(...)` | `HX-Trigger` | Trigger a client-side event (see Triggering Client-Side Events below). |
| `WithVary()` | `Vary: HX-Request` | Append `Vary: HX-Request` for proper browser caching. |

#### HtmxLocation

For advanced `HX-Location` usage, use the `HtmxLocation` class to configure all available options:

```c#
Response.Htmx(h => {
    h.Location(new HtmxLocation {
        Path = "/new-page",
        Target = "#content",
        Swap = HtmxSwap.InnerHtml,
        Values = new Dictionary<string, object> { ["key"] = "value" },
        Headers = new Dictionary<string, string> { ["X-Custom"] = "header" },
        Select = "#main-content"
    });
});
```

`HtmxLocation` supports the following properties: `Path`, `Source`, `Event`, `Handler`, `Target`, `Swap`, `Values`, `Headers`, `Select`, `Push`, and `Replace`.

#### HtmxSwap Constants

The `HtmxSwap` static class provides string constants for all htmx swap styles, for use with `Reswap()` or anywhere a swap style is needed:

| Constant | Value | Description |
|---|---|---|
| `HtmxSwap.InnerHtml` | `"innerHTML"` | Replace the inner HTML of the target element. |
| `HtmxSwap.OuterHtml` | `"outerHTML"` | Replace the entire target element with the response. |
| `HtmxSwap.BeforeBegin` | `"beforebegin"` | Insert the response before the target element. |
| `HtmxSwap.AfterBegin` | `"afterbegin"` | Insert the response before the first child of the target element. |
| `HtmxSwap.BeforeEnd` | `"beforeend"` | Insert the response after the last child of the target element. |
| `HtmxSwap.AfterEnd` | `"afterend"` | Insert the response after the target element. |
| `HtmxSwap.Delete` | `"delete"` | Deletes the target element regardless of the response. |
| `HtmxSwap.None` | `"none"` | Does not swap the content. |

```c#
Response.Htmx(h => {
    h.Reswap(HtmxSwap.OuterHtml)
     .Retarget("#another-element");
});
```

#### Triggering Client-Side Events

You can trigger client-side events with HTMX using the `HX-Trigger` header. Htmx.Net provides a `WithTrigger` helper method to configure one or more events you wish to trigger.

```c#
Response.Htmx(h => {
    h.WithTrigger("yes")
     .WithTrigger("cool", timing: HtmxTriggerTiming.AfterSettle)
     .WithTrigger("neat", new { valueForFrontEnd= 42, status= "Done!" }, timing: HtmxTriggerTiming.AfterSwap);
});
```

The `HtmxTriggerTiming` enum controls when the event fires:

| Value | Header | Description |
|---|---|---|
| `Default` | `HX-Trigger` | Triggers immediately when the response is received. |
| `AfterSettle` | `HX-Trigger-After-Settle` | Triggers after the settling step. |
| `AfterSwap` | `HX-Trigger-After-Swap` | Triggers after the swap step. |

Multiple calls to `WithTrigger` are aggregated and properly serialized. Existing trigger headers set outside the `Htmx()` call are preserved and merged.

#### Stop Polling

If you're using htmx polling, you can tell the client to stop by using the `StopPolling` extension method, which returns a `286` status code:

```c#
Response.StopPolling();
```

#### CORS Policy

By default, all Htmx requests and responses will be blocked in a cross-origin context. 

If you configure your application in a cross-origin context, then setting a CORS policy in ASP.NET Core also allows you to define specific restrictions on request and response headers, 
enabling fine-grained control over the data that can be exchanged between your web application and different origins. 

This library provides a simple approach to exposing Htmx headers to your CORS policy:

```c#
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://example.com", "http://www.contoso.com")
                                   .WithHeaders(HtmxRequestHeaders.Keys.All)  // Add htmx request headers
                                   .WithExposedHeaders(HtmxResponseHeaders.Keys.All)  // Add htmx response headers
                      });
});
```

The `HtmxRequestHeaders.Keys` and `HtmxResponseHeaders.Keys` nested classes provide string constants for every htmx header name, plus an `All` array containing all of them for CORS configuration.

## Htmx.TagHelpers

### Getting Started

Install the `Htmx.TagHelpers` [NuGet package](https://www.nuget.org/packages/Htmx.TagHelpers/) to your ASP.NET Core project. Targets .NET Core 3.1+ projects.

```console
dotnet add package Htmx.TagHelpers
```

Make the Tag Helpers available in your project by adding the following line to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, Htmx.TagHelpers
```

### URL Generation

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

### Headers Tag Helper

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

### Htmx.Config

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

#### Supported Configuration Attributes

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

### HTMX and Anti-forgery Tokens

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
                // update DOM, probably not necessary, but for sanity's sake
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

## License

Copyright (c) 2022 Khalid Abuhakmeh

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
