---
title: Config tag helper
description: Render the htmx-config meta tag with typed htmx 4 configuration attributes.
---

The tag helper targets `<meta name="htmx-config" />` (class `HtmxConfigMetaTagHelper`) and writes the `content` JSON that htmx 4 reads as `htmx.config`.

```cshtml
<meta name="htmx-config" includeAspNetAntiforgeryToken="true"
      defaultSwap="outerHTML" transitions="true" noSwap="@(new object[] { 204, 304, 422 })" />
```

**Unset attributes are not serialized**, so htmx defaults apply. Only one `htmx-config` meta may exist per page; the sample uses a layout section to let a page replace the global one (`samples/Htmx4.Sample/Pages/Config.cshtml`).

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

C# properties are `LogAll`, `Prefix`, `MetaCharacter`, `Transitions`, `History`, `Mode`, `DefaultSwap`, `AllowEmptySwapAfterOOB`, `DefaultFocusScroll`, `DefaultSettleDelay`, `IndicatorClass`, `RequestClass`, `IncludeIndicatorCSS`, `DefaultTimeout`, `Extensions`, `MorphIgnore`, `MorphScanLimit`, `MorphSkip`, `MorphSkipChildren`, `NoSwap`, `ImplicitInheritance`, `InlineScriptNonce`, `IncludeAntiForgery` (plus `ViewContext`, injected).

## Renamed or removed from htmx 2

Use `defaultSwap` (was `defaultSwapStyle`), `transitions` (`globalViewTransitions`), `history` (`historyEnabled`), `includeIndicatorCSS` (`includeIndicatorStyles`), `defaultTimeout` (`timeout`). Many htmx 2 keys (`historyCacheSize`, `selfRequestsOnly`, `withCredentials`, `allowEval`, `inlineStyleNonce`, …) no longer exist; see the [htmx 4 facts](/Htmx.Net/reference/htmx4-facts/#4-configuration).

## Effect of config on server code

`defaultSwap` and `noSwap` change what the client does with your responses. By default htmx 4 swaps 4xx/5xx responses; only `204` and `304` are skipped. Add statuses to `noSwap` (or use `hx-status`) to opt out.
