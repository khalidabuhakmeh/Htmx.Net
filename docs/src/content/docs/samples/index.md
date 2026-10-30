---
title: Samples
description: The Htmx4.Sample app demonstrates every Htmx and Htmx.TagHelpers feature, backed by integration and E2E tests.
---

`samples/Htmx4.Sample/` is a Razor Pages + MVC app on htmx 4.0.0 (vendored in `wwwroot/lib/htmx/`). Run it:

```bash
dotnet run --project samples/Htmx4.Sample
```

| Demo page | Guide |
|---|---|
| RequestHeaders | [Request detection](/Htmx.Net/guides/request-detection/) |
| ResponseHeaders | [Response headers](/Htmx.Net/guides/response-headers/) |
| Triggers | [Triggering events](/Htmx.Net/guides/triggering-events/) |
| Config | [Config tag helper](/Htmx.Net/guides/config/) |
| UrlHelper | [URL tag helper](/Htmx.Net/guides/url-tag-helper/) |
| Inherited | [Headers tag helper](/Htmx.Net/guides/headers-tag-helper/) |
| Antiforgery | [Antiforgery](/Htmx.Net/guides/antiforgery/) |
| Boosted | [Request detection](/Htmx.Net/guides/request-detection/) (`IsHtmxBoosted`) |
| Partials | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| Morph | [Partials and morph](/Htmx.Net/guides/partials-and-morph/) |
| ErrorSwaps | [Partials and morph](/Htmx.Net/guides/partials-and-morph/), [URL tag helper](/Htmx.Net/guides/url-tag-helper/) (`hx-delete`) |
| Location | [Response headers](/Htmx.Net/guides/response-headers/) (`HtmxLocation`) |
| Polling | [Response headers](/Htmx.Net/guides/response-headers/) (stopping polling) |

## Proof it works

- `samples/Htmx4.Sample/README.md` has the full API → demo-page checklist for both packages.
- `test/Htmx.IntegrationTests` exercises the sample over HTTP (`WebApplicationFactory`).
- `test/Htmx.E2ETests` drives the sample in a real browser with Playwright (`Category=E2E`), including the antiforgery cross-origin, `<base href>` and redirect scenarios.
- `test/Htmx.Tests` holds unit and snapshot tests.
