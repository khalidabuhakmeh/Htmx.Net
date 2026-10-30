---
title: Samples
description: Index of the 13 demos in the Htmx4.Sample app, what each shows, and links to the source and matching guide.
---

The sample lives in this project's GitHub repository, [khalidabuhakmeh/Htmx.Net](https://github.com/khalidabuhakmeh/Htmx.Net). Clone it, then run the app:

```bash
git clone https://github.com/khalidabuhakmeh/Htmx.Net.git
cd Htmx.Net
dotnet run --project samples/Htmx4.Sample
```

`samples/Htmx4.Sample/` is a Razor Pages + MVC app on htmx 4.0.0 (vendored in `wwwroot/lib/htmx/`).

Then open the demo you want (each is at `/<Name>`). The sample's `Demos.cs` lists the pages and the layout renders the navigation from it. Each demo ends with a "How it works" panel showing its key markup and server code.

## The 13 demos

| Demo | What it shows | Source | Guide |
|---|---|---|---|
| `/RequestHeaders` | Echoes every request header htmx sent: `IsHtmx`, `Source`/`Target` (parsed into `HtmxElementReference`), `RequestType`, `CurrentUrl`, `Boosted`, history restore, `Prompt`. | [RequestHeaders.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/RequestHeaders.cshtml) | [Request detection](/Htmx.Net/guides/request-detection/) |
| `/ResponseHeaders` | One button per response header (`PushUrl`, `PreventPush`, `ReplaceUrl`, `Redirect`, `Refresh`, `Retarget`, `Reswap`, `Reselect`, `Location`) and every `HtmxSwap` style, with `WithVary()`. | [ResponseHeaders.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/ResponseHeaders.cshtml) | [Response headers](/Htmx.Net/guides/response-headers/) |
| `/Triggers` | `WithTrigger` variants: plain event, scalar detail, object detail, multiple events, merging with an existing `HX-Trigger`, first-wins. | [Triggers.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Triggers.cshtml) | [Triggering events](/Htmx.Net/guides/triggering-events/) |
| `/Config` | Page-scoped `htmx-config` via a layout section covering all config attributes, with `htmx.config` read back; default swap and `noSwap` in action. | [Config.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Config.cshtml) | [Config](/Htmx.Net/guides/config/) |
| `/UrlHelper` | `HtmxUrlTagHelper`: Razor Page handler, MVC controller/action, named route with route values, area, and POST. | [UrlHelper.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/UrlHelper.cshtml) | [Url tag helper](/Htmx.Net/guides/url-tag-helper/) |
| `/Inherited` | Explicit htmx 4 inheritance: `hx-target:inherited` and `hx-headers-*` with `hx-headers-inherited`, compared to a non-inherited parent. | [Inherited.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Inherited.cshtml) | [Url tag helper](/Htmx.Net/guides/url-tag-helper/) |
| `/Antiforgery` | Token in header mode and form mode for POST/PUT/PATCH/DELETE, `[AutoValidateAntiforgeryToken]`, `[ValidateAntiForgeryToken]`, and `HtmxSnippets` / `Html.HtmxAntiforgeryScript()`. | [Antiforgery.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Antiforgery.cshtml) | [Antiforgery](/Htmx.Net/guides/antiforgery/) |
| `/Boosted` | `hx-boost` navigation between pages, `IsHtmxBoosted()` / `IsHtmxNonBoosted()` and a boosted POST. | [Boosted.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Boosted.cshtml) | [Request detection](/Htmx.Net/guides/request-detection/) |
| `/Partials` | `<hx-partial>` multi-target response with OOB swap and main-content-first ordering. | [Partials.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Partials.cshtml) | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| `/Morph` | `outerMorph` and `innerMorph` swaps that preserve typed input, compared with a plain swap. | [Morph.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Morph.cshtml) | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| `/ErrorSwaps` | 4xx/5xx swapping by default, `204`/`304` skipped, a validation POST, and `hx-delete` with `hx-include`. | [ErrorSwaps.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/ErrorSwaps.cshtml) | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| `/Location` | `HX-Location` as a string and as `HtmxLocation` (target, swap, select, selectOOB, values, headers, transition). | [Location.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Location.cshtml) | [Response headers](/Htmx.Net/guides/response-headers/) |
| `/Polling` | The htmx 4 polling pattern: `load delay:1s` and `every 1s`, stopped by replacing the poller (no `StopPolling()`). | [Polling.cshtml](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Polling.cshtml) | [Response headers](/Htmx.Net/guides/response-headers/) |

Fragments are served by Razor Page handlers and by `Controllers/DemoController.cs` (routes under `/demo/...`), and every fragment carries `data-testid` hooks for the tests.

## Supporting pieces

- `Pages/Shared/_Layout.cshtml`: global `htmx-config` meta (replaceable by the `HtmxConfig` section), vendored htmx with SRI, and the antiforgery script.
- `Controllers/DemoController.cs`: the MVC fragment endpoints, including `AntiforgeryDemoController`.
- `Controllers/AntiforgeryTestController.cs`: **test-only** endpoints that simulate a hostile second origin. Not recommended practice.
- `Areas/Admin/Controllers/AdminController.cs`: the area target used by the URL helper demo.

## Proof it works

- `samples/Htmx4.Sample/README.md` has the full API to demo-page checklist for both packages.
- `test/Htmx.IntegrationTests` exercises the sample over HTTP (`WebApplicationFactory`).
- `test/Htmx.E2ETests` drives the sample in a real browser with Playwright (`Category=E2E`), including the antiforgery cross-origin, `<base href>` and redirect scenarios.
- `test/Htmx.Tests` holds unit and snapshot tests.

New here? Start with [Getting started](/Htmx.Net/getting-started/).

## More recipes on the blog

Worked examples that combine htmx.NET with other libraries and patterns:

- [HTMX, ASP.NET Core, and Bootstrap Modals](https://khalidabuhakmeh.com/posts/htmx-aspnet-core-and-bootstrap-modals/)
- [Confirmation Dialogs with Htmx and SweetAlert](https://khalidabuhakmeh.com/posts/confirmation-dialogs-with-htmx-and-sweetalert/)
- [How to Integrate HTMX and Shoelace Web Components](https://khalidabuhakmeh.com/posts/how-to-integrate-htmx-and-shoelace-web-components/)
- [Blazor Server-Rendered Components with HTMX](https://khalidabuhakmeh.com/posts/how-to-use-blazor-server-rendered-components-with-htmx/)
- [View Transitions API with ASP.NET Core and HTMX](https://khalidabuhakmeh.com/posts/view-transitions-api-with-aspnet-core-and-htmx/)
- [Htmx and Playwright Tests in C#](https://khalidabuhakmeh.com/posts/htmx-and-playwright-tests-in-csharp/)
