
document.addEventListener("htmx:configRequest", (evt) => {
    let httpVerb = evt.detail.verb.toUpperCase();
    if (httpVerb === 'GET') return;
    
    let antiForgery = htmx.config.antiForgery;

    if (antiForgery) {
        
        // already specified on form, short circuit
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