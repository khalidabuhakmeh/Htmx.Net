// htmx 4 antiforgery integration. Minified with: npx terser@5.36.0 antiforgerySnippet.js -c -m
(function () {
    "use strict";
    // Idempotence guard (do not mutate document.body attributes; fragile on boost).
    if (window.__htmxAntiforgery) return;
    window.__htmxAntiforgery = true;

    const SAFE_METHODS = ["GET", "HEAD", "OPTIONS", "TRACE"];

    document.addEventListener("htmx:config:request", evt => {
        const ctx = evt.detail && evt.detail.ctx;
        const request = ctx && ctx.request;
        if (!request) return;

        const method = (request.method || "GET").toUpperCase();
        if (SAFE_METHODS.indexOf(method) !== -1) return;

        const af = htmx.config.antiForgery;
        if (!af || !af.requestToken) return;

        // SECURITY: only attach the token to same-origin requests, never leak it cross-origin.
        // Resolve the action the way fetch() will — against the document base URI, which a
        // <base href> can change — then require the resolved origin to equal the page origin.
        // Resolving against window.location.href instead would let a <base> tag slip a relative
        // action cross-origin while still passing this guard.
        let sameOrigin = false;
        try {
            sameOrigin = new URL(request.action, document.baseURI).origin === window.location.origin;
        } catch (e) {
            sameOrigin = false;
        }
        if (!sameOrigin) return;

        // NOTE: af.requestToken is HtmlAttributeEncoded server-side; the browser decodes the
        // attribute when parsing <meta name="htmx-config">, so the value read here is already
        // plain text. Do NOT decode again.
        if (af.headerName) {
            request.headers[af.headerName] = af.requestToken;
        } else if (af.formFieldName) {
            // Form-field mode. Assumes htmx 4 encodes a FormData assigned to request.body
            // when it is null (GET/DELETE are null pre-encoding); verified by E2E tests.
            // NOTE: DELETE will not validate in form mode (htmx 4 puts DELETE data in the query string).
            if (!request.body) request.body = new FormData();
            // Token already present (e.g. from a rendered form): leave it alone.
            if (request.body.has(af.formFieldName)) return;
            request.body.set(af.formFieldName, af.requestToken);
        }
    });

    // NOTE: No automatic token refresh from htmx responses.
    // htmx 4 boost swaps only <body> and never merges <head>, so a rotated token in a boosted
    // response's <meta name="htmx-config"> does not reach the DOM. Refreshing the token by parsing
    // response text cannot be done safely — the response body itself cannot be trusted to signal a
    // genuine full-page navigation, and gating on it enables token-configuration poisoning from
    // attacker-influenced fragments/OOB or cross-origin (mode:"cors") responses. See JavaScript/README.md.
    // Token rotation guidance for consuming apps: antiforgery tokens refresh naturally on full page
    // loads. If you use hx-boost AND rotate the token mid-session (e.g. on sign-in/sign-out), trigger
    // a full page reload (e.g. HX-Refresh) after that state change rather than relying on auto-refresh.
})();
