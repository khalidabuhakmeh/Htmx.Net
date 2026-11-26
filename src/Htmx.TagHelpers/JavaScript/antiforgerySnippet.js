// htmx 4 antiforgery integration. Minified with: npx terser@5.36.0 antiforgerySnippet.js -c -m
(function () {
    "use strict";
    // Idempotence guard (do not mutate document.body attributes; fragile on boost).
    if (window.__htmxAntiforgery) return;
    window.__htmxAntiforgery = true;

    const SAFE_METHODS = ["GET", "HEAD", "OPTIONS", "TRACE"];
    // Mirrors htmx 4.0.0's own (unanchored) test for verbs whose values go into the URL query
    // string, so custom methods such as "XGET" are covered too. Keep in sync with htmx.
    const USES_URL_PARAMS = /GET|DELETE/;

    document.addEventListener("htmx:config:request", evt => {
        const ctx = evt.detail && evt.detail.ctx;
        const request = ctx && ctx.request;
        if (!request) return;

        const af = htmx.config.antiForgery;
        if (!af) return;

        const method = (request.method || "GET").toUpperCase();

        // At this point htmx 4 has already collected form values (including a server-rendered
        // __RequestVerificationToken hidden input) into request.body, a FormData, for EVERY verb.
        // After this event it moves GET/DELETE values into the URL query string.
        const stripFormField = () => {
            if (af.formFieldName && request.body && typeof request.body.delete === "function") {
                request.body.delete(af.formFieldName);
            }
        };

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

        // Safe methods don't need the token (and would put form fields in the URL); cross-origin
        // targets must never receive it, even when a rendered form field or hx-include carries it.
        if (SAFE_METHODS.indexOf(method) !== -1 || !sameOrigin) {
            stripFormField();
            return;
        }

        if (!af.requestToken) return;

        // NOTE: af.requestToken is HtmlAttributeEncoded server-side; the browser decodes the
        // attribute when parsing <meta name="htmx-config">, so the value read here is already
        // plain text. Do NOT decode again.
        if (af.headerName) {
            request.headers[af.headerName] = af.requestToken;
            // The header is authoritative. Drop the form copy so a DELETE can't serialize it
            // into the query string.
            stripFormField();
            return;
        }

        if (!af.formFieldName) return;

        // Form-field mode. Never put the token in a URL: htmx 4 sends values for any method matching
        // /GET|DELETE/ as query parameters, so refuse those (fail closed).
        if (USES_URL_PARAMS.test(method)) {
            stripFormField();
            console.warn("[htmx.net] Antiforgery token not sent for " + method +
                ": htmx 4 sends its values in the URL. Configure an antiforgery header name " +
                "(the ASP.NET Core default) to protect " + method + " requests.");
            return;
        }

        if (!request.body) request.body = new FormData();
        // Token already present (e.g. from a rendered form): leave it alone.
        if (request.body.has(af.formFieldName)) return;
        request.body.set(af.formFieldName, af.requestToken);
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
