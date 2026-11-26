# Changelog

All notable changes to htmx.NET are documented here. This project adheres to
[Semantic Versioning](https://semver.org/).

## [2.0.0] - Unreleased (htmx 4)

> **Preview.** 2.0.0 is published to NuGet as a **pre-release** while htmx 4 support settles. Install
> with `--prerelease` (`dotnet add package Htmx --prerelease`) and share feedback on
> [GitHub issues](https://github.com/khalidabuhakmeh/Htmx.Net/issues). The stable 2.0.0 follows once
> the preview feedback is in.

> **Breaking change.** htmx.NET 2.x targets **[htmx 4](https://four.htmx.org/) only**. htmx 1.x/2.x
> users should remain on the 1.x line. See the full
> **[Migration guide](https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/)**.

### [2.0.0-preview.2] - 2026-11-26 (htmx 4)

#### Security

- **Antiforgery token no longer leaks into URLs or to other origins.** htmx 4 collects a rendered
  `__RequestVerificationToken` hidden input into the request body for every verb, then moves GET/DELETE
  values into the query string. In 2.0.0-preview.1 this meant: a `<form hx-delete>` (header mode) put the token in
  the URL (logs, history, `Referer`); form-field mode DELETE injected the token into the URL and still failed
  with 400; `<form hx-get>` put a rendered token in the URL; and with `mode: "cors"` or a cross-origin
  `<base href>` the rendered token was sent to the other origin.
- The snippet now strips the rendered token field from safe-method and cross-origin requests, strips the form
  copy in header mode (header is authoritative), and in form-field mode refuses DELETE and any other method htmx 4 sends as URL parameters (it matches `/GET|DELETE/`, unanchored, so e.g. a custom `XGET` also fails closed): the field is stripped, nothing is sent, and `[htmx.net] Antiforgery token not sent for <METHOD> ...` is logged.

#### Fixed

- `<form hx-delete>` with a rendered token works in header mode (200) without exposing the token in the URL.

#### Action required

- Update to 2.0.0-preview.2. Use **header mode** (the ASP.NET Core default header name) for DELETE;
  form-field mode no longer protects DELETE or other URL-parameter methods. No markup changes are needed otherwise.

### Highlights

- **Targets htmx 4.0.0.** Rewritten request/response header handling, config Tag Helper, and
  antiforgery integration for htmx 4's fetch-based pipeline.
- **Target frameworks:** now `net8.0` and `net10.0` (dropped `net6.0`).
- **New documentation site**, samples, and a rigorous test suite (unit, snapshot, integration, and
  Playwright E2E against real htmx 4.0.0).

### Breaking changes

- **URL tag helper attributes renamed `hx-*` → `hxn-*`** (`hxn-page`, `hxn-page-handler`, `hxn-controller`, `hxn-action`, `hxn-area`, `hxn-route[-*]`, `hxn-host`, `hxn-protocol`, `hxn-fragment`, `hxn-all-route-data`) because htmx 4 added a native `hx-action`.

**`Htmx` package**

- Request headers: removed `HtmxRequestHeaders.Trigger` and `TriggerName` (compile-error obsolete) —
  use `Source`/`SourceElement` (new `HX-Source`, `tag#id` format via `HtmxElementReference`). `Target`
  is now `tag#id` format with a new `TargetElement`.
- New `HX-Request-Type` support: `RequestType`, `IsHtmxPartial()`, `IsHtmxFullRequest()`.
- Response triggers: `HtmxTriggerTiming` removed (compile-error obsolete); `WithTrigger(name, detail,
  timing)` → `WithTrigger(name, detail)`. The `HX-Trigger-After-Swap`/`-After-Settle` headers were
  removed from htmx 4 — only `HX-Trigger` is emitted. Removed `Trigger(string)`, `TriggerAfterSwap`,
  `TriggerAfterSettle`, and `Push`.
- `HtmxLocation`: removed `Handler`, `Push`, `Replace`; added `SelectOob` and `Transition`.
- `HtmxSwap`: added `OuterSync`, `InnerMorph`, `OuterMorph`, `TextContent`, and alias constants.
- `StopPolling()` is obsolete (compile-error): htmx 4 no longer stops polling on HTTP 286 — return a
  response without the polling element instead.
- `Keys.All` (CORS) now reflects the htmx 4 header set.

**`Htmx.TagHelpers` package**

- `HtmxConfigMetaTagHelper`: emits only valid htmx 4 config keys. Renamed `defaultSwapStyle`→
  `defaultSwap`, `globalViewTransitions`→`transitions`, `historyEnabled`→`history`,
  `includeIndicatorStyles`→`includeIndicatorCSS`, `timeout`→`defaultTimeout`. Removed many htmx-2-only
  keys and the `HtmxTriggerSpecification*`/`HtmxResponseHandling` types. Added `implicitInheritance`,
  `noSwap`, `prefix`, `extensions`, `mode`, `metaCharacter`, `logAll`, `allowEmptySwapAfterOOB`, and
  the `morph*` keys. Unset keys are no longer serialized (previously some defaults were emitted,
  overriding htmx defaults — fixed).
- URL Tag Helper: a valueless `hx-get` is no longer rewritten (it is valid htmx 4). URL generation now
  keys off the routing attributes (`hxn-page`/`hxn-action`/`hxn-controller`/`hxn-route`) and supports the
  `:inherited` and `data-hx-` spellings. Note: `hx-get:inherited` does not trigger descendant requests
  in htmx 4 — use a direct verb attribute per element, or `hx-boost:inherited` for links/forms.
- Headers Tag Helper: new `hx-headers-inherited` emits `hx-headers:inherited`.
- **Migration aid:** the package emits build warning `HTMX001` (with file and line) for any leftover `hx-*` URL binding attribute in `.cshtml`, suggesting the `hxn-*` replacement. Opt out with `<HtmxMigrationWarnings>false</HtmxMigrationWarnings>` or `<NoWarn>HTMX001</NoWarn>`.
- Antiforgery JavaScript rewritten for htmx 4's `htmx:config:request` pipeline with a `document.baseURI`
  same-origin guard (security-reviewed). **No automatic token refresh**: trigger a full reload after
  mid-session token rotation. DELETE requires header mode. A residual cross-origin-redirect token-leak
  risk exists only when an app opts into `htmx.config.mode:"cors"`.

### Client-side (your Razor markup)

htmx 4 changes attribute inheritance (explicit `:inherited`), event names (`htmx:before:request`,
etc.), `hx-delete` form inclusion (use `hx-include`), non-200 swapping (now swaps by default except
204/304), and OOB ordering. See the
[migration guide](https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/) and the official
[What's new in htmx 4](https://four.htmx.org/docs/whats-new-in-htmx-4).

---

For releases prior to 2.0.0, see the [v1.x documentation](https://khalidabuhakmeh.github.io/Htmx.Net/v1/)
and the `v1-final` git tag.

## [1.13.0] - 2026-11-26 (htmx 1.x/2.x line)

> This entry is for the **1.x line** (htmx 1.x/2.x). It does not apply to htmx.NET 2.0.0 (htmx 4).

### Security

- The antiforgery snippet only sends the token to the page's origin, resolved against `document.baseURI`.
  Rendered token fields are stripped from safe-method and cross-origin requests.

### Fixed

- `<form hx-delete>` returned 400 on htmx 2 and leaked the token into the query string. Header mode now
  always sets the header and strips the form copy.
- Form-field mode no longer sends the token for verbs in `htmx.config.methodsThatUseUrlParams` (htmx 2: DELETE).
  It logs a console warning instead.

### Removed (behavior change)

- Automatic token refresh after boosted responses. It parsed response text, which allowed token-config
  poisoning. Apps that use `hx-boost` and change identity mid-session should send `HX-Refresh: true`
  (`Response.Htmx(h => h.Refresh())`).

### Changed (behavior change)

- `HtmxConfigMetaTagHelper.SelfRequestsOnly` is now `bool?` and omitted unless set. Previously it always
  emitted `selfRequestsOnly:false`, which silently disabled htmx 2's same-origin default.
- The idempotence guard moved to `window.__htmxAntiforgery`. HEAD, OPTIONS, and TRACE are skipped.
- Sample updated to htmx 2.0.11. `global.json` pinned to SDK 10.
