---
title: htmx 4.0.0 verified facts (Task A4)
description: Authoritative, citation-backed htmx 4.0.0 contract that all Htmx.Net upgrade tasks must follow.
---

> **Status: VERIFIED (Task A4).** This file is the single source of truth for the htmx 4 upgrade.
> Implementation code (Phases B, C, D) must follow this document, not memory or the brief.
> When the official v4 docs and the upgrade skill conflict, the v4 docs win. Every row cites a source.

# htmx 4.0.0 facts for Htmx.Net

This reference summarizes the official htmx 4 request/response contract and migration points relevant to a server-side helper library. It is based primarily on the htmx v4 reference and upgrade guide; the v4.0.0 release source is used where it clarifies exact request-header construction. Each factual row or statement links to its source.

## 1. Request headers

| Header | v4 status and format | Notes |
|---|---|---|
| `HX-Request` | **Present.** Always `true` on core htmx requests. | Indicates that htmx made the request. [HX-Request reference](https://four.htmx.org/reference/headers/HX-Request) |
| `HX-Source` | **Present; replaces request-side `HX-Trigger`.** Format is `<tag>#<id>`, or just `<tag>` when the source has no ID; it is not empty in that case. The tag is lowercase in the v4.0.0 implementation, and IDs are URI-encoded. | Examples: `button#submit`; `div`. The release source builds this value from `tagName.toLowerCase()` and appends the encoded ID only when present. [HX-Source reference](https://four.htmx.org/reference/headers/HX-Source) · [v4.0.0 source](https://github.com/bigskysoftware/htmx/blob/v4.0.0/dist/htmx.js) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#request-headers) |
| `HX-Target` | **Present only when a target is specified.** Format is `<tag>#<id>`, or just `<tag>` for a target without an ID. The tag is lowercase in the v4.0.0 implementation. | This is an element identifier, not the old bare-ID-only format. [HX-Target reference](https://four.htmx.org/reference/headers/HX-Target) · [v4.0.0 source](https://github.com/bigskysoftware/htmx/blob/v4.0.0/dist/htmx.js) |
| `HX-Request-Type` | **New in v4.** Exact values are `full` and `partial`. | `full` is sent when the target is `body` or `hx-select` is used. `partial` is sent for another target, including the default source-element target. The value is calculated from the final target/selection before the request is sent. [HX-Request-Type reference](https://four.htmx.org/reference/headers/HX-Request-Type) · [v4.0.0 source](https://github.com/bigskysoftware/htmx/blob/v4.0.0/dist/htmx.js) |
| `HX-Trigger-Name` | **Removed.** | The upgrade guide says to use `HX-Source` instead. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#request-headers) |
| `HX-Boosted` | **Present when applicable; value `true`.** | Sent when the request comes from a link or form enhanced by `hx-boost`. [HX-Boosted reference](https://four.htmx.org/reference/headers/HX-Boosted) |
| `HX-Current-URL` | **Present.** Contains the browser URL when the request was made. | Example format: `https://example.com/products`. [HX-Current-URL reference](https://four.htmx.org/reference/headers/HX-Current-URL) |
| `HX-History-Restore-Request` | **Present when applicable; value `true`.** | Sent when the user navigates back or forward in history. In v4, history restoration re-fetches from the server by default rather than restoring from the former built-in localStorage cache. [HX-History-Restore-Request reference](https://four.htmx.org/reference/headers/HX-History-Restore-Request) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#no-history-cache) |
| `HX-Prompt` | **Not sent by core.** Available through the `hx-prompt` extension. | The extension sends the user's answer in this header; canceling the prompt prevents the request. [hx-prompt extension](https://four.htmx.org/extensions/hx-prompt) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#request-headers) |
| `Accept` | **New core request header.** Core requests send `text/html`. | The upgrade guide notes that the `hx-sse` extension adds `text/event-stream`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#request-headers) |

The v4 reference's request-header section lists `HX-Request`, `HX-Request-Type`, `HX-Current-URL`, `HX-Source`, `HX-Target`, `HX-Boosted`, and `HX-History-Restore-Request` as the core request headers. [Request-header reference index](https://four.htmx.org/reference/headers)

## 2. Response headers

| Header | v4 status and semantics |
|---|---|
| `HX-Trigger` | **Supported.** Triggers client-side events after the swap completes. See the JSON detail format below. [HX-Trigger reference](https://four.htmx.org/reference/headers/HX-Trigger) |
| `HX-Trigger-After-Swap` | **Removed.** v4 consolidates the old trigger headers into `HX-Trigger`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#response-headers) |
| `HX-Trigger-After-Settle` | **Removed.** v4 consolidates the old trigger headers into `HX-Trigger`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#response-headers) |
| `HX-Push-Url` | **Supported.** Pushes a URL into browser history; `false` prevents a history update. [HX-Push-Url reference](https://four.htmx.org/reference/headers/HX-Push-Url) |
| `HX-Replace-Url` | **Supported.** Replaces the current history URL without adding a history entry; `false` prevents a URL update. [HX-Replace-Url reference](https://four.htmx.org/reference/headers/HX-Replace-Url) |
| `HX-Redirect` | **Supported.** Performs a full browser navigation by setting `location.href`. [HX-Redirect reference](https://four.htmx.org/reference/headers/HX-Redirect) |
| `HX-Location` | **Supported.** Starts htmx navigation without a full page load. [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location) |
| `HX-Refresh` | **Supported.** When `true`, reloads the full page. [HX-Refresh reference](https://four.htmx.org/reference/headers/HX-Refresh) |
| `HX-Retarget` | **Supported.** Overrides the response target with a CSS selector. [HX-Retarget reference](https://four.htmx.org/reference/headers/HX-Retarget) |
| `HX-Reswap` | **Supported.** Overrides the swap style; accepts the same values as `hx-swap`, including modifiers. [HX-Reswap reference](https://four.htmx.org/reference/headers/HX-Reswap) |
| `HX-Reselect` | **Supported.** Overrides the CSS selector used to select content from the response. [HX-Reselect reference](https://four.htmx.org/reference/headers/HX-Reselect) |

Response headers are not processed on 3xx responses; use a 2xx response when relying on these htmx headers. [HX-Trigger reference](https://four.htmx.org/reference/headers/HX-Trigger) · [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location)

### `HX-Location` JSON keys

`HX-Location` can be a path string or JSON. The JSON must include `path`. The documented htmx.ajax option fields are `source`, `event`, `target`, `swap`, `select`, `selectOOB`, `transition`, `values`, and `headers`. The HX-Location page permits serializable htmx.ajax options; the htmx.ajax reference documents those fields. **`handler` is not listed as a supported option.** [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location) · [htmx.ajax reference](https://four.htmx.org/reference/methods/htmx-ajax)

| JSON key | Documented? |
|---|---|
| `path` | **Yes; required for the object form.** [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location) |
| `target`, `swap`, `select`, `values`, `headers` | **Yes.** [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location) · [htmx.ajax options](https://four.htmx.org/reference/methods/htmx-ajax#options) |
| `source`, `event`, `selectOOB`, `transition` | **Listed htmx.ajax options**, hence within the HX-Location page's "serializable options" description. [HX-Location reference](https://four.htmx.org/reference/headers/HX-Location) · [htmx.ajax options](https://four.htmx.org/reference/methods/htmx-ajax#options) |
| `handler` | **Not listed** among documented htmx.ajax options. [htmx.ajax options](https://four.htmx.org/reference/methods/htmx-ajax#options) |

The documentation does not define whether every listed htmx.ajax option is meaningful when serialized into an HTTP header (notably `event`). Treat those fields as "documented options," not as a guarantee that every JSON value form is useful.

### `HX-Trigger` JSON detail

v4 documents event names as top-level JSON keys. A scalar value is exposed as `event.detail.value`; an object value has its properties copied onto the event detail; an object can include `target` to dispatch on another element. The upgrade guide labels the header **unchanged**, and the v4 reference describes these JSON-detail forms. [HX-Trigger reference](https://four.htmx.org/reference/headers/HX-Trigger) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#response-headers)

## 3. Status-code behavior and polling

| Question | v4 fact |
|---|---|
| Does status `286` still stop polling? | The official v4 upgrade guide does not document `286` as a polling stop signal. The v4.0.0 source's `every` trigger uses a timer that continues firing while its element remains connected; it does not contain a `286` stop branch. [Polling documentation](https://four.htmx.org/docs#polling) · [v4.0.0 source](https://github.com/bigskysoftware/htmx/blob/v4.0.0/dist/htmx.js) |
| What replaces `286`? | There is **no documented status-code replacement** in the official v4 docs. The documented load-polling pattern continues by having each response replace the polling element with another element configured to poll; stop by returning content that does not contain the poller. For an `every` trigger, the source timer continues while the trigger element remains connected, so the docs do not establish an equivalent status-based stop mechanism. [Load polling documentation](https://four.htmx.org/docs#load-polling) · [v4.0.0 source](https://github.com/bigskysoftware/htmx/blob/v4.0.0/dist/htmx.js) |
| Which statuses do not swap by default? | The default `htmx.config.noSwap` is `[204, 304]`. `noSwap` is configurable. [noSwap config reference](https://four.htmx.org/reference/config/htmx-config-noSwap) |
| Do 4xx/5xx responses swap by default? | **Yes.** v4 swaps all responses by default except configured `noSwap` statuses; the default exclusions are 204 and 304. Use `hx-status` or `noSwap` to change behavior. [hx-status reference](https://four.htmx.org/reference/attributes/hx-status) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#error-responses-swap) |

## 4. Configuration

The official `htmx.config` reference provides the complete available-options list below. Types are the JSON/JavaScript types implied by the documented defaults; `undefined` means the reference does not provide a concrete default value. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config)

| v4 key | Type and default | Source |
|---|---|---|
| `logAll` | boolean; `false` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `prefix` | string; `"data-hx-"` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [prefix reference](https://four.htmx.org/reference/config/htmx-config-prefix) |
| `metaCharacter` | string or `undefined`; `undefined` (uses `:`) | [config reference](https://four.htmx.org/reference/config/htmx-config) · [metaCharacter reference](https://four.htmx.org/reference/config/htmx-config-metaCharacter) |
| `transitions` | boolean; `false` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `history` | boolean or string mode; `true` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `mode` | string; `"same-origin"` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [mode reference](https://four.htmx.org/reference/config/htmx-config-mode) |
| `defaultSwap` | string; `"innerHTML"` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `allowEmptySwapAfterOOB` | boolean; `false` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `defaultFocusScroll` | boolean; `false` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `defaultSettleDelay` | number (milliseconds); `1` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [defaultSettleDelay reference](https://four.htmx.org/reference/config/htmx-config-defaultSettleDelay) |
| `indicatorClass` | string; `"htmx-indicator"` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [indicatorClass reference](https://four.htmx.org/reference/config/htmx-config-indicatorClass) |
| `requestClass` | string; `"htmx-request"` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [requestClass reference](https://four.htmx.org/reference/config/htmx-config-requestClass) |
| `includeIndicatorCSS` | boolean; `true` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `defaultTimeout` | number (milliseconds); `60000` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [defaultTimeout reference](https://four.htmx.org/reference/config/htmx-config-defaultTimeout) |
| `extensions` | string; `""` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `morphIgnore` | array of strings; `["data-htmx-powered"]` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `morphSkip` | string (CSS selector); `"[hx-morph-skip]"` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `morphSkipChildren` | string (CSS selector); `"[hx-morph-skip-children]"` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `morphScanLimit` | number; `10` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `noSwap` | array of numbers and/or status-pattern strings; `[204, 304]` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [noSwap reference](https://four.htmx.org/reference/config/htmx-config-noSwap) |
| `implicitInheritance` | boolean; `false` | [config reference](https://four.htmx.org/reference/config/htmx-config) |
| `inlineScriptNonce` | string or `undefined`; `undefined` | [config reference](https://four.htmx.org/reference/config/htmx-config) · [inlineScriptNonce reference](https://four.htmx.org/reference/config/htmx-config-inlineScriptNonce) |

### Renames and requested legacy keys

| htmx 2 name or candidate | v4 finding |
|---|---|
| `defaultSwapStyle` | Renamed to `defaultSwap`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `globalViewTransitions` | Renamed to `transitions`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `historyEnabled` | Renamed to `history`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `includeIndicatorStyles` | Renamed to `includeIndicatorCSS`. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `timeout` | Renamed to `defaultTimeout`; v4 default is `60000` ms. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) · [defaultTimeout reference](https://four.htmx.org/reference/config/htmx-config-defaultTimeout) |
| `implicitInheritance` | New v4 config key; default `false`. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) |
| `noSwap` | v4 config key; default `[204, 304]`. [noSwap reference](https://four.htmx.org/reference/config/htmx-config-noSwap) |
| `prefix` | v4 config key; default `"data-hx-"`. It is additive to the always-recognized `hx-` prefix; setting a custom value replaces the secondary `data-hx-` prefix. [prefix reference](https://four.htmx.org/reference/config/htmx-config-prefix) |
| `extensions` | v4 config key; default `""`. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) |
| `allowEmptySwapAfterOOB` | v4 config key; default `false`. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) |
| `allowEmptySwap` | **Not a listed v4 config key.** The relevant config name is `allowEmptySwapAfterOOB`; `swapEmpty` is an `hx-swap` modifier. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) · [swap options](https://four.htmx.org/docs#swap-options) |
| `indicatorClass`, `requestClass` | Survive as config keys. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) |
| `inlineScriptNonce` | Survives as a config key. [inlineScriptNonce reference](https://four.htmx.org/reference/config/htmx-config-inlineScriptNonce) |
| `inlineStyleNonce` | Removed; the upgrade guide says indicator CSS now uses Constructable Stylesheets, so an inline-style nonce is no longer needed. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `defaultSettleDelay` | Survives; default changed from 20 ms to 1 ms. [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `defaultFocusScroll` | Present in the v4 config reference. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) |
| `reportValidityOfForms` | Not listed in the v4 config reference or v4 migration config table. Its status as a prior-version setting is not clarified there. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |
| `historyRestoreAsHxRequest` | Not listed in the v4 config reference or v4 migration config table. [htmx.config reference](https://four.htmx.org/reference/config/htmx-config) · [Upgrade guide](https://four.htmx.org/docs/whats-new-in-htmx-4#config) |

The v4 upgrade guide explicitly lists the following as **removed** with no v4 config equivalent: `refreshOnHistoryMiss`, `historyCacheSize`, `defaultSwapDelay`, `addedClass`, `settlingClass`, `swappingClass`, `allowEval`, `allowScriptTags`, `attributesToSettle`, `useTemplateFragments`, `wsReconnectDelay`, `wsBinaryType`, `disableSelector`, `withCredentials`, `scrollBehavior`, `getCacheBusterParam`, `methodsThatUseUrlParams`, `selfRequestsOnly`, `ignoreTitle`, `scrollIntoViewOnBoost`, `triggerSpecsCache`, `allowNestedOobSwaps`, and `responseHandling`. The guide notes `disableSelector` is replaced by `hx-ignore`, `selfRequestsOnly` by `htmx.config.mode`, `withCredentials` by `hx-config`, and `responseHandling` by `hx-status` with `noSwap`; `ignoreTitle` remains available as an `hx-swap` option. [Upgrade guide: config](https://four.htmx.org/docs/whats-new-in-htmx-4#config)

## 5. Events and event detail

### Request configuration and CSRF

The exact event name is **`htmx:config:request`**. It fires after values are collected and validated but before request data is encoded and sent. Its `evt.detail.ctx.request` object exposes `method`, `action`, `headers`, and `body`, among other request properties. `method` is the HTTP method string (for example, `"GET"` or `"POST"`). At this event, `body` is a `FormData` object when applicable, so `.set(name, value)` is the documented way to add/update a body field. GET and DELETE requests have a `null` body before encoding; use the method to distinguish them, or add CSRF data as a request header. [config:request reference](https://four.htmx.org/reference/events/htmx-config-request) · [Events guide: `ctx.request`](https://four.htmx.org/docs/htmx-events-guide#ctxrequest) · [Events guide: adding a value to every request](https://four.htmx.org/docs/htmx-events-guide#add-a-value-to-every-request)

```js
htmx.on('htmx:config:request', (evt) => {
  const request = evt.detail.ctx.request;

  if (request.method !== 'GET' && request.method !== 'DELETE' && request.body) {
    request.body.set('csrf-token', getToken());
  }

  // Alternatively, send the token as a header for all methods:
  request.headers['X-CSRF-Token'] = getToken();
});
```

The example is an implementation pattern based on the documented event API, not a prescribed ASP.NET Core antiforgery convention. [Events guide: adding a value to every request](https://four.htmx.org/docs/htmx-events-guide#add-a-value-to-every-request)

### Post-response content

The event is **`htmx:after:request`**. It fires after the response body has been read and before swapping. Response status is at `evt.detail.ctx.response.status`; the response text is at **`evt.detail.ctx.text`**. `ctx.response` contains the raw Fetch `Response`, status, and headers; it is not where the guide documents the text string. [after:request reference](https://four.htmx.org/reference/events/htmx-after-request) · [Events guide: `ctx` object](https://four.htmx.org/docs/htmx-events-guide#the-ctx-object)

### Former `htmx:afterOnLoad`

The upgrade guide maps **`htmx:afterOnLoad` → `htmx:after:init`**. In the v4 reference, `htmx:after:init` fires after an element is fully initialized; it is not described as an event specifically for boosted navigation completion. For a swap-completion hook, the v4 lifecycle includes `htmx:after:swap` and `htmx:after:settle`. [Upgrade guide: events](https://four.htmx.org/docs/whats-new-in-htmx-4#events) · [after:init reference](https://four.htmx.org/reference/events/htmx-after-init) · [Events guide: request/swap lifecycle](https://four.htmx.org/docs/htmx-events-guide#the-event-lifecycle)

## 6. Attribute model and empty `hx-get`

- Attribute inheritance is explicit by default in v4. Use the `:inherited` modifier, for example `hx-target:inherited="#result"`. [Upgrade guide: explicit inheritance](https://four.htmx.org/docs/whats-new-in-htmx-4#explicit-inheritance) · [Attribute inheritance documentation](https://four.htmx.org/docs#attribute-inheritance)
- `hx-disinherit` and `hx-inherit` are removed; the upgrade guide says they are no longer needed with explicit inheritance. [Upgrade guide: attribute removals](https://four.htmx.org/docs/whats-new-in-htmx-4#attributes)
- `prefix` defaults to `"data-hx-"`. The secondary prefix works alongside `hx-`; if a custom prefix is set, it replaces the secondary prefix. [prefix reference](https://four.htmx.org/reference/config/htmx-config-prefix)
- An empty `hx-get` value is valid: it requests the current URL and swaps the current HTML page. [hx-get reference](https://four.htmx.org/reference/attributes/hx-get)

## 7. Swap styles

The v4 docs list these valid swap styles and aliases:

| v4 token | Meaning / alias |
|---|---|
| `innerHTML` | Default; inserts response content inside the target. |
| `outerHTML` | Replaces the target element. |
| `outerSync` | Morphs target attributes, then replaces its children while retaining the target. |
| `before` / `beforebegin` | Insert before the target in its parent. |
| `prepend` / `afterbegin` | Insert before the target's first child. |
| `append` / `beforeend` | Insert after the target's last child. |
| `after` / `afterend` | Insert after the target in its parent. |
| `delete` | Remove the target. |
| `none` | Do not append response content; OOB swaps and response headers are still processed. |
| `innerMorph` | Morph the target's children. |
| `outerMorph` | Morph the target itself. |
| `textContent` | Set target text without parsing HTML. |

All tokens and descriptions above are documented in the v4 swap-style table. [Swap styles documentation](https://four.htmx.org/docs#configuring-swaps) · [What's New: swap styles](https://four.htmx.org/docs/whats-new-in-htmx-4#hx-swap-styles)

## 8. `<hx-partial>` and OOB ordering

`<hx-partial>` wraps response content and directs it to a target with its own optional swap strategy. It can use `hx-target` or `id` as the target shorthand; the default swap is `innerHTML`. For a response containing only partials, the main target is left untouched. [\<hx-partial\> reference](https://four.htmx.org/reference/tags/hx-partial)

In v4, the **main content swap occurs first**, then OOB and `<hx-partial>` swaps occur afterward in document order. This reverses the v2 OOB-before-main ordering. [Upgrade guide: OOB swap order](https://four.htmx.org/docs/whats-new-in-htmx-4#oob-swap-order)

## 9. `Vary` and caching

The v4 caching guidance says to use `Vary: HX-Request` when the same URL returns a full page for non-htmx requests and a fragment for `HX-Request: true`. It also states the general rule: if a response varies based on other request headers, include those headers in `Vary`. [Caching documentation](https://four.htmx.org/docs#caching)

`HX-Request-Type` is the v4 header that distinguishes full from partial requests. The v4 caching example does **not** explicitly mention it. Therefore, if an application varies its representation specifically by `HX-Request-Type`, including `HX-Request-Type` in `Vary` follows the documented general rule; this is an inference from the two docs, not an explicit v4 caching recommendation. [HX-Request-Type reference](https://four.htmx.org/reference/headers/HX-Request-Type) · [Caching documentation](https://four.htmx.org/docs#caching)

## Unresolved / ambiguous (raise with maintainer before Phase B)

1. **Polling stop signal:** the official v4 docs do not state whether any replacement status code exists for v2's `286`. The v4.0.0 source shows `every` timers continue while their element remains connected; the documented load-polling pattern instead continues by returning a replacement poller. **Implication for library:** `StopPolling()` (status 286) is no longer honored by htmx 4 → per plan B2 it becomes `[Obsolete(error:true)]`.
2. **Caching on `HX-Request-Type`:** the caching section's specific example mentions only `HX-Request`; it does not explicitly discuss `HX-Request-Type`. The general `Vary` rule implies varying on `HX-Request-Type` when representations depend on it.
3. **`HX-Location` serialization of `event`:** the docs list `event` as an htmx.ajax option and say HX-Location accepts serializable htmx.ajax options, but do not explain what serialized event value is useful or accepted. `handler` is **not** listed → `HtmxLocation.Handler` should be removed/obsoleted in B3.
4. **`reportValidityOfForms` and `historyRestoreAsHxRequest`:** neither appears in the complete v4 config list or the upgrade guide's removed-config list, so the docs do not clearly state whether these were prior-version config keys that were intentionally removed or simply unsupported names. Treat as removed from the config tag helper.
5. **`HX-Trigger` equivalence:** the upgrade guide calls the header unchanged, while the v4 reference documents its JSON behavior. Preserve the v4-documented `value`, copied-object-properties, and `target` behavior when implementing helpers.
