if (!document.body.attributes.__htmx_antiforgery) {
    document.addEventListener("htmx:configRequest", evt => {
        let httpVerb = evt.detail.verb.toUpperCase();
        if (httpVerb === 'GET') return;
        let antiForgery = htmx.config.antiForgery;
        if (antiForgery) {
            // already specified on the form, short circuit
            if (evt.detail.parameters[antiForgery.formFieldName])
                return;

            if (antiForgery.headerName) {
                evt.detail.headers[antiForgery.headerName]
                    = antiForgery.requestToken;
            } else {
                evt.detail.parameters[antiForgery.formFieldName]
                    = antiForgery.requestToken;
            }
        }
    });
    document.addEventListener("htmx:afterOnLoad", evt => {
        if (evt.detail.boosted) {
            const parser = new DOMParser();
            const html = parser.parseFromString(evt.detail.xhr.responseText, 'text/html');
            const selector = 'meta[name=htmx-config]';
            const config = html.querySelector(selector);
            if (config) {
                const current = document.querySelector(selector);
                // only change the anti-forgery token
                const key = 'antiForgery';
                htmx.config[key] = JSON.parse(config.attributes['content'].value)[key];
                // update DOM, probably not necessary, but for sanity's sake
                current.replaceWith(config);
            }
        }
    });
    document.body.attributes.__htmx_antiforgery = true;
}