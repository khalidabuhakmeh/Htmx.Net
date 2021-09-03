# Htmx.Net

![](https://raw.githubusercontent.com/bigskysoftware/htmx/master/www/img/htmx_logo.1.png)

This is a package designed to add server side helper methods for `HttpRequest` and `HttpResponse`. This makes working with [htmx](https://htmx.org/) server-side concepts simpler.

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

This can be used to either return a full page response or a partial page render.

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

### HttpResponse

We can set Http Response headers using the `Htmx` extension method, which passes an action and `HtmxResponseHeaders` object.

```c#
Response.Htmx(h => {
    h.Push("/new-url")
     .TriggerAfterSettle("yes")
     .TriggerAfterSwap("cool");
});
```

Read more about the HTTP response headers at the [official documentation site](https://htmx.org/reference/#request_headers).

## Htmx.TagHelpers

### Getting Started

Install the `Htmx.TagHelpers` [NuGet package](https://www.nuget.org/packages/Htmx.TagHelpers/) to your ASP.NET Core project. Targets .NET Core 3.1+ projects.

```console
dotnet add package Htmx.TagHelpers
```

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

### Htmx.Config

An additional tag helper is included that makes creating htmx configuration simpler. For example, below we can set the `historyCacheSize`, default `indicatorClass`, and whether to include ASP.NET Core's anti-forgery tokens as an additional element on the HTMX configuration.

```html
    <meta name="htmx-config" 
          historyCacheSize="20"
          indicatorClass="htmx-indicator"
          includeAspNetAntiforgeryToken="true"
          />
```

The resulting HTML will be.

```html
<meta name="htmx-config" content='{"indicatorClass":"htmx-indicator","historyCacheSize":20,"antiForgery":{"formFieldName":"__RequestVerificationToken","headerName":"RequestVerificationToken","requestToken":"<token>"}}' />
```

### HTMX and Anti-forgery Tokens

You can set the attribute `includeAspNetAntiforgerToken` on the `htmx-config` element. Then you'll need to include this additional JavaScript in your web application.

```javascript
document.addEventListener("htmx:configRequest", (evt) => {
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
```

## License

Copyright © 2021 Khalid Abuhakmeh

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.