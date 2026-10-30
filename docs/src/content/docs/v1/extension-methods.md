---
title: Htmx Extension Methods (v1.x)
description: HttpRequest and HttpResponse extension methods, headers, triggers and CORS in htmx.NET v1.x.
---

:::caution[Previous version]
This documents **htmx.NET v1.x** for **htmx 1.x / 2.x**. The current version targets **htmx 4** and has breaking changes. See the [Migration guide](/Htmx.Net/migration/v1-to-v2/) and the current docs.
:::

:::note
The full v1 source is available at the [`v1-final` git tag](https://github.com/khalidabuhakmeh/Htmx.Net/tree/v1-final). The frozen legacy sample lives in `test/Sample` (see its README).
:::


## Getting Started

Install the `Htmx` [NuGet package](https://www.nuget.org/packages/Htmx/) to your ASP.NET Core project.

```console
dotnet add package Htmx
```

## HttpRequest

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

### Additional Request Checks

There are additional extension methods for common htmx request checks:

```c#
// Was the request boosted via hx-boost?
Request.IsHtmxBoosted()

// Is this a non-boosted htmx request?
Request.IsHtmxNonBoosted()

// Also available with out parameter for header values
Request.IsHtmxNonBoosted(out var values)

// Is this a history restore request after a local cache miss?
Request.IsHtmxHistoryRestoreRequest()
```

### HtmxRequestHeaders

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

### Browser Caching

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

## HttpResponse

We can set Http Response headers using the `Htmx` extension method, which passes an action and `HtmxResponseHeaders` object.

```c#
Response.Htmx(h => {
    h.PushUrl("/new-url")
     .WithTrigger("cool")
});
```

Read more about the HTTP response headers at the [official documentation site](https://htmx.org/reference/#request_headers).

### Response Header Methods

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

### HtmxLocation

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

### HtmxSwap Constants

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

### Triggering Client-Side Events

You can trigger client-side events with HTMX using the `HX-Trigger` header. htmx.NET provides a `WithTrigger` helper method to configure one or more events you wish to trigger.

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

### Stop Polling

If you're using htmx polling, you can tell the client to stop by using the `StopPolling` extension method, which returns a `286` status code:

```c#
Response.StopPolling();
```

### CORS Policy

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
                                   // Add htmx request headers
                                   .WithHeaders(HtmxRequestHeaders.Keys.All)
                                   // Add htmx response headers
                                   .WithExposedHeaders(HtmxResponseHeaders.Keys.All)
                      });
});
```

The `HtmxRequestHeaders.Keys` and `HtmxResponseHeaders.Keys` nested classes provide string constants for every htmx header name, plus an `All` array containing all of them for CORS configuration.

