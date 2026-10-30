# JavaScript snippets

`antiforgerySnippet.min.js` is generated from `antiforgerySnippet.js` with:

    npx terser@5.36.0 antiforgerySnippet.js -c -m -o antiforgerySnippet.min.js

Keep a one-line header comment with this command at the top of the `.min.js` after regenerating
(terser strips comments). Always regenerate the `.min.js` after editing the source and verify both
files encode identical logic (especially the same-origin guard and safe-method check).

## Antiforgery (CSRF) security notes

The snippet injects the ASP.NET Core antiforgery token into non-safe (`POST`/`PUT`/`PATCH`/`DELETE`)
htmx requests. It does not auto-refresh the token from responses (see item 4). Security properties are
enforced and residual limitations are documented below. These were raised in a security audit (warp) of
the htmx 4 rewrite and are proven by the D5 Playwright E2E tests.

1. **Safe methods are skipped.** `GET`/`HEAD`/`OPTIONS`/`TRACE` never receive the token.

2. **Same-origin guard.** The token is only attached when the request's target resolves to the page
   origin. The target is resolved against `document.baseURI` — the same base URL `fetch()` uses — so
   a `<base href>` element cannot slip a relative action cross-origin while passing the guard.
   (Resolving against `window.location.href` instead would be bypassable; do not change this.)

3. **Residual limitation — same-origin → cross-origin redirects.** The guard checks the *initial*
   request URL only. If the consuming application exposes a **same-origin endpoint that 307/308
   redirects to another origin** and htmx follows that redirect, the browser re-sends the
   token-bearing request to the foreign origin. **This only happens when the app sets
   `htmx.config.mode: "cors"`**; the DEFAULT `mode: "same-origin"` blocks the redirect and the token
   does not leak (proven by D5). When opted in to `cors`, **both modes can expose the token:**
   - **Form-field mode** (body injection): a 307/308 cross-origin redirect re-sends the POST body,
     including the token.
   - **Header mode** (`headerName` set): a custom request header survives a cross-origin redirect
     when the destination's CORS preflight permits the victim origin and the header — and an
     **attacker-controlled destination can choose to permit exactly that**. Header mode is therefore
     NOT a reliable mitigation against an attacker-controlled redirect target.

   The snippet cannot close this alone because htmx 4 owns the `fetch()` call and its `redirect`
   mode. **Mitigations for consuming apps:** do not expose same-origin open redirects; rotate
   antiforgery tokens server-side; validate `Origin`/`Referer` server-side on state-changing
   requests. The D5 E2E suite proves base-URL resolution and the redirect behavior in both script
   variants — including an attacker endpoint that permits the preflight — before F3 accepts the
   residual risk.

4. **Token rotation — no automatic refresh (by design).** The snippet does **not** try to refresh
   the antiforgery token from htmx responses. htmx 4 boost swaps only `<body>` and never merges
   `<head>`, so a rotated token in a boosted response's `<meta name="htmx-config">` never reaches the
   DOM; and refreshing by parsing response text cannot be done safely — the response body cannot be
   trusted to signal a genuine full-page navigation, so gating on it would allow token-configuration
   poisoning from attacker-influenced fragment/OOB responses or cross-origin (`mode:"cors"`) responses
   (a security audit rejected every response-text-based approach). **Guidance:** antiforgery tokens
   refresh naturally on full page loads. If you use `hx-boost` **and** rotate the token mid-session
   (e.g. on sign-in/sign-out), trigger a full page reload (for example via the `HX-Refresh` response
   header) after that state change rather than relying on an auto-refresh.

5. **DELETE requires header mode.** For DELETE requests, use header-mode antiforgery (the default when
   a header name is configured). Form-field mode does not work for DELETE because htmx 4 serializes
   DELETE form data into the query string, which ASP.NET antiforgery validation does not inspect.
