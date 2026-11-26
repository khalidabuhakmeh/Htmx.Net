// htmx 1.x/2.x antiforgery integration (HTMX.NET 1.x).
// Minified with: npx terser@5.36.0 antiforgerySnippet.js -c -m -o antiforgerySnippet.min.js
(function () {
    "use strict";
    // Idempotence guard. Don't store state on document.body; boosts swap body content.
    if (window.__htmxAntiforgery) return;
    window.__htmxAntiforgery = true;

    var SAFE_METHODS = ["GET", "HEAD", "OPTIONS", "TRACE"];

    document.addEventListener("htmx:configRequest", function (evt) {
        var af = htmx.config.antiForgery;
        if (!af) return;

        var verb = String(evt.detail.verb || "get").toUpperCase();

        // SECURITY: only send the token to our own origin. Resolve against document.baseURI
        // (what htmx/XHR use), not location.href, so a <base href> can't bypass the check.
        var sameOrigin = false;
        try {
            sameOrigin = new URL(evt.detail.path, document.baseURI).origin === window.location.origin;
        } catch (e) {
            sameOrigin = false;
        }

        // Safe methods don't need the token (and would put form fields in the URL), and
        // cross-origin targets must never get it, even when a rendered form field (or
        // hx-include) would carry it. Strip the field and send nothing.
        if (SAFE_METHODS.indexOf(verb) !== -1 || !sameOrigin) {
            if (af.formFieldName) delete evt.detail.parameters[af.formFieldName];
            return;
        }

        if (!af.requestToken) return;

        if (af.headerName) {
            // Header mode (default: RequestVerificationToken). Always set the header, even if a
            // rendered form already carries the token (this is the fix for the hx-delete 400).
            evt.detail.headers[af.headerName] = af.requestToken;
            // Drop the form copy so htmx 2 can't serialize it into the query string for DELETE.
            if (af.formFieldName) delete evt.detail.parameters[af.formFieldName];
            return;
        }

        if (!af.formFieldName) return;

        // Form-field mode. Never put the token in a URL: if htmx will send this verb's
        // parameters in the query string (htmx 2 default: GET and DELETE), refuse.
        var urlParamMethods = (htmx.config.methodsThatUseUrlParams || ["get"]).map(function (m) {
            return String(m).toUpperCase();
        });
        if (urlParamMethods.indexOf(verb) !== -1) {
            delete evt.detail.parameters[af.formFieldName];
            console.warn("[htmx.net] Antiforgery token not sent for " + verb +
                ": htmx sends its parameters in the URL. Configure an antiforgery header name, " +
                "or set htmx.config.methodsThatUseUrlParams = [\"get\"].");
            return;
        }

        if (!evt.detail.parameters[af.formFieldName]) {
            evt.detail.parameters[af.formFieldName] = af.requestToken;
        }
    });

    // No automatic token refresh from htmx responses. Refreshing by parsing boosted response
    // text was removed: the response body can't prove it is a genuine full-page navigation, so
    // attacker-influenced fragments could poison htmx.config.antiForgery.
    // Tokens refresh on full page loads. If you use hx-boost AND the user's identity changes
    // mid-session (sign-in/sign-out), respond with HX-Refresh: true after that change
    // (e.g. Response.Htmx(h => h.Refresh())).
})();
