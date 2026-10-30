# Migrating to htmx.NET 2.0

htmx.NET 2.0 targets **htmx 4** (breaking). It requires `net8.0` or `net10.0`.

Full guide: <https://khalidabuhakmeh.github.io/Htmx.Net/migration/v1-to-v2/>
(source: [`docs/src/content/docs/migration/v1-to-v2.md`](docs/src/content/docs/migration/v1-to-v2.md))

Top breaking changes:

1. **TFM**: `net6.0`/`net7.0` dropped; use `net8.0` or `net10.0`.
2. **Request headers**: `HtmxRequestHeaders.Trigger`/`TriggerName` removed; use `Source`/`SourceElement` (`HX-Source`, `tag#id`). `Target` is now `tag#id` too (`TargetElement`).
3. **Response triggers**: `HtmxTriggerTiming`, `WithTrigger(name, detail, timing)`, `TriggerAfterSwap/Settle`, `Trigger(string)`, and `Push` removed; use `WithTrigger(name, detail)` and `PushUrl`. `StopPolling()` no longer works (htmx 4 drops status 286).
4. **`<htmx-config>`**: many attributes renamed (`defaultSwapStyle`→`defaultSwap`, `timeout`→`defaultTimeout`, `historyEnabled`→`history`, …) or removed; unset keys are no longer serialized.
5. **URL tag helper**: a valueless `hx-get` is no longer rewritten, and `hx-get:inherited` does not trigger descendants. Also, htmx 4 changes event names, `hx-delete` form inclusion, non-200 swapping, and inheritance (`:inherited`). The antiforgery script has no automatic token refresh.
