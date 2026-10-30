---
title: Config tag helper
description: Render the htmx-config meta tag from Razor with typed htmx 4 configuration attributes, page-scoped overrides via layout sections, and the effect of config on server responses.
---

htmx 4 reads its settings from a `<meta name="htmx-config">` tag. Writing that JSON by hand is error prone, so `HtmxConfigMetaTagHelper` lets you set each option as a typed attribute and serializes the `content` for you. Reach for it whenever you change a default (swap style, timeout, `noSwap` statuses, morph options) or want the antiforgery token embedded.

Enable it with `@addTagHelper *, Htmx.TagHelpers` in `_ViewImports.cshtml`, then put the tag in your layout `<head>` **before** the htmx script:

```razor
<meta name="htmx-config" includeAspNetAntiforgeryToken="true"
      defaultSwap="outerHTML" transitions="true" noSwap="@(new object[] { 204, 304, 422 })" />
```

which renders roughly as:

```html
<meta name="htmx-config" content='{"defaultSwap":"outerHTML","transitions":true,"noSwap":[204,304,422],"antiForgery":{...}}' />
```

**Unset attributes are not serialized**, so htmx's own defaults apply.

## Typed value examples

```razor
<meta name="htmx-config"
      logAll="false"
      history="true"
      mode="same-origin"
      defaultTimeout="45000"
      defaultSettleDelay="5"
      indicatorClass="loading-dot" requestClass="is-loading"
      morphIgnore="@(new[] { "data-htmx-powered", "data-keep" })"
      morphSkip="[data-skip]" morphSkipChildren="[data-skip-children]" morphScanLimit="7"
      noSwap="@(new object[] { 204, 304, 422, "5xx" })"
      inlineScriptNonce="@nonce" />
```

- `bool?`, `int?` and `string?` attributes are omitted when not set.
- `morphIgnore` takes a `string[]`; `noSwap` takes `object[]` (status numbers and/or status-pattern strings).
- `history` is typed as a **string** (`"true"`, `"false"` or a mode), so `history="true"` reads back as `"true"` in `htmx.config`.
- `includeAspNetAntiforgeryToken="true"` (C# `IncludeAntiForgery`) writes an `antiForgery` object with the header name, form field name and request token. See [Antiforgery](/Htmx.Net/guides/antiforgery/).

## Page-scoped config with a layout section

Only one `htmx-config` meta may exist on a page. To let an individual page replace the global config, make the layout render a section when it is defined:

```razor
@* _Layout.cshtml *@
@if (IsSectionDefined("HtmxConfig"))
{
    @await RenderSectionAsync("HtmxConfig")
}
else
{
    <meta name="htmx-config" includeAspNetAntiforgeryToken="true" indicatorClass="htmx-indicator" />
}
```

```razor
@* Config.cshtml *@
@section HtmxConfig {
    <meta name="htmx-config" includeAspNetAntiforgeryToken="true"
          defaultSwap="beforeend" defaultTimeout="45000"
          noSwap="@(new object[] { 204, 304, 422 })" implicitInheritance="false" />
}
```

:::caution
A page-scoped meta **replaces** the global one. Re-add `includeAspNetAntiforgeryToken="true"` (and anything else you need) or antiforgery stops working on that page.
:::

## Attribute reference

| Attribute | Type | htmx 4 default |
|---|---|---|
| `logAll` | bool | `false` |
| `prefix` | string | `"data-hx-"` |
| `metaCharacter` | string | `:` |
| `transitions` | bool | `false` |
| `history` | string / bool | `true` |
| `mode` | string | `"same-origin"` |
| `defaultSwap` | string | `"innerHTML"` |
| `allowEmptySwapAfterOOB` | bool | `false` |
| `defaultFocusScroll` | bool | `false` |
| `defaultSettleDelay` | int (ms) | `1` |
| `indicatorClass` | string | `"htmx-indicator"` |
| `requestClass` | string | `"htmx-request"` |
| `includeIndicatorCSS` | bool | `true` |
| `defaultTimeout` | int (ms) | `60000` |
| `extensions` | string | `""` |
| `morphIgnore` | string[] | `["data-htmx-powered"]` |
| `morphSkip` | string (selector) | `"[hx-morph-skip]"` |
| `morphSkipChildren` | string (selector) | `"[hx-morph-skip-children]"` |
| `morphScanLimit` | int | `10` |
| `noSwap` | object[] (numbers / status patterns) | `[204, 304]` |
| `implicitInheritance` | bool | `false` |
| `inlineScriptNonce` | string | none |
| `includeAspNetAntiforgeryToken` | bool | `false` (library-specific; see [Antiforgery](/Htmx.Net/guides/antiforgery/)) |



## Tag helper members

`HtmxConfigMetaTagHelper` exposes one property per attribute: `LogAll`, `Prefix`, `MetaCharacter`, `Transitions`, `History`, `Mode`, `DefaultSwap`, `AllowEmptySwapAfterOOB`, `DefaultFocusScroll`, `DefaultSettleDelay`, `IndicatorClass`, `RequestClass`, `IncludeIndicatorCSS`, `DefaultTimeout`, `Extensions`, `MorphIgnore`, `MorphScanLimit`, `MorphSkip`, `MorphSkipChildren`, `NoSwap`, `ImplicitInheritance`, `InlineScriptNonce`, `IncludeAntiForgery`, plus the injected `ViewContext` and the overridden `Process(...)`.

## Renamed or removed from htmx 2

Use `defaultSwap` (was `defaultSwapStyle`), `transitions` (`globalViewTransitions`), `history` (`historyEnabled`), `includeIndicatorCSS` (`includeIndicatorStyles`), `defaultTimeout` (`timeout`). Many htmx 2 keys (`historyCacheSize`, `selfRequestsOnly`, `withCredentials`, `allowEval`, `inlineStyleNonce`, …) no longer exist; see the [htmx 4 reference](/Htmx.Net/reference/htmx4-facts/#4-configuration).

## Effect of config on server code

`defaultSwap` and `noSwap` change what the client does with your responses. By default htmx 4 swaps 4xx/5xx responses; only `204` and `304` are skipped. Add statuses to `noSwap` (or use `hx-status`) to opt out.


Examples you will feel on the server:

- `defaultSwap="beforeend"` makes responses to elements without `hx-swap` append instead of replace.
- `noSwap` including `422` means a validation fragment returned with `422` is **not** shown. Leave it out to render validation errors (htmx 4 swaps them by default).
- `implicitInheritance="true"` restores htmx 2-style attribute inheritance; keep it `false` to use explicit `:inherited` (see [URL tag helper](/Htmx.Net/guides/url-tag-helper/#inheritance-in-htmx-4)).
- `prefix` is additive to `hx-`; a custom value replaces the secondary `data-hx-` prefix. The tag helpers recognize both `hx-` and `data-hx-` spellings.

---

See the live demo: run `dotnet run --project samples/Htmx4.Sample` and open `/Config` ([source](https://github.com/khalidabuhakmeh/Htmx.Net/blob/main/samples/Htmx4.Sample/Pages/Config.cshtml)). It reads `htmx.config` back so you can see what the browser received.

Related: [htmx 4 reference: configuration](/Htmx.Net/reference/htmx4-facts/#4-configuration), [Antiforgery](/Htmx.Net/guides/antiforgery/), [Partials and morph](/Htmx.Net/guides/partials-and-morph/).
