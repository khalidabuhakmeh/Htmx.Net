# Antiforgery snippet (htmx 1.x / 2.x)

`antiforgerySnippet.js` is the source. `antiforgerySnippet.min.js` is generated from it. Both are embedded
resources (`AntiforgeryJavaScript` and `AntiforgeryJavaScriptMinified`), served by
`app.MapHtmxAntiforgeryScript()` and emitted by `Html.HtmxAntiforgeryScript()`.

After **any** edit to the source, regenerate the minified file:

```bash
npx terser@5.36.0 antiforgerySnippet.js -c -m -o antiforgerySnippet.min.js
```

`test/Htmx.Tests/TagHelpers/HtmxSnippetsTests.cs` checks both resources for the guards below, so a stale
`.min.js` fails the build.

## Security notes

1. **Same-origin only.** The token is attached only when `new URL(evt.detail.path, document.baseURI).origin`
   equals `window.location.origin`. Resolve against `document.baseURI` (what XHR uses), not `location.href`,
   so a `<base href>` can't push a relative URL cross-origin. Don't rely on htmx's `selfRequestsOnly`. It
   defaults to `false` in htmx 1.x, and apps can turn it off in 2.x.
2. **Safe methods are skipped.** `GET`, `HEAD`, `OPTIONS`, and `TRACE` never carry the token.
3. **Never in a URL.** htmx 2 sends parameters for the verbs in `htmx.config.methodsThatUseUrlParams` (default
   `["get", "delete"]`) as a query string. In header mode the snippet always sets the header and deletes any
   rendered form field from `evt.detail.parameters`. In form-field mode it refuses to send the token for those
   verbs and logs a warning.
4. **No response-text token refresh.** Earlier versions parsed boosted responses with `DOMParser` and replaced
   `htmx.config.antiForgery` with whatever `htmx-config` meta they found. That allowed token-config poisoning
   from attacker-influenced fragments, so it was removed. Tokens refresh on full page loads. After a
   sign-in or sign-out under `hx-boost`, respond with `HX-Refresh: true`.
5. **Idempotent.** The guard is `window.__htmxAntiforgery`, not an attribute on `document.body`.
