# Changelog

All notable changes to htmx.NET are documented here. This project adheres to
[Semantic Versioning](https://semver.org/).

## [2.0.0] - Unreleased (htmx 4)

> **Breaking change.** htmx.NET 2.x targets **[htmx 4](https://four.htmx.org/) only**. htmx 1.x/2.x
> users should remain on the 1.x line. See the full
> **[Migration guide](https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/)**.

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
