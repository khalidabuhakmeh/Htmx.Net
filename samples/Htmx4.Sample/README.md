# Htmx4.Sample

Razor Pages + MVC sample for `Htmx` and `Htmx.TagHelpers` on htmx 4.0.0 (vendored at `wwwroot/lib/htmx/htmx.min.js`).
Run: `dotnet run --project samples/Htmx4.Sample`. Every demo is linked from `/` and uses `data-testid` hooks (prefix per page, e.g. `rh-`, `resp-`, `trg-`, `cfg-`, `url-`, `inh-`, `af-`, `boost-`, `part-`, `morph-`, `err-`/`del-`, `loc-`, `poll-`).

Fragments live in `Controllers/DemoController.cs` (and `AntiforgeryDemoController`), `Areas/Admin/Controllers/AdminController.cs`, and page handlers.

## Demo pages

| Page | Proves |
|---|---|
| RequestHeaders | request header props, source/target element refs, Full/Partial, key constants |
| ResponseHeaders | each response helper + every `HtmxSwap` constant via `Reswap` |
| Triggers | `WithTrigger` (name, scalar, object, multiple, merge, duplicate) |
| Config | config tag helper keys, `htmx.config` read-back, defaultSwap/noSwap effects |
| UrlHelper | url tag helper matrix incl. `hx-get:inherited`, `data-hx-get` |
| Inherited | `:inherited` vs not, headers tag helper `hx-headers-inherited` |
| Antiforgery | header + form mode for POST/PUT/PATCH/DELETE, GET control, validation attributes (cross-origin/redirect: D5) |
| Boosted | `hx-boost:inherited`, `HX-Boosted` detection, boosted form POST (+ `BoostedOther`) |
| Partials | `<hx-partial>` multi-target, main-first ordering, OOB, partials-only |
| Morph | `OuterMorph`/`InnerMorph` vs plain swap |
| ErrorSwaps | 422/500 swap, 204/304 do not, `hx-delete` + `hx-include` |
| Location | `HtmxLocation` object overload client navigation |
| Polling | load polling + `every 1s`, stop by omitting the poller |

## Public API checklist (package `Htmx`)

| Member | Page(s) |
|---|---|
| `HttpRequest.IsHtmx()` / `IsHtmx(out values)` | RequestHeaders, Boosted, Location, DemoController |
| `IsHtmxHistoryRestoreRequest()` | RequestHeaders |
| `IsHtmxPartial()` / `IsHtmxFullRequest()` | RequestHeaders |
| `IsHtmxBoosted()` / `IsHtmxNonBoosted()` (both overloads) | RequestHeaders, Boosted |
| `HttpResponse.Htmx(Action<HtmxResponseHeaders>)` | ResponseHeaders, Triggers, Morph, Location |
| `HttpResponse.StopPolling()` | Polling (obsolete in v2: demonstrated by omission; not called) |
| `HtmxRequestHeaders.CurrentUrl` | RequestHeaders |
| `.HistoryRestoreRequest` | RequestHeaders |
| `.Prompt` | RequestHeaders |
| `.Target` / `.TargetElement` | RequestHeaders, Inherited |
| `.Source` / `.SourceElement` | RequestHeaders |
| `.RequestType` | RequestHeaders, Boosted |
| `.Boosted` | RequestHeaders |
| `.Trigger` / `.TriggerName` (throw `NotSupportedException`, htmx 4 removed) | not exercised (obsolete, would throw) |
| `HtmxRequestHeaders.Keys.*` / `.All` | RequestHeaders |
| `HtmxElementReference.TagName` / `.Id` / `.DecodedId` / `.Raw` / `TryParse` | RequestHeaders (`TryParse` used by header parsing) |
| `HtmxRequestType` (`Full`, `Partial`) | RequestHeaders |
| `HtmxResponseHeaders.PushUrl` | ResponseHeaders |
| `.PreventPush` | ResponseHeaders |
| `.ReplaceUrl` / `.PreventReplace` | ResponseHeaders |
| `.Redirect` | ResponseHeaders |
| `.Refresh` | ResponseHeaders |
| `.Retarget` | ResponseHeaders |
| `.Reswap` | ResponseHeaders, Morph |
| `.Reselect` | ResponseHeaders |
| `.Location(string)` / `.Location(HtmxLocation)` | ResponseHeaders, Location |
| `.WithVary()` / `.WithVary(params string[])` | ResponseHeaders (push-url, prevent-push) |
| `.WithTrigger(name)` / `(name, detail)` | Triggers |
| `.WithTrigger(name, detail, HtmxTriggerTiming)` + `HtmxTriggerTiming` | obsolete (error) in v2; not used |
| `HtmxResponseHeaders.Keys.*` / `.All` | ResponseHeaders |
| `HtmxSwap.*` (all 16 constants) | ResponseHeaders (matrix via `/demo/swap/{style}`), Morph, Location, Partials |
| `HtmxLocation.Path` / `Target` / `Swap` / `Select` / `SelectOob` / `Values` / `Headers` / `Transition` | Location |
| `HtmxLocation.Source` / `Event` | Location (`loc-btn-header-only` shows the serialized header; not used to navigate) |

## Public API checklist (package `Htmx.TagHelpers`)

| Member | Page(s) |
|---|---|
| `HtmxConfigMetaTagHelper` (`<meta name="htmx-config">`): `logAll`, `prefix`, `metaCharacter`, `transitions`, `history`, `mode`, `defaultSwap`, `allowEmptySwapAfterOOB`, `defaultFocusScroll`, `defaultSettleDelay`, `indicatorClass`, `requestClass`, `includeIndicatorCSS`, `defaultTimeout`, `extensions`, `morphIgnore`, `morphSkip`, `morphSkipChildren`, `morphScanLimit`, `noSwap`, `implicitInheritance`, `inlineScriptNonce` | Config |
| `includeAspNetAntiforgeryToken` | Layout (all pages), Antiforgery, Config |
| `HtmxUrlTagHelper`: `hx-page`, `hx-page-handler`, `hx-controller`, `hx-action`, `hx-area`, `hx-route`, `hx-route-{values}`, `hx-fragment` | UrlHelper |
| `hx-host`, `hx-protocol` | UrlHelper (`url-btn-host`, inert) |
| verb spellings `hx-get`, `hx-get:inherited`, `data-hx-get`, `hx-post` | UrlHelper |
| `HtmxHeadersTagHelper`: `hx-headers-*`, `hx-headers-inherited` | Inherited |
| `HtmxAntiforgeryScriptEndpoints.MapHtmxAntiforgeryScript` / `.Path` | Program.cs, Layout |
| `HtmlExtensions.HtmxAntiforgeryScript(IHtmlHelper)` | Antiforgery (`af-inline-length`, rendered to a string, not emitted) |
| `HtmxSnippets.AntiforgeryJavaScript` / `AntiforgeryJavaScriptMinified` / `GetAntiforgeryJavaScript` | served via the endpoint (Layout) |

## Notes
- The only API members not exercised are the `[Obsolete(error: true)]` ones (`StopPolling`, `HtmxTriggerTiming`, `Trigger`/`TriggerName`), which cannot compile or would throw.
- Cross-origin, `<base href>`, redirect and token-rotation antiforgery scenarios are proven in D5.
